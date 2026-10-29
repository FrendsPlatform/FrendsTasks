# Frends Task template - solution

[![BuildMaster](https://github.com/FrendsPlatform/FrendsTasks/actions/workflows/template_task_release.yml/badge.svg)](https://github.com/FrendsPlatform/FrendsTasks/actions/workflows/template_task_release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

Source of the `frendstasktemplate` NuGet package (`dotnet new frends-task`). For instructions on installing and
using the template, see the [template README](FrendsTaskTemplate/README.md).

## Solution layout

| Path                                                                                                  | Description                                                                                       |
|-------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------|
| [FrendsTaskTemplate.sln](FrendsTaskTemplate.sln)                                                      | Solution containing the template package project and its tests.                                   |
| [FrendsTaskTemplate/](FrendsTaskTemplate)                                                             | Template package project (`FrendsTaskTemplate.csproj`); its README is the NuGet package readme.   |
| [FrendsTaskTemplate/Frends.Template/](FrendsTaskTemplate/Frends.Template)                             | Template content: `.template.config`, GitHub workflows and the `Party.Echo.Execute` sample Task.  |
| [FrendsTaskTemplate.Tests/](FrendsTaskTemplate.Tests)                                                 | Unit tests that compile the sample Task sources (linked) and verify attributes and helpers.       |

The sample Task in `Party.Echo.Execute` has its own solution, functional tests and
[README](FrendsTaskTemplate/Frends.Template/Party.Echo.Execute/README.md); its names are replaced with the
`--FullTaskName` value when the template is instantiated (see
[template.json](FrendsTaskTemplate/Frends.Template/.template.config/template.json)).

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

The [Release Task Template](../.github/workflows/template_task_release.yml) workflow runs on pushes touching this
folder, packs the template and pushes it to the Azure Artifacts feed (`main` feed from the `main` branch, `test` feed
otherwise). Bump `PackageVersion` in [FrendsTaskTemplate.csproj](FrendsTaskTemplate/FrendsTaskTemplate.csproj) for
each release.
