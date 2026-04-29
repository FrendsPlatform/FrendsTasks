<#
.SYNOPSIS
    Scans a NuGet feed for all Frends.* packages and downloads each with its full
    transitive dependency graph.

.DESCRIPTION
    Uses the Azure DevOps Artifacts REST API to discover every package whose ID starts
    with "Frends." (latest version only), then calls Get-NuGetPackageDependencies.ps1
    for each one. Every task gets its own subfolder under OutputFolder:

        <OutputFolder>\
            Frends.AmazonKinesis.PutRecords\
                Frends.AmazonKinesis.PutRecords.1.0.0.nupkg
                AWSSDK.Kinesis.3.7.x.nupkg
                ...
            Frends.Salesforce.Query\
                ...

.PARAMETER FeedUrl
    NuGet v2 feed URL.

.PARAMETER OutputFolder
    Root output folder. Defaults to .\packages.

.PARAMETER TargetFramework
    TFM passed to the restore step. Defaults to net8.0.

.PARAMETER IdPrefix
    Package ID prefix to search for. Defaults to Frends.

.PARAMETER ContinueOnError
    If set, a failure for one package is logged and the script continues with the rest.
    Without this switch the script stops on first error.

.EXAMPLE
    .\Download-AllFrendsTasks.ps1 -FeedUrl 'https://pkgs.dev.azure.com/frends-platform/frends-tasks/_packaging/main/nuget/v2'

.EXAMPLE
    .\Download-AllFrendsTasks.ps1 `
        -FeedUrl 'https://pkgs.dev.azure.com/frends-platform/frends-tasks/_packaging/main/nuget/v2' `
        -OutputFolder 'C:\frends-offline' `
        -TargetFramework 'net8.0' `
        -ContinueOnError

.NOTES
    Requires the .NET SDK (dotnet CLI) and Get-NuGetPackageDependencies.ps1 in the same
    directory as this script.

    For authenticated feeds, configure NuGet credentials via environment variables before
    running. Do NOT hardcode credentials in this script.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$FeedUrl,

    [Parameter(Mandatory = $false)]
    [string]$OutputFolder = ".\packages",

    [Parameter(Mandatory = $false)]
    [string]$TargetFramework = "net8.0",

    [Parameter(Mandatory = $false)]
    [string]$IdPrefix = "Frends.",

    [Parameter(Mandatory = $false)]
    [switch]$ContinueOnError
)

$ErrorActionPreference = 'Stop'

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$DownloaderScript = Join-Path $ScriptDir "Get-NuGetPackageDependencies.ps1"

if (-not (Test-Path $DownloaderScript)) {
    throw "Get-NuGetPackageDependencies.ps1 not found at: $DownloaderScript"
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet CLI not found. Install the .NET SDK from https://dotnet.microsoft.com/download"
}

# ---------------------------------------------------------------------------
# Discover all matching packages from the v2 OData feed (with pagination)
# ---------------------------------------------------------------------------

