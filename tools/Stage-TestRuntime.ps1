param(
 [string]$MediaPortalDirectory='C:\Program Files\Team MediaPortal\MediaPortal',
 [string]$MediaPortalData='C:\ProgramData\Team MediaPortal\MediaPortal',
 [string]$Destination=(Join-Path $PSScriptRoot '../../runtime'),
 [switch]$IncludeWritableAssets
)
$ErrorActionPreference='Stop'
$Destination=[IO.Path]::GetFullPath($Destination)
$base=Join-Path $Destination 'base'
New-Item -ItemType Directory -Path $base -Force | Out-Null
Get-ChildItem -LiteralPath $MediaPortalDirectory -File | Copy-Item -Destination $base -Force
Get-ChildItem -LiteralPath $MediaPortalDirectory -Directory | Copy-Item -Destination $base -Recurse -Force
foreach($name in @('config','log','database','thumbs','cache')){New-Item -ItemType Directory -Path (Join-Path $Destination $name) -Force | Out-Null}
Get-ChildItem -LiteralPath $MediaPortalData -File | Where-Object {$_.Extension -in @('.xml','.config','.ini')} | Copy-Item -Destination (Join-Path $Destination 'config') -Force
# Native movie database activation expects scripts below Config, and installed
# plugins can read their auxiliary configuration from these subdirectories.
foreach($name in @('scripts','Sounds','MP-TVSeries','Trakt','FanartHandler','IMDb+')){
 $auxiliary=Join-Path $MediaPortalData $name
 if(Test-Path -LiteralPath $auxiliary){Copy-Item -LiteralPath $auxiliary -Destination (Join-Path $Destination 'config') -Recurse -Force}
}
if(Test-Path -LiteralPath (Join-Path $MediaPortalData 'database')){Get-ChildItem -LiteralPath (Join-Path $MediaPortalData 'database') -File | Copy-Item -Destination (Join-Path $Destination 'database') -Force}
# Point mutable directories at this workspace copy. Skin/language are read-only references during UI testing.
$doc=New-Object Xml.XmlDocument
$doc.Load((Join-Path $MediaPortalDirectory 'MediaPortalDirs.xml'))
$dirs=@{Config=(Join-Path $Destination 'config');Log=(Join-Path $Destination 'log');Database=(Join-Path $Destination 'database');Thumbs=(Join-Path $Destination 'thumbs');Cache=(Join-Path $Destination 'cache');Skin=(Join-Path $MediaPortalData 'skin');Language=(Join-Path $MediaPortalData 'language');Plugins=(Join-Path $base 'plugins')}
if($IncludeWritableAssets){
 foreach($name in @('skin','language')){
  $target=Join-Path $Destination $name
  New-Item -ItemType Directory -Path $target -Force | Out-Null
  Copy-Item -Path (Join-Path (Join-Path $MediaPortalData $name) '*') -Destination $target -Recurse -Force
 }
 $dirs.Skin=Join-Path $Destination 'skin';$dirs.Language=Join-Path $Destination 'language'
}
foreach($node in $doc.SelectNodes('/Config/Dir')){$id=$node.GetAttribute('id');if($dirs.ContainsKey($id)){$node.Path=$dirs[$id]+'\'}}
$doc.Save((Join-Path $base 'MediaPortalDirs.xml'))
Set-Content -LiteralPath (Join-Path $base 'studio-test-runtime.marker') -Value 'Isolated Configuration Studio test runtime; mutable directories must remain inside this runtime.' -Encoding UTF8
if(Test-Path -LiteralPath (Join-Path $PSScriptRoot 'persistence-policy.xml')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'persistence-policy.xml') -Destination (Join-Path $Destination 'persistence-policy.xml') -Force}
Get-FileHash -LiteralPath (Join-Path $MediaPortalDirectory 'Configuration.exe'),(Join-Path $MediaPortalData 'MediaPortal.xml') | Select-Object Path,Hash | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $Destination 'original-hashes-before.json') -Encoding UTF8
Write-Output $base
