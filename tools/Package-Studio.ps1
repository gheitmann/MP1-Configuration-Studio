param(
    [string]$Version = '1.0.0',
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '..\dist\package')
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$dist = Join-Path $root 'dist'
$app = Join-Path $dist 'MP1ConfigurationStudio.exe'
$config = $app + '.config'

if (-not (Test-Path -LiteralPath $app)) {
    throw "Built application not found: $app. Run build.ps1 first."
}
if (-not (Test-Path -LiteralPath $config)) {
    throw "Application config not found: $config"
}

$version = $Version.TrimStart('v')
$name = "MP1-Configuration-Studio-$version"
$stage = Join-Path $OutputDirectory $name
$zip = Join-Path $OutputDirectory "$name-Portable.zip"
$notes = Join-Path $root 'docs\RELEASE-1.0.0.md'
if (-not (Test-Path -LiteralPath $notes)) {
    $notes = Join-Path $root 'RELEASE-NOTES.md'
}

if (Test-Path -LiteralPath $stage) {
    Remove-Item -LiteralPath $stage -Recurse -Force
}
if (Test-Path -LiteralPath $zip) {
    Remove-Item -LiteralPath $zip -Force
}
New-Item -ItemType Directory -Path $stage -Force | Out-Null

Copy-Item -LiteralPath $app -Destination $stage
Copy-Item -LiteralPath $config -Destination $stage
Copy-Item -LiteralPath (Join-Path $root 'LICENSE') -Destination $stage
Copy-Item -LiteralPath (Join-Path $root 'README.md') -Destination $stage
Copy-Item -LiteralPath (Join-Path $root 'SECURITY.md') -Destination $stage
Copy-Item -LiteralPath (Join-Path $root 'SUPPORT.md') -Destination $stage
if (Test-Path -LiteralPath $notes) {
    Copy-Item -LiteralPath $notes -Destination (Join-Path $stage 'RELEASE-NOTES.md')
}

Compress-Archive -LiteralPath $stage -DestinationPath $zip -CompressionLevel Optimal

$hash = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash
"$hash  $(Split-Path -Leaf $zip)" | Set-Content -LiteralPath ($zip + '.sha256') -Encoding ASCII

Write-Output $zip
Write-Output ($zip + '.sha256')
