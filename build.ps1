$ErrorActionPreference = 'Stop'

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$Source = Join-Path $Root 'src\Program.cs'
$Dist = Join-Path $Root 'dist'
$Out = Join-Path $Dist 'AITrainingStudio.exe'

New-Item -ItemType Directory -Force -Path $Dist | Out-Null

$Csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $Csc)) {
    $Csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}

if (-not (Test-Path $Csc)) {
    throw 'Cannot find csc.exe. Install .NET Framework 4.x or Visual Studio Build Tools.'
}

& $Csc `
    /nologo `
    /codepage:65001 `
    /target:winexe `
    /platform:anycpu `
    /reference:System.dll `
    /reference:System.Core.dll `
    /reference:System.Drawing.dll `
    /reference:System.Windows.Forms.dll `
    /out:$Out `
    $Source

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "Built: $Out"
