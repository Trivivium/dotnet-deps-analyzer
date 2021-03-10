# A .NET 3rd-Party Dependency Analysis Tool

A .NET Core dependency analyzer that inspects your projects to provide metrics on the usage of third-party dependencies. The tool is realized as a command-line application.

## Usage

```dotnetcli
cd ./src/App
dotnet ./App.csproj -- inspect <path-to-solution-to-inspect>
```

## Motivation

The use of third-party dependencies as a method of code reuse has become a defacto standard. However, research points to several factors that complicate the usage with examples such as breaking changes, security, and continued maintenance.

This project seeks to automate the calculation of essential metrics on the usage of third-party dependencies at the code level. This helps developers assess the use on a continuous basis and take proactive actions should the deem it necessary. It is important to emphasize the goal is not to provide hard recommendations, but to surface information to the developers can take better informed decisions.

## Metrics

The metrics focus on modularity by calculating the following metrics:

* The number of members used versus the number available in the dependency. This provide a high-level overview of the utilization of the dependency.
* The scattering of the used members across the project. This metric provide an estimate of how well an external dependency is decoupled from the project. High scattering could indicate high coupling that may complicate dependency upgrades and increase the work required to address breaking changes.

## Requirements

The requirements for both development and executing the tool as part of the work-flow is the same:

* An installation of the .NET Core SDK version 5 or higher.
