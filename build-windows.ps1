$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectPath = Join-Path $repoRoot 'src\AudioMetadataManager.UI\AudioMetadataManager.UI.csproj'
$publishDir = Join-Path $repoRoot 'publish\AudioMetadataManager'
$innoScript = Join-Path $repoRoot 'installer\AudioMetadataManager.iss'
$innoCompiler = Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe'

dotnet restore $projectPath

dotnet publish $projectPath -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $publishDir

Write-Host "Publicado en $publishDir" -ForegroundColor Green

if (Test-Path $innoCompiler) {
    & $innoCompiler $innoScript
    Write-Host "Instalador generado en installer\Output\AudioMetadataManager-Setup.exe" -ForegroundColor Green
} else {
    Write-Host "Inno Setup no está instalado; se omitió la generación del instalador." -ForegroundColor Yellow
    Write-Host "Instalalo con: winget install --id JRSoftware.InnoSetup -e" -ForegroundColor Yellow
}
