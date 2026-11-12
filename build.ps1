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
    Write-Warning 'The 1.0 build is x64-only. -TestX86 is kept for compatibility and ignored.'
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

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

Write-Output "Building x64 with $dotnet"
& $dotnet build $project -c $Configuration -o $OutputDirectory
if ($LASTEXITCODE -ne 0) {
    throw "Application build failed with exit code $LASTEXITCODE"
}

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
