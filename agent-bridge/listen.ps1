# Flowtype agent-bridge listener (PoC)
# Receives POSTs from Flowtype's Agent chord on 127.0.0.1 and hands each ask to a
# local agent runtime (default: Claude Code CLI). One ask = one new console window,
# so the listener is never busy and Flowtype never sees "agent offline" mid-run.
#
# Usage:  powershell -ExecutionPolicy Bypass -File listen.ps1
#         powershell -File listen.ps1 -Port 5599 -Agent claude -AgentFlags ''
# Stop:   Ctrl+C

param(
    [int]$Port = 5599,
    [string]$Agent = 'claude',
    # Extra flags appended to the agent call, e.g. '--permission-mode acceptEdits'
    [string]$AgentFlags = ''
)

$ErrorActionPreference = 'Stop'
$askDir = Join-Path $PSScriptRoot 'asks'
New-Item -ItemType Directory -Force -Path $askDir | Out-Null
$logPath = Join-Path $PSScriptRoot 'bridge.log'

function Write-Log([string]$message) {
    $line = "{0}  {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $message
    Write-Host $line
    Add-Content -Path $logPath -Value $line -Encoding UTF8
}

# TcpListener instead of HttpListener: loopback TCP needs no URL ACL, no admin.
$listener = New-Object System.Net.Sockets.TcpListener ([System.Net.IPAddress]::Loopback), $Port
$listener.Start()
Write-Log "Listening on http://127.0.0.1:$Port/ask  (agent: $Agent $AgentFlags)"
Write-Log "In Flowtype: tray menu -> 'Agent chord' to enable, then hold the agent chord and speak."

$utf8 = New-Object System.Text.UTF8Encoding($false)

try {
    while ($true) {
        $client = $listener.AcceptTcpClient()
        try {
            $client.ReceiveTimeout = 5000
            $stream = $client.GetStream()

            # Read headers byte-wise until CRLFCRLF, then Content-Length bytes of body.
            $headerBytes = New-Object System.Collections.Generic.List[byte]
            $tail = New-Object 'System.Collections.Generic.Queue[byte]'
            while ($true) {
                $b = $stream.ReadByte()
                if ($b -lt 0) { break }
                $headerBytes.Add([byte]$b)
                $tail.Enqueue([byte]$b)
                if ($tail.Count -gt 4) { [void]$tail.Dequeue() }
                if ($tail.Count -eq 4) {
                    $t = $tail.ToArray()
                    if ($t[0] -eq 13 -and $t[1] -eq 10 -and $t[2] -eq 13 -and $t[3] -eq 10) { break }
                }
                if ($headerBytes.Count -gt 32768) { break }
            }
            $headerText = $utf8.GetString($headerBytes.ToArray())
            $contentLength = 0
            if ($headerText -match '(?im)^Content-Length:\s*(\d+)') { $contentLength = [int]$Matches[1] }
            $token = ''
            if ($headerText -match '(?im)^X-Flowtype-Token:\s*(\S+)') { $token = $Matches[1].Trim() }
            $hasBrowserOrigin = $headerText -match '(?im)^Origin:' -or $headerText -match '(?im)^Referer:'
            $contentType = ''
            if ($headerText -match '(?im)^Content-Type:\s*(.+)$') { $contentType = $Matches[1].Trim() }
            $hostHeader = ''
            if ($headerText -match '(?im)^Host:\s*(.+)$') { $hostHeader = $Matches[1].Trim() }
            $loopbackHost = $hostHeader -match '^(127\.0\.0\.1|localhost|\[::1\])(:\d+)?$'

            $expectedToken = ''
            $tokenPath = Join-Path $env:APPDATA 'Flowtype\agent-token'
            if (Test-Path -LiteralPath $tokenPath) {
                $expectedToken = (Get-Content -LiteralPath $tokenPath -Raw).Trim()
            }
            if ($hasBrowserOrigin -or -not $loopbackHost -or [string]::IsNullOrWhiteSpace($expectedToken) -or $token -ne $expectedToken -or $contentType -notmatch 'json') {
                $deny = '{"status":"error","error":"refused"}'
                $denyBytes = $utf8.GetBytes($deny)
                $denyResponse = "HTTP/1.1 403 Forbidden`r`nContent-Type: application/json`r`nConnection: close`r`nContent-Length: $($denyBytes.Length)`r`n`r`n$deny"
                $denyResponseBytes = $utf8.GetBytes($denyResponse)
                $stream.Write($denyResponseBytes, 0, $denyResponseBytes.Length)
                $stream.Flush()
                Write-Log 'REFUSED ask (token / origin / host / content-type)'
                continue
            }

            $bodyBytes = New-Object byte[] $contentLength
            $read = 0
            while ($read -lt $contentLength) {
                $n = $stream.Read($bodyBytes, $read, $contentLength - $read)
                if ($n -le 0) { break }
                $read += $n
            }
            $body = $utf8.GetString($bodyBytes, 0, $read)

            $text = ''
            try {
                $json = $body | ConvertFrom-Json
                if ($json -and $json.text) { $text = [string]$json.text }
            } catch { $text = $body }

            $responseBody = '{"status":"accepted"}'
            $responseBodyBytes = $utf8.GetBytes($responseBody)
            $response = "HTTP/1.1 200 OK`r`nContent-Type: application/json`r`nConnection: close`r`nContent-Length: $($responseBodyBytes.Length)`r`n`r`n$responseBody"
            $responseBytes = $utf8.GetBytes($response)
            $stream.Write($responseBytes, 0, $responseBytes.Length)
            $stream.Flush()

            if ([string]::IsNullOrWhiteSpace($text)) {
                Write-Log 'Received empty ask; ignored.'
            } else {
                $stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
                $askFile = Join-Path $askDir "ask-$stamp.txt"
                [IO.File]::WriteAllText($askFile, $text, $utf8)
                $previewLength = [Math]::Min(100, $text.Length)
                Write-Log ("ASK: " + $text.Substring(0, $previewLength))
                # New console per ask: the agent's own permission prompts and output live
                # there, and this listener is free for the next take immediately.
                $inner = "`$ask = Get-Content -Raw '$askFile'; Write-Host '=== FLOWTYPE ASK ==='; Write-Host `$ask; Write-Host '===================='; & $Agent -p `$ask $AgentFlags"
                Start-Process -FilePath 'powershell' -ArgumentList @('-NoExit', '-ExecutionPolicy', 'Bypass', '-Command', $inner)
            }
        } catch {
            Write-Log ("Request error: " + $_.Exception.Message)
        } finally {
            $client.Close()
        }
    }
} finally {
    $listener.Stop()
}
