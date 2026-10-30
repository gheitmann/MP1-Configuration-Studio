param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'dist'),[switch]$TestX86)
$ErrorActionPreference='Stop'
$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
if(-not(Test-Path -LiteralPath $compiler)){$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework/v4.0.30319/csc.exe'}
if(-not(Test-Path -LiteralPath $compiler)){throw '.NET Framework 4.x C# compiler not found.'}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$sources=@(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'src') -Filter '*.cs' | ForEach-Object FullName)
$common=@('/nologo','/optimize+','/debug:pdbonly','/r:System.dll','/r:System.Core.dll','/r:System.Data.dll','/r:System.Drawing.dll','/r:System.Windows.Forms.dll','/r:System.Xml.dll')
foreach($architecture in @('anycpu','x64','x86')){
  $fileName=if($architecture -eq 'anycpu'){'MP1ConfigurationStudio.exe'}else{'MP1ConfigurationStudio.'+$architecture+'.exe'}
  $out=Join-Path $OutputDirectory $fileName
  & $compiler @common '/target:winexe' ('/platform:'+$architecture) ('/out:'+$out) ('/win32manifest:'+(Join-Path $PSScriptRoot 'app.manifest')) @sources
  if($LASTEXITCODE -ne 0){throw "Build failed: $architecture"}
  Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'app.config') -Destination ($out+'.config')
}
$test=Join-Path $OutputDirectory 'StudioTests.exe'
& $compiler @common '/target:exe' '/platform:x64' ('/out:'+$test) ('/win32manifest:'+(Join-Path $PSScriptRoot 'app.manifest')) @sources
if($LASTEXITCODE -ne 0){throw 'Test build failed'}
$testLog=Join-Path $OutputDirectory 'test-results.log'
& $test --self-test | Tee-Object -FilePath $testLog
if($LASTEXITCODE -ne 0){throw 'Preservation checks failed'}
if($TestX86){
 $test32=Join-Path $OutputDirectory 'StudioTests.x86.exe'
 & $compiler @common '/target:exe' '/platform:x86' ('/out:'+$test32) ('/win32manifest:'+(Join-Path $PSScriptRoot 'app.manifest')) @sources
 if($LASTEXITCODE -ne 0){throw 'x86 fixture build failed'}
 & $test32 --self-test | Tee-Object -FilePath (Join-Path $OutputDirectory 'test-results-x86.log')
 if($LASTEXITCODE -ne 0){throw 'x86 preservation checks failed'}
}
Write-Output "Build and tests passed: $OutputDirectory"
