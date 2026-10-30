param(
 [Parameter(Mandatory=$true)][string]$SourceDirectory,
 [Parameter(Mandatory=$true)][string]$TestRuntime,
 [Parameter(Mandatory=$true)][string]$ResultFile,
 [string]$MediaPortalDirectory='C:\Program Files\Team MediaPortal\MediaPortal'
)
$ErrorActionPreference='Stop'
$logFile=$ResultFile+'.log'
$testReport=Join-Path $TestRuntime 'native-checks-alpha3.xml'
$testInventory=Join-Path $TestRuntime 'native-inventory-alpha3.xml'
$main=Join-Path $MediaPortalDirectory 'MP1ConfigurationStudio.exe'
$native=Join-Path $MediaPortalDirectory 'Configuration.exe'
$profile='C:\ProgramData\Team MediaPortal\MediaPortal\MediaPortal.xml'
$backup=Join-Path (Split-Path $ResultFile -Parent) ('studio-backup-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $backup -Force | Out-Null
try {
 if(-not(Test-Path -LiteralPath (Join-Path $TestRuntime 'base/studio-test-runtime.marker'))){throw 'The test runtime is not marked as isolated.'}
 $nativeBefore=(Get-FileHash -LiteralPath $native).Hash
 if(Test-Path -LiteralPath $profile){Copy-Item -LiteralPath $profile -Destination $backup}
 $targets=@(Get-Process -Name 'MP1ConfigurationStudio','MP1ConfigurationStudio.x64','MP1ConfigurationStudio.x86' -ErrorAction SilentlyContinue | Where-Object {$_.Path -and (Split-Path $_.Path -Parent) -eq $MediaPortalDirectory})
 foreach($process in $targets){$process.CloseMainWindow() | Out-Null;if(-not $process.WaitForExit(12000)){throw 'Studio is still open. No process was forcefully terminated and no executable was replaced.'}}
 'Closed the previous local Studio normally.' | Set-Content -LiteralPath $logFile
 $profileBefore=if(Test-Path -LiteralPath $profile){(Get-FileHash -LiteralPath $profile).Hash}else{''}
 $testStarted=Get-Date
 $testArgs=@(('--runtime="'+(Join-Path $TestRuntime 'base')+'"'),('--preferences="'+(Join-Path $TestRuntime 'studio-appearance.xml')+'"'),('--audit="'+$testInventory+'"'),('--verify="'+$testReport+'"'))
 Start-Process -FilePath (Join-Path $SourceDirectory 'MP1ConfigurationStudio.exe') -ArgumentList $testArgs -WindowStyle Hidden
 $deadline=(Get-Date).AddSeconds(90)
 $testResult=$null
 while((Get-Date) -lt $deadline){
  if((Test-Path -LiteralPath $testReport) -and (Get-Item -LiteralPath $testReport).LastWriteTime -ge $testStarted){
   try{[xml]$candidate=Get-Content -LiteralPath $testReport -Raw;if($candidate.nativeChecks.total){$testResult=$candidate;break}}catch [System.Xml.XmlException]{}
  }
  Start-Sleep -Milliseconds 250
 }
 if(-not $testResult){throw 'The isolated integration test did not complete; the existing local installation was not replaced.'}
 if($testResult.nativeChecks.passed -ne $testResult.nativeChecks.total){throw ('Integration checks failed: '+$testResult.nativeChecks.passed+'/'+$testResult.nativeChecks.total)}
 $testBase=Join-Path $TestRuntime 'base'
 foreach($testProcess in @(Get-Process -Name 'MP1ConfigurationStudio','MP1ConfigurationStudio.x64','MP1ConfigurationStudio.x86' -ErrorAction SilentlyContinue | Where-Object {$_.Path -and (Split-Path $_.Path -Parent) -eq $testBase})){
  if(-not $testProcess.WaitForExit(15000)){throw 'The isolated test window has not exited; local update stopped.'}
 }
 ('Isolated checks passed: '+$testResult.nativeChecks.passed+'/'+$testResult.nativeChecks.total) | Add-Content -LiteralPath $logFile
 $profileAfter=if(Test-Path -LiteralPath $profile){(Get-FileHash -LiteralPath $profile).Hash}else{''}
 if($profileBefore -ne $profileAfter){throw 'The production profile changed during isolated verification; local update stopped for review.'}
 foreach($name in @('MP1ConfigurationStudio.exe','MP1ConfigurationStudio.x64.exe','MP1ConfigurationStudio.x86.exe')){
  $source=Join-Path $SourceDirectory $name;$target=Join-Path $MediaPortalDirectory $name
  if(Test-Path -LiteralPath $target){Copy-Item -LiteralPath $target -Destination $backup}
  if(Test-Path -LiteralPath ($target+'.config')){Copy-Item -LiteralPath ($target+'.config') -Destination $backup}
  Copy-Item -LiteralPath $source -Destination $target -Force
  Copy-Item -LiteralPath ($native+'.config') -Destination ($target+'.config') -Force
  if((Get-FileHash -LiteralPath $source).Hash -ne (Get-FileHash -LiteralPath $target).Hash){throw ('Installed file hash mismatch: '+$name)}
 }
 if((Get-FileHash -LiteralPath $native).Hash -ne $nativeBefore){throw 'The original native executable changed unexpectedly.'}
 $receipt=[pscustomobject]@{Status='Installed';Version='ALPHA3';NativeOriginalUnchanged=$true;ProductionProfileUnchangedDuringVerification=$true;NativeChecksPassed=[int]$testResult.nativeChecks.passed;NativeChecksTotal=[int]$testResult.nativeChecks.total;Backup=$backup;Executable=$main;Completed=(Get-Date).ToString('o')}
 $receipt | ConvertTo-Json | Set-Content -LiteralPath $ResultFile -Encoding UTF8
 Start-Process -FilePath $main -WorkingDirectory $MediaPortalDirectory -WindowStyle Normal
 'Updated local Studio started.' | Add-Content -LiteralPath $logFile
} catch {
 $_.Exception.ToString() | Add-Content -LiteralPath $logFile
 [pscustomobject]@{Status='Failed';Error=$_.Exception.Message;Backup=$backup} | ConvertTo-Json | Set-Content -LiteralPath $ResultFile -Encoding UTF8
 throw
}


