# FrendsTaskAnalyzers

[![Analyzers Build](https://github.com/FrendsPlatform/FrendsTasks/actions/workflows/analyzers_build.yml/badge.svg)](https://github.com/FrendsPlatform/FrendsTasks/actions/workflows/analyzers_build.yml)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/FrendsTasks/FrendsTaskAnalyzers|main)
[![NuGet Version](https://img.shields.io/nuget/v/FrendsTaskAnalyzers)](https://www.nuget.org/packages/FrendsTaskAnalyzers/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Package of Roslyn analyzers designed to help in the development of [Frends](https://frends.com) tasks
by enforcing conformance to task development guidelines and best practices.

## Installation

The recommended way to add the analyzers to your task project is
by adding the NuGet package as a `PackageReference` in your `.csproj`.
This ensures the analyzers are automatically active during development and build processes.

```xml
<PropertyGroup>
  <PackageReference Include="FrendsTaskAnalyzers" Version="*">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
</PropertyGroup>
```

## Configuration

After installation, you need to tell the analyzers which methods in your code are task methods.

There are two ways of doing this:

### Using `FrendsTaskMetadata.json` (Recommended)

This is the simplest and recommended way to configure the analyzers.
The `FrendsTaskMetadata.json` file is a standard part of Frends tasks,
and the analyzers can use it to automatically discover the task methods.

To enable this,
ensure the `FrendsTaskMetadata.json` file is included as an `AdditionalFiles` item in your `.csproj` file.

```xml
<ItemGroup>
  <AdditionalFiles Include="FrendsTaskMetadata.json" Pack="true" PackagePath="/">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </AdditionalFiles>
</ItemGroup>
```

### Using `.editorconfig`

Alternatively, you can specify the task methods in your `.editorconfig` file.
This is useful if you cannot change the `FrendsTaskMetadata.json` file to an `AdditionalFiles` item.

Add the `frends_task_analyzers.task_methods` key to your `.editorconfig` file with a semicolon-separated list of your
complete task method names.

```ini
[*.cs]
frends_task_analyzers.task_methods = Frends.Echo.Execute.Echo.Execute;Frends.Echo.Execute.Echo.Execute2
```

## Alternative Usage: Roslynator CLI

If you cannot add a `PackageReference` to your project,
you can run the analyzers using the [Roslynator CLI](https://josefpihrt.github.io/docs/roslynator/cli/).

You will need to configure the task methods using an `.editorconfig` file as described above,
unless `FrendsTaskMetadata.json` is already configured as an `AdditionalFiles` item.

1. **Install Roslynator CLI tool**:

```shell
dotnet tool install -g Roslynator.DotNet.Cli
```

2. **Acquire analyzer DLLs**:
   Download the FrendsTaskAnalyzers NuGet package from the NuGet feed or a GitHub release.
   Unzip the DLL files from the package to a local directory.

```shell
unzip -o -j -d analyzers FrendsTaskAnalyzers.nupkg *.dll
```

3. **Run analysis:**
   Execute Roslynator from your project's directory, providing the path to the analyzer assemblies.

```shell
roslynator analyze --analyzer-assemblies /path/to/analyzers
```