function Get-FeedPackages {
    param([string]$FeedUrl, [string]$IdPrefix)

    # The NuGet v2 OData filter endpoint on Azure DevOps is unreliable (TF400898 for any
    # filter expression). Use the Artifacts REST API instead — it's JSON, supports search,
    # and is designed for exactly this. Parse org/project/feed out of the NuGet feed URL.
    #
    # Feed URL format: https://pkgs.dev.azure.com/{org}/{project}/_packaging/{feed}/nuget/v2
    $uri      = [Uri]$FeedUrl
    $segments = $uri.AbsolutePath.TrimStart('/') -split '/'

    if ($uri.Host -ne 'pkgs.dev.azure.com' -or $segments.Count -lt 4) {
        throw ("Feed URL does not look like an Azure DevOps NuGet feed.`n" +
               "Expected: https://pkgs.dev.azure.com/{org}/{project}/_packaging/{feed}/nuget/v2`n" +
               "Got:      $FeedUrl")
    }

    $org      = $segments[0]
    $project  = $segments[1]
    $feedName = $segments[3]
    $apiBase  = "https://feeds.dev.azure.com/$org/$project/_apis/packaging/feeds/$feedName/packages"

    $packages   = [System.Collections.Generic.List[hashtable]]::new()
    $pageSize   = 1000
    $skip       = 0
    $searchTerm = [Uri]::EscapeDataString($IdPrefix.TrimEnd('.'))

    do {
        $url = "{0}?api-version=7.1&packageNameQuery={1}&protocolType=NuGet&includeLatestOnly=true&`$top={2}&`$skip={3}" `
               -f $apiBase, $searchTerm, $pageSize, $skip

        Write-Host "  Querying Artifacts API (skip=$skip) ..." -ForegroundColor DarkGray

        try {
            $response = Invoke-RestMethod -Uri $url -Method Get
        }
        catch {
            throw "Failed to query Artifacts API at $url`n$_"
        }

        $items = @($response.value)
        if ($items.Count -eq 0) { break }

        foreach ($pkg in $items) {
            $id      = $pkg.name
            $latest  = $pkg.versions | Where-Object { $_.isLatest } | Select-Object -First 1
            $version = if ($latest) { $latest.version } else { ($pkg.versions | Select-Object -First 1).version }

            if ($id -and $version -and $id.StartsWith($IdPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
                $packages.Add(@{ Id = $id; Version = $version })
            }
        }

        $skip += $pageSize
    } while ($items.Count -eq $pageSize)

    return $packages
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

Write-Host ""
Write-Host "Scanning feed for packages matching '$IdPrefix*' ..." -ForegroundColor Cyan
Write-Host "  Feed: $FeedUrl"
Write-Host ""

$allPackages = Get-FeedPackages -FeedUrl $FeedUrl -IdPrefix $IdPrefix

if ($allPackages.Count -eq 0) {
    Write-Warning "No packages found matching '$IdPrefix*' on the feed."
    exit 0
}

Write-Host "Found $($allPackages.Count) package(s):" -ForegroundColor Green
$allPackages | ForEach-Object { Write-Host ("  {0,-60} {1}" -f $_.Id, $_.Version) }
Write-Host ""

$rootOutput  = (New-Item -ItemType Directory -Path $OutputFolder -Force).FullName
$succeeded   = [System.Collections.Generic.List[string]]::new()
$failed      = [System.Collections.Generic.List[string]]::new()
$total        = $allPackages.Count
$index        = 0

foreach ($pkg in $allPackages) {
    $index++
    $taskFolder = Join-Path $rootOutput $pkg.Id

    Write-Host ("[{0}/{1}] Downloading {2} {3} ..." -f $index, $total, $pkg.Id, $pkg.Version) `
        -ForegroundColor Cyan
    Write-Host "  -> $taskFolder"

    try {
        & $DownloaderScript `
            -FeedUrl         $FeedUrl `
            -PackageId       $pkg.Id `
            -Version         $pkg.Version `
            -OutputFolder    $taskFolder `
            -TargetFramework $TargetFramework

        if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) {
            throw "Get-NuGetPackageDependencies.ps1 exited with code $LASTEXITCODE"
        }

        $succeeded.Add($pkg.Id)
        Write-Host ""
    }
    catch {
        $msg = "FAILED: $($pkg.Id) $($pkg.Version) - $_"
        Write-Warning $msg
        $failed.Add("$($pkg.Id) $($pkg.Version)")
        Write-Host ""

        if (-not $ContinueOnError) {
            throw "Aborting. Use -ContinueOnError to skip failures and continue."
        }
    }
}

# ---------------------------------------------------------------------------
# Summary
# ---------------------------------------------------------------------------

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "All done." -ForegroundColor Green
Write-Host "  Succeeded : $($succeeded.Count)"
Write-Host "  Failed    : $($failed.Count)"

if ($failed.Count -gt 0) {
    Write-Host ""
    Write-Host "Failed packages:" -ForegroundColor Yellow
    $failed | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
}

Write-Host ""
Write-Host "Output: $rootOutput" -ForegroundColor Green
