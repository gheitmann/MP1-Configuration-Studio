param(
    [string]$MediaPortalDir = 'C:\Program Files\Team MediaPortal\MediaPortal',
    [string]$SkinRoot = 'C:\ProgramData\Team MediaPortal\MediaPortal\skin',
    [string]$ConfigPath = 'C:\ProgramData\Team MediaPortal\MediaPortal\MediaPortal.xml',
    [switch]$Enable,
    [switch]$Uninstall
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$localDll = Join-Path $PSScriptRoot 'MP1.ConfigurationStudio.Plugin.dll'
$dll = if (Test-Path -LiteralPath $localDll) { $localDll } else { Join-Path $root 'dist\plugin\MP1.ConfigurationStudio.Plugin.dll' }
$localSkin = Join-Path $PSScriptRoot 'MP1StudioSettings.xml'
$skin = if (Test-Path -LiteralPath $localSkin) { $localSkin } else { Join-Path $root 'plugin\Skin\MP1StudioSettings.xml' }
$pluginDir = Join-Path $MediaPortalDir 'plugins\Windows'
$pluginName = 'MP1 Configuration Studio'

if ($Uninstall) {
    $target = Join-Path $pluginDir 'MP1.ConfigurationStudio.Plugin.dll'
    if (Test-Path -LiteralPath $target) {
        Remove-Item -LiteralPath $target -Force
        Write-Output ("Removed plugin DLL: " + $target)
    } else {
        Write-Output ("Plugin DLL not installed: " + $target)
    }
    $removedSkins = 0
    if (Test-Path -LiteralPath $SkinRoot) {
        foreach ($dir in Get-ChildItem -LiteralPath $SkinRoot -Directory) {
            $file = Join-Path $dir.FullName 'MP1StudioSettings.xml'
            if (Test-Path -LiteralPath $file) {
                Remove-Item -LiteralPath $file -Force
                $removedSkins++
            }
        }
    }
    Write-Output ("Removed skin file from " + $removedSkins + " skins")
    if (Test-Path -LiteralPath $ConfigPath) {
        $backup = $ConfigPath + '.plugin-backup-' + (Get-Date -Format 'yyyyMMdd-HHmmss')
        Copy-Item -LiteralPath $ConfigPath -Destination $backup -Force
        $document = New-Object System.Xml.XmlDocument
        $document.PreserveWhitespace = $true
        $document.Load($ConfigPath)
        $changed = $false
        foreach ($entry in $document.SelectNodes("/profile/section[@name='plugins']/entry[@name='" + $pluginName + "']")) {
            $entry.InnerText = 'no'
            $changed = $true
        }
        if ($changed) {
            $temporary = $ConfigPath + '.plugin.tmp'
            $document.Save($temporary)
            Move-Item -LiteralPath $temporary -Destination $ConfigPath -Force
            Write-Output ("Disabled plugin in " + $ConfigPath + " (backup: " + (Split-Path $backup -Leaf) + ")")
        } else {
            Remove-Item -LiteralPath $backup -Force
            Write-Output ("Plugin entry not present in " + $ConfigPath)
        }
    }
    return
}

if (-not (Test-Path -LiteralPath $dll)) { throw "Plugin DLL not found: $dll. Build plugin\MP1.ConfigurationStudio.Plugin.csproj first." }
if (-not (Test-Path -LiteralPath $skin)) { throw "Skin file not found: $skin" }

New-Item -ItemType Directory -Path $pluginDir -Force | Out-Null
Copy-Item -LiteralPath $dll -Destination (Join-Path $pluginDir (Split-Path $dll -Leaf)) -Force
Write-Output ("Installed plugin DLL: " + (Join-Path $pluginDir (Split-Path $dll -Leaf)))

foreach ($dir in Get-ChildItem -LiteralPath $SkinRoot -Directory) {
    Copy-Item -LiteralPath $skin -Destination (Join-Path $dir.FullName 'MP1StudioSettings.xml') -Force
}
Write-Output ("Installed skin file into " + ((Get-ChildItem -LiteralPath $SkinRoot -Directory).Count) + " skins")

if ($Enable -and (Test-Path -LiteralPath $ConfigPath)) {
    $backup = $ConfigPath + '.plugin-backup-' + (Get-Date -Format 'yyyyMMdd-HHmmss')
    Copy-Item -LiteralPath $ConfigPath -Destination $backup -Force
    $document = New-Object System.Xml.XmlDocument
    $document.PreserveWhitespace = $true
    $document.Load($ConfigPath)
    $sections = $document.SelectNodes("/profile/section[@name='plugins']")
    $section = $null
    $maxEntries = -1
    foreach ($candidate in $sections) {
        $count = $candidate.SelectNodes("entry").Count
        if ($count -gt $maxEntries) { $section = $candidate; $maxEntries = $count }
    }
    if ($null -eq $section) {
        $section = $document.CreateElement('section')
        $name = $document.CreateAttribute('name'); $name.Value = 'plugins'; $section.Attributes.Append($name)
        $document.DocumentElement.AppendChild($section) | Out-Null
    }
    foreach ($candidate in @($sections)) {
        if (-not [object]::ReferenceEquals($candidate, $section)) { $document.DocumentElement.RemoveChild($candidate) | Out-Null }
    }
    $entry = $section.SelectSingleNode("entry[@name='MP1 Configuration Studio']")
    if ($null -eq $entry) {
        $entry = $document.CreateElement('entry')
        $name = $document.CreateAttribute('name'); $name.Value = 'MP1 Configuration Studio'; $entry.Attributes.Append($name)
        $section.AppendChild($entry) | Out-Null
    }
    $entry.InnerText = 'yes'
    $temporary = $ConfigPath + '.plugin.tmp'
    $document.Save($temporary)
    Move-Item -LiteralPath $temporary -Destination $ConfigPath -Force
    Write-Output ("Enabled plugin in " + $ConfigPath + " (backup: " + (Split-Path $backup -Leaf) + ")")
}
