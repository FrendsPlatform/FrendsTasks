<#
.SYNOPSIS
    Downloads a NuGet package and all its transitive dependencies from a feed.

.DESCRIPTION
    Spins up a throwaway SDK-style project, points it at the supplied feed,
    adds the requested package, runs `dotnet restore`, then copies every
    resolved .nupkg file into the output subfolder.

.PARAMETER FeedUrl
    NuGet feed URL (v2 or v3 index.json).

.PARAMETER PackageId
    Package identifier to download.

.PARAMETER Version
    Optional. Specific version. If omitted, the latest stable version is used.

.PARAMETER OutputFolder
    Optional. Subfolder for the .nupkg files. Defaults to .\packages.

.PARAMETER TargetFramework
    Optional. TFM used by the temp project for dependency resolution.
    Defaults to net8.0. Use netstandard2.0 if you need broader compatibility.

.EXAMPLE
    .\Get-NuGetPackage.ps1 -FeedUrl 'https://api.nuget.org/v3/index.json' -PackageId 'Newtonsoft.Json'

.EXAMPLE
    .\Get-NuGetPackage.ps1 'https://api.nuget.org/v3/index.json' 'Serilog' -Version '3.1.1' -OutputFolder '.\nupkgs'

.NOTES
    For authenticated feeds, set credentials via environment variables and
    extend the generated NuGet.config (do NOT hardcode secrets in the script).
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$FeedUrl,

    [Parameter(Mandatory = $true, Position = 1)]
    [string]$PackageId,

    [Parameter(Mandatory = $false)]
    [string]$Version,

    [Parameter(Mandatory = $false)]
    [string]$OutputFolder = ".\packages",

    [Parameter(Mandatory = $false)]
    [string]$TargetFramework = "net8.0"
)

$ErrorActionPreference = 'Stop'

# Verify dotnet CLI is available
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet CLI not found. Install the .NET SDK from https://dotnet.microsoft.com/download"
}

# Resolve absolute output path (creates folder if missing)
$OutputPath = (New-Item -ItemType Directory -Path $OutputFolder -Force).FullName
Write-Host "Output folder: $OutputPath" -ForegroundColor Cyan

# Unique temp working directory
$TempDir = Join-Path ([System.IO.Path]::GetTempPath()) ("nugetdl_" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $TempDir | Out-Null

try {
    Push-Location $TempDir

    # Minimal SDK-style project. RestorePackagesPath isolates the package cache
    # so we get a clean view of just what this package + its graph pulled in.
    $csproj = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>$TargetFramework</TargetFramework>
    <RestorePackagesPath>__packages__</RestorePackagesPath>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
  </PropertyGroup>
</Project>
"@
    Set-Content -Path "tmp.csproj" -Value $csproj -Encoding UTF8

    # Project-scoped NuGet.config so we don't read or pollute user/global config.
    # nuget.org is included as a fallback so framework ref packages and public
    # transitive dependencies (e.g. Microsoft.NETCore.App.Ref) can resolve even
    # when only the custom feed is the primary source.
    $nugetConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="customFeed" value="$FeedUrl" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
"@
    Set-Content -Path "NuGet.config" -Value $nugetConfig -Encoding UTF8

    # Add the package reference
    $addArgs = @('add', 'tmp.csproj', 'package', $PackageId)
    if ($Version) { $addArgs += @('--version', $Version) }

    $verLabel = if ($Version) { " $Version" } else { " (latest)" }
    Write-Host "Adding $PackageId$verLabel ..." -ForegroundColor Cyan
    & dotnet @addArgs
    if ($LASTEXITCODE -ne 0) { throw "Failed to add package $PackageId from $FeedUrl" }

    # Restore resolves the full dependency graph and downloads every .nupkg
    Write-Host "Resolving and downloading dependencies ..." -ForegroundColor Cyan
    & dotnet restore tmp.csproj --packages "__packages__"
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }

    # Collect every .nupkg (one per package id+version)
    $packagesRoot = Join-Path $TempDir "__packages__"
    $nupkgFiles = Get-ChildItem -Path $packagesRoot -Filter '*.nupkg' -Recurse -File

    if (-not $nupkgFiles) { throw "No .nupkg files were produced." }

    Write-Host "Copying packages to $OutputPath ..." -ForegroundColor Cyan
    foreach ($pkg in $nupkgFiles) {
        Copy-Item -Path $pkg.FullName -Destination $OutputPath -Force
        Write-Host ("  - {0}" -f $pkg.Name)
    }

    Write-Host ""
    Write-Host ("Done. {0} package(s) written to {1}" -f $nupkgFiles.Count, $OutputPath) -ForegroundColor Green
}
finally {
    Pop-Location
    Remove-Item -Path $TempDir -Recurse -Force -ErrorAction SilentlyContinue
}
