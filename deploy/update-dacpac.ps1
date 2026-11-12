# À lancer sous Windows (Visual Studio avec SSDT) après toute modification du schéma (projet AmetisAPI.Database) :
# reconstruit le DACPAC et le copie dans deploy/schema, utilisé par « ./deploy.sh update » sur le serveur.
$ErrorActionPreference = 'Stop'
$v_Root = Split-Path -Parent $PSScriptRoot
$v_MsBuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
& $v_MsBuild "$v_Root\AmetisAPI\AmetisAPI\src\AmetisAPI.Database\AmetisAPI.Database.sqlproj" -nologo -v:m -p:Configuration=Release
if ($LASTEXITCODE -ne 0) { throw "Échec de la construction du DACPAC" }
Copy-Item "$v_Root\AmetisAPI\AmetisAPI\src\AmetisAPI.Database\bin\Release\AmetisAPI.Database.dacpac" "$PSScriptRoot\schema\AmetisAPI.Database.dacpac" -Force
Write-Host "DACPAC copié dans deploy\schema."
