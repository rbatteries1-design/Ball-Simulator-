$ErrorActionPreference = 'Stop'
$destination = Join-Path $PSScriptRoot 'Ball Simulator 3'
if (Test-Path -LiteralPath $destination) { throw 'Move the existing Ball Simulator 3 folder first.' }
$archive = Join-Path ([IO.Path]::GetTempPath()) ([guid]::NewGuid().ToString() + '.zip')
try {
    $stream = [IO.File]::Create($archive)
    try {
        foreach ($part in (Get-ChildItem -LiteralPath $PSScriptRoot -Filter 'source-*.part' | Sort-Object Name)) {
            $inputStream = $part.OpenRead()
            try { $inputStream.CopyTo($stream) } finally { $inputStream.Dispose() }
        }
    } finally { $stream.Dispose() }
    if ((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne '81f53ef8e6b68f42223a33c0287bfa1f6346910d7f1c9feca6f5dcc2fe902d5d') {
        throw 'Source archive checksum mismatch. Download every part again.'
    }
    Expand-Archive -LiteralPath $archive -DestinationPath $destination
} finally {
    Remove-Item -LiteralPath $archive -ErrorAction SilentlyContinue
}
Write-Output "Open this project in Unity 2021.3.16f1: $destination"
