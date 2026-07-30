<#
    Publie l'application (self-contained, single-file, win-x64) puis compile l'installeur Inno Setup.
    Resultat: installer\Output\SteelSeriesResourceMonitor-Setup.exe
#>

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$installerDir = Join-Path $root "installer"
$publishDir = Join-Path $installerDir "app"
$csproj = Join-Path $root "SteelSeries.SysMonitor.csproj"

if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

Write-Host "Publication de l'application (self-contained, win-x64)..." -ForegroundColor Cyan
dotnet publish $csproj -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o $publishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish a echoue." }

$iscc = Get-Command ISCC.exe -ErrorAction SilentlyContinue
if (-not $iscc) {
    $candidates = @(
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
    )
    $found = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $found) { throw "ISCC.exe introuvable. Installe Inno Setup 6 (winget install JRSoftware.InnoSetup)." }
    $iscc = $found
} else {
    $iscc = $iscc.Source
}

Write-Host "Compilation de l'installeur..." -ForegroundColor Cyan
& $iscc (Join-Path $installerDir "setup.iss")
if ($LASTEXITCODE -ne 0) { throw "ISCC a echoue." }

Write-Host "Installeur genere dans installer\Output\" -ForegroundColor Green
