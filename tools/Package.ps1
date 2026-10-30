param([string]$OutputRoot=(Join-Path $PSScriptRoot '../../../outputs'))
$ErrorActionPreference='Stop'
$project=Split-Path $PSScriptRoot -Parent
$release=Join-Path ([IO.Path]::GetFullPath($OutputRoot)) 'MP1-Configuration-Studio-ALPHA3'
$source=Join-Path $release 'source'
$evidence=Join-Path $release 'pruefung'
New-Item -ItemType Directory -Path $release,$source,$evidence -Force | Out-Null
foreach($file in @('MP1ConfigurationStudio.exe','MP1ConfigurationStudio.x64.exe','MP1ConfigurationStudio.x86.exe')){
 Copy-Item -LiteralPath (Join-Path $project ('dist/'+$file)) -Destination $release -Force
 Copy-Item -LiteralPath (Join-Path $project ('dist/'+$file+'.config')) -Destination $release -Force
}
foreach($file in @('README-DE.md','ARCHITEKTUR.md','VALIDIERUNG.md','LICENSE')){Copy-Item -LiteralPath (Join-Path $project $file) -Destination $release -Force}
Copy-Item -LiteralPath (Join-Path $project 'src') -Destination $source -Recurse -Force
Copy-Item -LiteralPath (Join-Path $project 'tools') -Destination $source -Recurse -Force
foreach($file in @('build.ps1','app.config','app.manifest','MP1.ConfigurationStudio.csproj','LICENSE')){Copy-Item -LiteralPath (Join-Path $project $file) -Destination $source -Force}
Copy-Item -LiteralPath (Join-Path $project 'dist/test-results.log') -Destination $evidence -Force
$runtime=Join-Path $project '../runtime'
Copy-Item -LiteralPath (Join-Path $runtime 'native-checks-alpha3.xml') -Destination (Join-Path $evidence 'native-checks.xml') -Force
Copy-Item -LiteralPath (Join-Path $runtime 'native-inventory-alpha3.xml') -Destination (Join-Path $evidence 'native-inventory.xml') -Force
$integrity=@(Get-Content (Join-Path $runtime 'original-hashes-before.json') -Raw | ConvertFrom-Json | Where-Object {(Split-Path $_.Path -Leaf) -eq 'Configuration.exe'} | ForEach-Object {
 $after=Get-FileHash -LiteralPath $_.Path
 if($after.Hash -ne $_.Hash){throw "Original changed: $($_.Path)"}
 [pscustomobject]@{File=(Split-Path $_.Path -Leaf);Sha256=$_.Hash;Unchanged=$true}
})
$integrity | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $evidence 'original-integrity.json') -Encoding UTF8
Get-ChildItem -LiteralPath $release -File -Recurse | Where-Object {$_.Name -ne 'SHA256SUMS.txt'} | Sort-Object FullName | ForEach-Object {
 $hash=Get-FileHash -LiteralPath $_.FullName
 $relative=$_.FullName.Substring($release.Length+1).Replace('\','/')
 "$($hash.Hash.ToLowerInvariant())  $relative"
} | Set-Content -LiteralPath (Join-Path $release 'SHA256SUMS.txt') -Encoding ASCII
$archive=Join-Path ([IO.Path]::GetFullPath($OutputRoot)) 'MP1-Configuration-Studio-ALPHA3.zip'
Compress-Archive -Path (Join-Path $release '*') -DestinationPath $archive -CompressionLevel Optimal -Force
Write-Output $release
Write-Output $archive




