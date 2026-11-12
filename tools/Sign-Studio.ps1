param(
    [string[]]$Target = @((Join-Path $PSScriptRoot '..\dist\MP1ConfigurationStudio.exe')),
    [switch]$RequireSignature
)

$ErrorActionPreference = 'Stop'

function Find-SignTool {
    $candidates = @()
    $command = Get-Command signtool.exe -ErrorAction SilentlyContinue
    if ($command) { $candidates += $command.Source }

    $kits = Join-Path ${env:ProgramFiles(x86)} 'Windows Kits\10\bin'
    if (Test-Path -LiteralPath $kits) {
        $candidates += Get-ChildItem -LiteralPath $kits -Recurse -Filter signtool.exe -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match '\\x64\\' } |
            Sort-Object LastWriteTime -Descending |
            Select-Object -ExpandProperty FullName
    }

    foreach ($candidate in $candidates) {
        if ($candidate -and (Test-Path -LiteralPath $candidate)) {
            return $candidate
        }
    }
    return $null
}

$base64 = $env:SIGNING_PFX_BASE64
$password = $env:SIGNING_PFX_PASSWORD

if ([string]::IsNullOrWhiteSpace($base64)) {
    if ($RequireSignature) {
        throw 'Signing is required for a final release, but SIGNING_PFX_BASE64 is not configured.'
    }
    Write-Warning 'No signing certificate configured; skipping code signing.'
    exit 0
}

$signTool = Find-SignTool
if (-not $signTool) {
    throw 'signtool.exe was not found. Install the Windows SDK before signing.'
}

$pfx = Join-Path $env:TEMP ('mp1-studio-' + [Guid]::NewGuid().ToString('N') + '.pfx')
try {
    [IO.File]::WriteAllBytes($pfx, [Convert]::FromBase64String($base64))

    foreach ($file in $Target) {
        if (-not (Test-Path -LiteralPath $file)) {
            throw "Signing target not found: $file"
        }

        & $signTool sign /f $pfx /p $password /fd sha256 /tr 'http://timestamp.digicert.com' /td sha256 $file
        if ($LASTEXITCODE -ne 0) {
            throw "signtool failed for $file with exit code $LASTEXITCODE"
        }

        $signature = Get-AuthenticodeSignature -LiteralPath $file
        if ($signature.Status -ne 'Valid') {
            throw "Signature verification failed for $file: $($signature.Status)"
        }
        Write-Output "Signed: $file"
    }
}
finally {
    if (Test-Path -LiteralPath $pfx) {
        Remove-Item -LiteralPath $pfx -Force
    }
}
