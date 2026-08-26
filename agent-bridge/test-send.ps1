# Sends a fake Flowtype ask to the daemon, with the same token header Flowtype uses.
param(
    [int]$Port = 5599,
    [string]$Text = 'Say hello and tell me what directory you are in.'
)

$tokenPath = Join-Path $env:APPDATA 'Flowtype\agent-token'
if (-not (Test-Path -LiteralPath $tokenPath)) {
    throw "No agent token at $tokenPath. Start flowtype_agentd.py (or start-agent.ps1) first so it can mint one."
}
$token = (Get-Content -LiteralPath $tokenPath -Raw).Trim()
$body = @{ text = $Text; source = 'test-send'; sentUtc = (Get-Date).ToUniversalTime().ToString('o') } | ConvertTo-Json
$headers = @{ 'X-Flowtype-Token' = $token }
$response = Invoke-RestMethod -Uri "http://127.0.0.1:$Port/ask" -Method Post -ContentType 'application/json; charset=utf-8' -Headers $headers -Body $body
$response | ConvertTo-Json
