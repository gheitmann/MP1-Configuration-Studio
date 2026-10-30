param(
 [Parameter(Mandatory=$true)][string]$SourceDirectory,
 [Parameter(Mandatory=$true)][string]$TestRuntime,
 [Parameter(Mandatory=$true)][string]$ResultFile,
 [switch]$ResumeStudio,
 [switch]$Persistence,
 [switch]$OkCycle,
 [switch]$Core
)
$ErrorActionPreference='Stop'
$nativeDirectory='C:\Program Files\Team MediaPortal\MediaPortal'
$native=Join-Path $nativeDirectory 'Configuration.exe'
$studio=Join-Path $nativeDirectory 'MP1ConfigurationStudio.exe'
$profile='C:\ProgramData\Team MediaPortal\MediaPortal\MediaPortal.xml'
$report=Join-Path $TestRuntime $(if($Core){'native-checks-alpha3.xml'}elseif($OkCycle){'native-ok-cycle.xml'}elseif($Persistence){'persistence-review.xml'}else{'full-review.xml'})
$log=$ResultFile+'.log'
$wasOpen=$false
try {
 if(-not(Test-Path -LiteralPath (Join-Path $TestRuntime 'base/studio-test-runtime.marker'))){throw 'Unmarked test runtime.'}
 if($Persistence -or $OkCycle){
  [xml]$locations=Get-Content -LiteralPath (Join-Path $TestRuntime 'base/MediaPortalDirs.xml') -Raw
  $rootPath=[IO.Path]::GetFullPath($TestRuntime).TrimEnd('\')+'\'
  foreach($name in @('Skin','Language')){
   $location=$locations.SelectSingleNode('/Config/Dir[@id="'+$name+'"]')
   if(-not $location -or -not ([IO.Path]::GetFullPath($location.Path)).StartsWith($rootPath,[StringComparison]::OrdinalIgnoreCase)){throw 'Persistence review requires copied Skin/Language assets. Run Stage-TestRuntime.ps1 with IncludeWritableAssets first.'}
  }
 }
 foreach($process in @(Get-Process -Name 'MP1ConfigurationStudio','MP1ConfigurationStudio.x64','MP1ConfigurationStudio.x86' -ErrorAction SilentlyContinue | Where-Object {$_.Path -and (Split-Path $_.Path -Parent) -eq $nativeDirectory})){
  $wasOpen=$true;$process.CloseMainWindow() | Out-Null
  if(-not $process.WaitForExit(12000)){throw 'Studio remains open; review stopped without terminating it.'}
 }
 $nativeBefore=(Get-FileHash -LiteralPath $native).Hash
 $profileBefore=(Get-FileHash -LiteralPath $profile).Hash
 $started=Get-Date
 $reviewArgument=if($Core){'--verify="'}elseif($OkCycle){'--save-test="'}elseif($Persistence){'--persistence="'}else{'--review="'}
 $arguments=@(('--runtime="'+(Join-Path $TestRuntime 'base')+'"'),('--preferences="'+(Join-Path $TestRuntime 'review-appearance.xml')+'"'),($reviewArgument+$report+'"'))
 Start-Process -FilePath (Join-Path $SourceDirectory 'MP1ConfigurationStudio.exe') -ArgumentList $arguments -WindowStyle Hidden
 $deadline=(Get-Date).AddMinutes(15)
 $complete=$false
 $restarted=$false
 while((Get-Date) -lt $deadline){
  if((Test-Path -LiteralPath $report) -and (Get-Item -LiteralPath $report).LastWriteTime -ge $started){
   try{
    [xml]$review=Get-Content -LiteralPath $report -Raw
    if($Core -and $review.DocumentElement.Name -eq 'nativeChecks' -and [int]$review.DocumentElement.GetAttribute('total') -gt 0){
     if($review.DocumentElement.GetAttribute('passed') -ne $review.DocumentElement.GetAttribute('total')){throw 'Native core verification failed.'}
     $complete=$true;break
    }
    if($OkCycle -and -not $restarted -and $review.DocumentElement.GetAttribute('stage') -eq 'saved'){
     foreach($process in @(Get-Process -Name 'MP1ConfigurationStudio','MP1ConfigurationStudio.x64','MP1ConfigurationStudio.x86' -ErrorAction SilentlyContinue | Where-Object {$_.Path -and (Split-Path $_.Path -Parent) -eq (Join-Path $TestRuntime 'base')})){if(-not $process.WaitForExit(15000)){throw 'First OK cycle process has not exited.'}}
     Start-Process -FilePath (Join-Path $SourceDirectory 'MP1ConfigurationStudio.exe') -ArgumentList $arguments -WindowStyle Hidden
     $restarted=$true
    }
    if($review.DocumentElement.GetAttribute('complete') -eq 'true'){
     if($OkCycle -and ($review.DocumentElement.GetAttribute('savedValueMatches') -ne 'True' -or $review.DocumentElement.GetAttribute('freshProcessLoadedSavedValue') -ne 'True' -or $review.DocumentElement.GetAttribute('originalRestored') -ne 'True')){throw 'Native OK cycle did not pass all persistence checks.'}
     $complete=$true;break
    }
   }catch [System.Xml.XmlException]{}
  }
  Start-Sleep -Milliseconds 500
 }
 if(-not $complete){throw 'Review did not complete. Inspect the isolated test window and partial report.'}
 foreach($process in @(Get-Process -Name 'MP1ConfigurationStudio','MP1ConfigurationStudio.x64','MP1ConfigurationStudio.x86' -ErrorAction SilentlyContinue | Where-Object {$_.Path -and (Split-Path $_.Path -Parent) -eq (Join-Path $TestRuntime 'base')})){
  if(-not $process.WaitForExit(15000)){throw 'Review window has not exited.'}
 }
 if((Get-FileHash -LiteralPath $native).Hash -ne $nativeBefore -or (Get-FileHash -LiteralPath $profile).Hash -ne $profileBefore){throw 'Production integrity mismatch.'}
 [pscustomobject]@{Status='Reviewed';ProductionUnchanged=$true;Report=$report;Completed=(Get-Date).ToString('o')} | ConvertTo-Json | Set-Content -LiteralPath $ResultFile -Encoding UTF8
 if($wasOpen -and $ResumeStudio){Start-Process -FilePath $studio -WorkingDirectory $nativeDirectory -WindowStyle Normal}
}catch{
 $_.Exception.ToString() | Set-Content -LiteralPath $log
 [pscustomobject]@{Status='Failed';Error=$_.Exception.Message;Report=$report} | ConvertTo-Json | Set-Content -LiteralPath $ResultFile -Encoding UTF8
 throw
}
