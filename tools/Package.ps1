param([string]$OutputRoot=(Join-Path $PSScriptRoot '../artifacts'))
$ErrorActionPreference='Stop'
$project=Split-Path $PSScriptRoot -Parent
$release=Join-Path ([IO.Path]::GetFullPath($OutputRoot)) 'MP1-Configuration-Studio-v0.1.0-alpha.1'
if(Test-Path -LiteralPath $release){throw 'Output folder already exists. Choose a clean OutputRoot.'}
New-Item -ItemType Directory -Path $release -Force | Out-Null
foreach($name in @('MP1ConfigurationStudio.exe','MP1ConfigurationStudio.x64.exe','MP1ConfigurationStudio.x86.exe')){
 Copy-Item -LiteralPath (Join-Path $project ('dist/'+$name)) -Destination $release
 Copy-Item -LiteralPath (Join-Path $project ('dist/'+$name+'.config')) -Destination $release
}
foreach($name in @('README.md','VALIDATION.md','LICENSE')){Copy-Item -LiteralPath (Join-Path $project $name) -Destination $release}
$archive=$release+'.zip'
Compress-Archive -Path (Join-Path $release '*') -DestinationPath $archive
$hash=(Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
Set-Content -LiteralPath (Join-Path (Split-Path $release -Parent) 'SHA256SUMS.txt') -Value ($hash+'  '+(Split-Path $archive -Leaf)) -Encoding ASCII
Write-Output $archive
