param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'dist'),
    [string]$Configuration = 'Release',
    [switch]$SkipTests,
    [switch]$TestX86
)

$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'

if ($TestX86) {
    # Kept for compatibility; the x86 build is now part of every Release build.
    Write-Output 'The x86 build is verified automatically.'
}

function Find-DotNet {
    $candidates = @()
    if ($env:DOTNET_ROOT) { $candidates += (Join-Path $env:DOTNET_ROOT 'dotnet.exe') }
    $command = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($command) { $candidates += $command.Source }
    $candidates += (Join-Path $PSScriptRoot '..\dotnet\dotnet.exe')
    $candidates += (Join-Path $env:ProgramFiles 'dotnet\dotnet.exe')
    $candidates += (Join-Path ${env:ProgramFiles(x86)} 'dotnet\dotnet.exe')

    foreach ($candidate in $candidates) {
        if (-not $candidate -or -not (Test-Path -LiteralPath $candidate)) {
            continue
        }

        $sdks = & $candidate --list-sdks 2>$null
        if ($LASTEXITCODE -eq 0 -and $sdks) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    throw 'The .NET SDK was not found. Install the .NET 8 SDK or set DOTNET_ROOT, then run this script again.'
}

$dotnet = Find-DotNet
$project = Join-Path $PSScriptRoot 'MP1.ConfigurationStudio.csproj'
$diagnosticsProject = Join-Path $PSScriptRoot 'diagnostics\Studio.Diagnostics.csproj'
$diagnosticsOutput = Join-Path $OutputDirectory 'diagnostics'

function Get-PeMachine([string]$Path) {
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $peOffset = [System.BitConverter]::ToInt32($bytes, 0x3C)
    if ($peOffset -lt 0 -or $peOffset + 6 -ge $bytes.Length) { throw "Invalid PE header: $Path" }
    return [System.BitConverter]::ToUInt16($bytes, $peOffset + 4)
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

Write-Output "Building x64 with $dotnet"
& $dotnet build $project -c $Configuration -o $OutputDirectory
if ($LASTEXITCODE -ne 0) {
    throw "Application build failed with exit code $LASTEXITCODE"
}

$mainExe = Join-Path $OutputDirectory 'MP1ConfigurationStudio.exe'
if ((Get-PeMachine $mainExe) -ne 0x8664) {
    throw "The main application is not an x64 image: $mainExe"
}

Write-Output "Building x86 with $dotnet"
$x86Output = Join-Path $OutputDirectory 'x86'
& $dotnet build $project -c ReleaseX86 -p:PlatformTarget=x86 -o $x86Output
if ($LASTEXITCODE -ne 0) {
    throw "x86 application build failed with exit code $LASTEXITCODE"
}
$x86Exe = Join-Path $x86Output 'MP1ConfigurationStudio.exe'
if ((Get-PeMachine $x86Exe) -ne 0x14C) {
    throw "The x86 application is not an x86 image: $x86Exe"
}
Copy-Item -LiteralPath $x86Exe -Destination (Join-Path $OutputDirectory 'MP1ConfigurationStudio.x86.exe') -Force
Copy-Item -LiteralPath ($mainExe + '.config') -Destination (Join-Path $OutputDirectory 'MP1ConfigurationStudio.x86.exe.config') -Force

& $dotnet build $diagnosticsProject -c $Configuration -o $diagnosticsOutput
if ($LASTEXITCODE -ne 0) {
    throw "Diagnostics build failed with exit code $LASTEXITCODE"
}

if (-not $SkipTests) {
    $tests = Join-Path $diagnosticsOutput 'StudioDiagnostics.exe'
    if (-not (Test-Path -LiteralPath $tests)) {
        throw "Diagnostics host not found: $tests"
    }

    $testLog = Join-Path $OutputDirectory 'test-results.log'
    & $tests --self-test | Tee-Object -FilePath $testLog
    if ($LASTEXITCODE -ne 0) {
        throw 'Preservation checks failed'
    }
}

Write-Output "Build and tests passed: $OutputDirectory"
