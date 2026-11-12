<#
.SYNOPSIS
    Packs Alphacloud.CakeBuild as a local development version into a local NuGet feed.

.DESCRIPTION
    Development / validation helper. Packs the library with a fixed version (default 0.1.0) into a
    folder registered as a NuGet source and removes the same version from the global packages cache,
    so consumers (e.g. Src/Samples/SampleSolution) restore the freshly built package instead of a stale copy.

.EXAMPLE
    ./pack-local.ps1
    ./pack-local.ps1 -Version 0.1.1 -Feed D:/nuget/local
#>
param(
    [string] $Version = '0.1.0',
    [string] $Feed = 'W:/Data/nuget/temp'
)

$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'Src/Lib/Alphacloud.CakeBuild/Alphacloud.CakeBuild.csproj'
$packageId = 'alphacloud.cakebuild'

if (-not (Test-Path $Feed)) {
    New-Item -ItemType Directory -Path $Feed | Out-Null
}

Get-ChildItem -Path $Feed -Filter "Alphacloud.CakeBuild.$Version.*nupkg" -ErrorAction SilentlyContinue |
    Remove-Item -Force

dotnet pack $project -c Release -o $Feed "-p:Version=$Version" --nologo
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$globalPackages = ((dotnet nuget locals global-packages --list) -replace '^global-packages:\s*', '').Trim()
$cached = Join-Path $globalPackages "$packageId/$Version"
if (Test-Path $cached) {
    Remove-Item -Recurse -Force $cached
    Write-Host "Removed cached package $cached"
}

Write-Host "Packed Alphacloud.CakeBuild $Version to $Feed"
