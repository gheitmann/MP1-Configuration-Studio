param(
    [string]$MediaPortalDir = 'C:\Program Files\Team MediaPortal\MediaPortal',
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '..\dist\plugin')
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$project = Join-Path $root 'plugin\MP1.ConfigurationStudio.Plugin.csproj'
$dotnet = Join-Path $root '..\dotnet\dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet)) {
    $command = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $command) { throw 'The .NET SDK was not found. Install the .NET 8 SDK or set DOTNET_ROOT.' }
    $dotnet = $command.Source
}
if (-not (Test-Path -LiteralPath (Join-Path $MediaPortalDir 'Core.dll'))) {
    throw "MediaPortal assemblies not found in $MediaPortalDir"
}

& $dotnet build $project -c Release -o $OutputDirectory "-p:MediaPortalDir=$MediaPortalDir"
if ($LASTEXITCODE -ne 0) { throw "Plugin build failed with exit code $LASTEXITCODE" }
Write-Output ("Plugin built: " + (Join-Path $OutputDirectory 'MP1.ConfigurationStudio.Plugin.dll'))
