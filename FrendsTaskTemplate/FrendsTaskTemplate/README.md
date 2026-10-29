# Frends Task template

[![BuildMaster](https://github.com/FrendsPlatform/FrendsTasks/actions/workflows/template_task_release.yml/badge.svg)](https://github.com/FrendsPlatform/FrendsTasks/actions/workflows/template_task_release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

This template can be used to create new .NET (previously .NET Core) Tasks for [Frends](https://frends.com) integration
platform. This enables you to start writing code without any hassle with project formats etc.

You can learn more about custom Tasks [here](https://docs.frends.com/en/articles/2206746-custom-tasks).

## Prerequisite

You will need the [.NET SDK](https://dotnet.microsoft.com/en-us/download/dotnet), at
minimum [.NET SDK 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) is required.

Frends Tasks are usually written in C#, for the best experience you will want a compatible integrated development
environment (IDE); some common examples are Visual Studio, Visual Studio Code and JetBrains Rider. You can also use any
text editor and the [dotnet](https://learn.microsoft.com/en-us/dotnet/core/tools/) command-line interface.

Some IDEs allow you to install the template to the project wizard but can always
use [dotnet new](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new) command when that is not possible.

## Install the template

`dotnet new install frendstasktemplate --nuget-source https://pkgs.dev.azure.com/frends-platform/frends-tasks/_packaging/main/nuget/v3/index.json`

## Clone repository

You need to clone the repository for the new Task. After cloning, move to that folder.

## Create a new Task

Run the following command in the Task's repository folder.

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

## Developing the template

See the [repository README](https://github.com/FrendsPlatform/FrendsTasks/blob/main/FrendsTaskTemplate/README.md)
for the solution layout and how to build, test and pack the template.
