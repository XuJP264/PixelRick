param([string]$Dotnet = 'dotnet', [string]$Iscc = '', [switch]$SkipArtBuild)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = Split-Path $PSScriptRoot -Parent
Push-Location $root
try {
    $version = (Get-Content version.json -Raw | ConvertFrom-Json).version
    if ($version -notmatch '^\d+\.\d+\.\d+$') { throw 'Invalid semantic version' }
    if (!$SkipArtBuild) {
        python tools/art_pipeline/build.py
        if ($LASTEXITCODE) { throw 'Art build failed' }
    }
    python tools/art_pipeline/test_art.py
    if ($LASTEXITCODE) { throw 'Art tests failed' }
    & $Dotnet clean -c Release
    if ($LASTEXITCODE) { throw 'Clean failed' }
    & $Dotnet restore
    if ($LASTEXITCODE) { throw 'Restore failed' }
    & $Dotnet build -c Release --no-restore
    if ($LASTEXITCODE) { throw 'Build failed' }
    & $Dotnet test -c Release --no-build --logger 'trx;LogFileName=tests.trx'
    if ($LASTEXITCODE) { throw 'Tests failed' }
    $publish = Join-Path $root 'dist/publish'
    if (Test-Path $publish) {
        $resolved = (Resolve-Path $publish).Path
        $allowed = [IO.Path]::GetFullPath((Join-Path $root 'dist')) + [IO.Path]::DirectorySeparatorChar
        if (!$resolved.StartsWith($allowed, [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe publish path' }
        Remove-Item -LiteralPath $resolved -Recurse -Force
    }
    & $Dotnet publish src/PixelRick/PixelRick.csproj -c Release -r win-x64 --self-contained true -p:DebugType=None -p:DebugSymbols=false -o $publish
    if ($LASTEXITCODE) { throw 'Publish failed' }
    Copy-Item LICENSE,ARTWORK-NOTICE.md -Destination $publish
    python tools/validate_package.py $publish
    if ($LASTEXITCODE) { throw 'Package validation failed' }
    if (!$Iscc) {
        $candidates = @("$env:LOCALAPPDATA/Programs/Inno Setup 6/ISCC.exe", "${env:ProgramFiles(x86)}/Inno Setup 6/ISCC.exe", "$env:ProgramFiles/Inno Setup 6/ISCC.exe")
        $Iscc = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
    }
    if (!$Iscc) { throw 'Inno Setup 6 compiler not found' }
    & $Iscc "/DAppVersion=$version" "/DPublishDir=$publish" "/DOutputPath=$root/dist" installer/PixelRick.iss
    if ($LASTEXITCODE) { throw 'Installer failed' }
    Compress-Archive -Path "$publish/*" -DestinationPath dist/PixelRick-Portable.zip -Force
    $lines = @('PixelRick-Setup.exe', 'PixelRick-Portable.zip') | ForEach-Object {
        $hash = (Get-FileHash (Join-Path 'dist' $_) -Algorithm SHA256).Hash.ToLowerInvariant()
        "$hash  $_"
    }
    [IO.File]::WriteAllLines((Join-Path $root 'dist/SHA256SUMS.txt'), $lines)
    Write-Output "Release $version ready in $root/dist"
} finally { Pop-Location }
