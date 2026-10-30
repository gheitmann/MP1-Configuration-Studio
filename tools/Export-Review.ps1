param([Parameter(Mandatory=$true)][string]$Report,[Parameter(Mandatory=$true)][string]$Destination)
$ErrorActionPreference='Stop'
[xml]$review=Get-Content -LiteralPath $Report -Raw
$pages=@($review.SelectNodes('//page'))
$active=@($pages | Where-Object {$_.GetAttribute('activated') -eq 'True'}).Count
$stable=@($pages | Where-Object {$_.GetAttribute('themeAndScaleValuesPreserved') -eq 'True'}).Count
$tabs=($pages | ForEach-Object {[int]$_.GetAttribute('detailTabsActivated')} | Measure-Object -Sum).Sum
$roundTrips=@($review.SelectNodes('//saveReload[@status="passed" and @originalValueRestored="True"]'))
$lines=New-Object 'System.Collections.Generic.List[string]'
$lines.Add('# Seitenprüfung – MediaPortal Configuration Studio')
$lines.Add('')
$lines.Add(('Stand: '+(Get-Date -Format 'dd.MM.yyyy HH:mm')+'. Automatisierter Prüflauf in einer isolierten Kopie der lokalen Installation.'))
$lines.Add('')
$lines.Add(('**Seiten aktiviert: '+$active+'/'+$pages.Count+'. Werteerhalt: '+$stable+'/'+$pages.Count+'. Detail-Tabs aktiviert: '+$tabs+'. Erfolgreiche gezielte Speicher-/Laderundläufe: '+$roundTrips.Count+'.**'))
$lines.Add('')
$lines.Add('Werteerhalt wird bei 100, 125 und 175 Prozent sowie Hell und Dunkel geprüft. Die Speicherprüfungen ändern jeweils eine geeignete Checkbox über die originalen SaveSettings-/LoadSettings-Methoden, prüfen die geänderte Profil-Datei und stellen den ursprünglichen Wert wieder her. Sie ersetzen keinen vollständigen fachlichen Speichertest aller Optionen oder der produktiven OK-Validierung. Cache-Leerung und Cache-Schreiben sind pro Fall im XML dokumentiert.')
$lines.Add('')
$lines.Add('| Originalseite | Aktiviert | Werte erhalten | Detail-Tabs | Layout-Hinweise | Speicherprüfung |')
$lines.Add('|---|---|---|---:|---:|---|')
foreach($page in $pages){
 $name=$page.GetAttribute('path').Replace('|','/')
 $save=$page.SelectSingleNode('saveReload')
 $saveStatus=if($save){$save.GetAttribute('status')}else{'nicht ausgeführt'}
 $lines.Add(('| '+$name+' | '+$page.GetAttribute('activated')+' | '+$page.GetAttribute('themeAndScaleValuesPreserved')+' | '+$page.GetAttribute('detailTabsActivated')+' | '+$page.SelectNodes('overflowCandidate').Count+' | '+$saveStatus+' |'))
}
$lines.Add('')
$lines.Add('Layout-Hinweise sind geometrische Kandidaten für abgeschnittene Inhalte und keine automatisch bestätigten sichtbaren Fehler. Leere oder deaktivierte Hardwareoptionen können eine reale Gerätekonfiguration benötigen. Plugin-Konfigurationsdialoge, Tuner, Live-TV, Audioausgabe und Fernbedienungen wurden damit nicht vollständig funktional geprüft. Netzwerkpfade werden nicht als verfügbares Medium vorausgesetzt.')
$lines.Add('')
$lines.Add('Der XML-Nachweis enthält die Einzelresultate und eventuell aufgetretene Ausnahmen. Der separate Prüflaufbeleg überwacht Originalprogramm und produktives MediaPortal.xml während des Tests per SHA-256.')
New-Item -ItemType Directory -Path (Split-Path $Destination -Parent) -Force | Out-Null
$lines | Set-Content -LiteralPath $Destination -Encoding UTF8
