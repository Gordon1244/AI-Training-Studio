$ErrorActionPreference = 'Stop'

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$App = Join-Path $Root 'dist\AITrainingStudio.exe'

Write-Host 'AI Training Studio Launcher'
Write-Host ''
Write-Host "App path: $App"
Write-Host ''

if (-not (Test-Path -LiteralPath $App)) {
    Write-Host 'ERROR: Cannot find the exe file.'
    Write-Host 'Please check the dist folder or run build.ps1 again.'
    Read-Host 'Press Enter to exit'
    exit 1
}

Write-Host 'Starting app...'
$p = Start-Process -FilePath $App -PassThru
Start-Sleep -Seconds 2

if ($p.HasExited) {
    Write-Host "ERROR: The app exited immediately. Exit code: $($p.ExitCode)"
    Read-Host 'Press Enter to exit'
    exit 1
}

Write-Host "OK: App is running. PID: $($p.Id)"
Read-Host 'Press Enter to close this launcher'
