# Frends Task Dependency Downloader

Two PowerShell scripts for downloading Frends task NuGet packages and their full transitive dependency graphs as `.nupkg` files — useful for offline installs, air-gapped environments, or pre-staging Frends task packages.

| Script | Purpose |
|---|---|
| `Get-NuGetPackageDependencies.ps1` | Download one specific package + its dependencies |
| `Download-AllFrendsTasks.ps1` | Scan the feed and download **all** `Frends.*` packages |

---

## `Get-NuGetPackageDependencies.ps1`

### How it works

The script spins up a throwaway SDK-style `.csproj` in a temp directory, points it at the supplied NuGet feed, adds the requested package, runs `dotnet restore`, then copies every resolved `.nupkg` into your output folder. The temp directory is cleaned up automatically on exit.

## Prerequisites

- PowerShell 5.1+ (or PowerShell 7+)
- [.NET SDK](https://dotnet.microsoft.com/download) — the `dotnet` CLI must be on your `PATH`

## Usage

```powershell
.\Get-NuGetPackageDependencies.ps1 -FeedUrl <feed-url> -PackageId <id> [-Version <version>] [-OutputFolder <path>] [-TargetFramework <tfm>]
```

### Parameters

| Parameter | Required | Default | Description |
|---|---|---|---|
| `FeedUrl` | Yes | — | NuGet feed URL (v2 or v3 `index.json`) |
| `PackageId` | Yes | — | Package identifier to download |
| `Version` | No | latest stable | Specific package version |
| `OutputFolder` | No | `.\packages` | Destination folder for `.nupkg` files |
| `TargetFramework` | No | `net8.0` | TFM used for dependency resolution. Use `netstandard2.0` for broader compatibility |

### Examples

Download the latest version of a Frends task from the main feed:

```powershell
.\Get-NuGetPackageDependencies.ps1 `
  -FeedUrl 'https://pkgs.dev.azure.com/frends-platform/frends-tasks/_packaging/main/nuget/v2' `
  -PackageId 'Frends.AmazonKinesis.PutRecords'
```

Download a specific version targeting `net8.0`, writing packages to a custom folder:

```powershell
.\Get-NuGetPackageDependencies.ps1 `
  -FeedUrl 'https://pkgs.dev.azure.com/frends-platform/frends-tasks/_packaging/main/nuget/v2' `
  -PackageId 'Frends.AmazonKinesis.PutRecords' `
  -Version '1.0.0' `
  -OutputFolder '.\output' `
  -TargetFramework 'net8.0'
```

Download from the public nuget.org feed:

```powershell
.\Get-NuGetPackageDependencies.ps1 `
  -FeedUrl 'https://api.nuget.org/v3/index.json' `
  -PackageId 'Serilog' `
  -Version '3.1.1' `
  -OutputFolder '.\nupkgs'
```

## Frends feed URLs

| Feed | URL |
|---|---|
| Main | `https://pkgs.dev.azure.com/frends-platform/frends-tasks/_packaging/main/nuget/v2` |
| Legacy | `https://pkgs.dev.azure.com/frends-platform/frends-tasks/_packaging/legacy/nuget/v2` |

---

## `Download-AllFrendsTasks.ps1`

Scans a NuGet v2 feed for every `Frends.*` package (latest stable version), then downloads each one with its full dependency graph into a per-task subfolder.

### Output structure

```
packages\
    Frends.AmazonKinesis.PutRecords\
        Frends.AmazonKinesis.PutRecords.1.0.0.nupkg
        AWSSDK.Kinesis.3.7.x.nupkg
        ...
    Frends.Salesforce.Query\
        Frends.Salesforce.Query.2.1.0.nupkg
        ...
```

### Usage

```powershell
.\Download-AllFrendsTasks.ps1 -FeedUrl <feed-url> [-OutputFolder <path>] [-TargetFramework <tfm>] [-IdPrefix <prefix>] [-ContinueOnError]
```

### Parameters

| Parameter | Required | Default | Description |
|---|---|---|---|
| `FeedUrl` | Yes | — | NuGet v2 feed URL |
| `OutputFolder` | No | `.\packages` | Root output folder |
| `TargetFramework` | No | `net8.0` | TFM for dependency resolution |
| `IdPrefix` | No | `Frends.` | Package ID prefix to search for |
| `ContinueOnError` | No | `false` | Skip failures and continue instead of aborting |

### Examples

Download all Frends tasks from the main feed:

```powershell
.\Download-AllFrendsTasks.ps1 `
  -FeedUrl 'https://pkgs.dev.azure.com/frends-platform/frends-tasks/_packaging/main/nuget/v2'
```

Download to a custom folder, skip failures, target `net8.0`:

```powershell
.\Download-AllFrendsTasks.ps1 `
  -FeedUrl 'https://pkgs.dev.azure.com/frends-platform/frends-tasks/_packaging/main/nuget/v2' `
  -OutputFolder 'C:\frends-offline' `
  -TargetFramework 'net8.0' `
  -ContinueOnError
```

---

## Authenticated feeds

The generated `NuGet.config` is project-scoped and does not read your global NuGet credentials. For feeds that require authentication, supply credentials via environment variables and extend the config before running restore. **Never hardcode secrets in the script.**

## Notes

- `nuget.org` is always included as a fallback source so framework reference packages and public transitive dependencies can resolve even when the primary feed is private.
- The isolated `RestorePackagesPath` ensures only packages relevant to the requested graph are collected — your global package cache is untouched.
