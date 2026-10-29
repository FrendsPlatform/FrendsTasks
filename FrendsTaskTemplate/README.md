# Frends Task template

[![BuildMaster](https://github.com/FrendsPlatform/FrendsTasks/actions/workflows/task_template_pipeline.yml/badge.svg)](https://github.com/FrendsPlatform/FrendsTasks/actions/workflows/task_template_pipeline.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

This template can be used to create new .NET based Tasks for [Frends](https://frends.com) integration
platform. This enables you to start writing code without any hassle with project formats etc. This folder is the
source of the `frendstasktemplate` NuGet package (`dotnet new frends-task`).

## Prerequisite

You will need the [.NET SDK](https://dotnet.microsoft.com/en-us/download/dotnet), at
minimum [.NET SDK 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) is required.

## Install the template

`dotnet new install frendstasktemplate --nuget-source https://pkgs.dev.azure.com/frends-platform/frends-tasks/_packaging/main/nuget/v3/index.json`

## Clone repository

You need to clone the repository for the new Task. After cloning, move to that folder.

## Create a new Task

Run the following command in the Task's repository root folder.

`dotnet new frends-task -F Company.System.Action -D "Description of the Task"`

For example `-F Frends.Xml.Write`. Note that the reserved `Frends.*` prefix is intended for official Frends Tasks only.

## Get help using the template

`dotnet new frends-task -h`

### Example output:

```
Frends Task (C#)
Author: Frends

Usage:
  dotnet new frends-task [options] [template options]

Options:
  -n, --name <name>       The name for the output being created. If no name is specified, the name of the output directory is used.
  -o, --output <output>   Location to place the generated output.
  --dry-run               Displays a summary of what would happen if the given command line were run if it would result in a template creation.
  --force                 Forces content to be generated even if it would change existing files.
  --no-update-check       Disables checking for the template package updates when instantiating a template.
  --project <project>     The project that should be used for context evaluation.
  -lang, --language <C#>  Specifies the template language to instantiate.
  --type <solution>       Specifies the template type to instantiate.

Template options:
  -D, --Description <Description>    Description of what the Task will do.
                                     Type: string
                                     Default: Description of the Task
  -F, --FullTaskName <FullTaskName>  Full name in format: Company.System.Action e.g. Frends.Xml.Write
                                     Required: *true*
                                     Type: string
                                     Default: Frends.Echo.Execute
```

## Update the template

`dotnet new update`

## Uninstall the template

`dotnet new uninstall frendstasktemplate`

## Solution layout

| Path                                                                      | Description                                                                                        |
|---------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------|
| [FrendsTaskTemplate.sln](FrendsTaskTemplate.sln)                          | Solution containing the template package project and its tests.                                    |
| [FrendsTaskTemplate/](FrendsTaskTemplate)                                 | Template package project (`FrendsTaskTemplate.csproj`); this README is packed as the NuGet readme. |
| [FrendsTaskTemplate/Frends.Template/](FrendsTaskTemplate/Frends.Template) | Template content: `.template.config`, GitHub workflows and the `Party.Echo.Execute` sample Task.   |
| [FrendsTaskTemplate.Tests/](FrendsTaskTemplate.Tests)                     | Unit tests that compile the sample Task sources (linked) and verify codebase's repetitive part.    |

## Developing the template

Clone the repository:

`git clone https://github.com/FrendsPlatform/FrendsTasks.git`

Run the commands below from this folder (`FrendsTaskTemplate`).

Run the tests:

`dotnet test`

Pack the template:

`dotnet pack --configuration Release`

The NuGet package is created in `FrendsTaskTemplate\bin\Release`.

Install the template from the locally created package:

`dotnet new install <ABSOLUTE_PATH_TO_NUPKG_FILE>`

Then try it in an empty folder:

`dotnet new frends-task -F Company.System.Action -D "Description of the Task"`

## Releasing

The [Task Template Pipeline](../.github/workflows/task_template_pipeline.yml) workflow runs the tests on every push.
When all tests pass, it packs the template and pushes it to the Azure Artifacts feed (`main` feed from the `main`
branch, `test` feed otherwise). Bump `PackageVersion`
in [FrendsTaskTemplate](FrendsTaskTemplate/FrendsTaskTemplate.csproj) and describe changes in [CHANGELOG](CHANGELOG.md) for each release.
