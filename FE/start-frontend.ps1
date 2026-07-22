param(
    [ValidateSet("site", "portal")]
    [string]$App = "portal"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$bundledNode = Join-Path $env:USERPROFILE ".cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin"
if (-not (Get-Command node.exe -ErrorAction SilentlyContinue) -and (Test-Path -LiteralPath (Join-Path $bundledNode "node.exe"))) {
    $env:PATH = "$bundledNode;$env:PATH"
}
$pnpmCommand = Get-Command pnpm.cmd -ErrorAction SilentlyContinue

if (-not $pnpmCommand) {
    $bundledPnpm = Join-Path $env:USERPROFILE ".cache\codex-runtimes\codex-primary-runtime\dependencies\bin\fallback\pnpm.cmd"
    if (-not (Test-Path -LiteralPath $bundledPnpm)) {
        Write-Host "Khong tim thay pnpm. Cai Node.js LTS va pnpm truoc khi chay."
        exit 1
    }
    $pnpm = $bundledPnpm
}
else {
    $pnpm = $pnpmCommand.Source
}

$pnpmDirectory = Split-Path -Parent $pnpm
$env:PATH = "$pnpmDirectory;$env:PATH"

Push-Location $root
try {
    if (-not (Test-Path -LiteralPath (Join-Path $root "node_modules"))) {
        & $pnpm install
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
    & $pnpm "dev:$App"
}
finally {
    Pop-Location
}
