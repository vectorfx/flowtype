# Start the Flowtype agent daemon on loopback so this machine's Flowtype can talk to
# the CLI you already use. Never binds the LAN. Other people run this on their PC.
param(
    [ValidateSet('notes', 'commit', 'full')]
    [string]$Profile = 'notes',
    [string]$Cli = '',
    [string]$Model = '',
    [int]$Port = 5599,
    [string]$WorkingDirectory = $HOME
)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location -LiteralPath $here

$python = Get-Command python -ErrorAction SilentlyContinue
if (-not $python) { $python = Get-Command py -ErrorAction SilentlyContinue }
if (-not $python) {
    throw "Python is not on PATH. Install Python 3, then: pip install claude-agent-sdk"
}

Write-Host "Flowtype agent — loopback only, token-gated."
Write-Host "Profile: $Profile   Port: $Port   Cwd: $WorkingDirectory"
Write-Host "In Flowtype: Settings → Agent → enable, endpoint http://127.0.0.1:$Port/ask"
Write-Host ""

$daemonArgs = @(
    (Join-Path $here 'flowtype_agentd.py'),
    '--port', "$Port",
    '--profile', $Profile,
    '--cwd', $WorkingDirectory
)
if (-not [string]::IsNullOrWhiteSpace($Cli)) {
    $daemonArgs += @('--cli', $Cli)
    Write-Host "CLI: $Cli"
}
if (-not [string]::IsNullOrWhiteSpace($Model)) {
    $daemonArgs += @('--model', $Model)
    Write-Host "Model: $Model"
}

& $python.Source @daemonArgs
