$ErrorActionPreference = 'Stop'
dotnet restore .\AudioMetadataManager.sln
dotnet test .\AudioMetadataManager.sln -c Release
dotnet publish .\src\AudioMetadataManager\AudioMetadataManager.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o .\publish\AudioMetadataManager
Write-Host "Publicado en publish\AudioMetadataManager" -ForegroundColor Green
