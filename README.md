# A .NET 3rd-Party Dependency Analysis Tool

A .NET Core dependency analyzer that inspects a C# solution or project to compute metrics on the integration of external dependencies at the code level.

## How to use

Before running the tool build the solution or project to be inspected. This is a required to ensure all packages has been restored and the analysis is able to parse the source code.

 To inspect the built solution or project use the `inspect` command with the path to the solution (`.sln`) or project (`.csproj`) file:

```dotnetcli
dotnet deps inspect <path>
```

Use the following command the see available options:

````dotnetcli
dotnet deps inspect -h
````

### Common options

The following three options all take a comma-separated list (`--option=one,two,three`) of values and are case-insensitive:

* Use `--metrics` to declare the metrics to compute. The default is all.
* Use `--excluded-projects` to exclude one or more projects from the results. The project names must be a direct match. Note: This only has an effect when inspecting a solution. The default is none.
* Use `--excluded-namespaces` to exclude one or more namespaces from the results. Use this to exclude an entire dependency (by providing the root namespace) or a subset of types exported by it.

> **Note:** The inspection excludes any type defined in a `System` or `Microsoft` namespace by default. This is currently a hard-coded limitation, which may result in NuGet packages published by Microsoft (e.g., `Microsoft.Extensions.Logging`) to be excluded inadvertently.

## Motivation

The use of external dependencies as an approach to code reuse has become a defacto standard. However, research points to several factors that complicate the practice by introducing mutiple potentially problematic consequences. Examples are:

* An increased risk of breaking changes introduced, while being outside the control of developers. This increases the work-load put on developers to ensure high quality and often requires extra work to put in place sufficient tests.

* A larger attack surface to analyze for security vulnerabilities. This is consequence is compounded if the dependency has a large number of transitive dependencies.

* Added maintenance to keep up-to-date with multiple external developers organisations. This consequence is necessary to ensure continued trust in the dependency, but the process for this is non-standardized and often time-consuming.

This project seeks to automate the calculation of essential metrics on the usage of external dependencies at the code level. This helps developers assess the use on a continuous basis and take proactive actions should the deem it necessary. It is important to emphasize the goal is not to provide hard recommendations, but to surface information to the developers can take better informed decisions.

## Metrics

The inspection can compute the following metrics:

### Usage

This calculates the ratio between the number of members used versus the number available members in the dependency. This provide a high-level overview of the utilization of the dependency.

> **Problem**
>
> A low ratio indicates the project utilizes few of the features the dependency provides. The remaining features thus only adds to the (attack?) surface of code to maintain.
>
> **Potential course of action**
>
> To avoid problems when updating the dependency it could be beneficial to determine if the used features justifies its inclusion. If the utilized features is trivial it could be internalized and the dependency removed.

### Scattering

This metric calculates the ratio of *source files* where a dependency is integrated compared to the total number of files in the project. The result is an estimate of the coupling between a dependency and the project.

> **Problem**
>
> A high scattering corresponds to high coupling, which increases the work required to maintain the integration with the dependency. Fixing bugs or breaking changes after updates are spread out over multiple source files.
>
> **Potential course of action**
>
> It may be beneficial to determine the utilized features and let the project define an abstraction the dependency implements.

### Transitive Count

This metric sums the number of unique transitive dependencies referenced by dependency. For a dependency to be considered unique the combination of its NuGet ID and its version has to be unique across all other dependencys in the dependency graph.

> **Problem**
>
> A dependency with a large number of transitive dependencies increases the risk that updates introduces breaking changes. Further, it also complicates refactoring as transitive dependencies may be used (see the usage metric) by the project without an explicit `<PackageReference>` to inform the developers.
>
> **Potential course of action**
>
> This one is hard to fix as the transitive dependencies are outside the control of the developers. Also, adding an explicit reference to a utilized transitive package may result in version conflicts the package manager currently handles.
>
> A possible fix is to determine if alternative dependencies exists, or if a subset of the transitive dependencies are sufficient.

## Requirements

The requirements for both development and executing the tool as part of the work-flow is the same:

* An installation of the .NET Core SDK version 5 or higher.
* An installation of MSBuild (this is bundled with Visual Studio, so if you have that installed you are good to go).
* The local NuGet dependency cache must be in the default location.

## Building

1. Clone the repository
2. Run `dotnet build`
3. Run `dotnet pack`

### Use a local build

The following commands creates a tool manifest local to the repository to inspect. Skip if it already exists or you're installing the tool globally:

````dotnetcli
dotnet new tool-manifest
dotnet tool install --add-source <path-to-repo>/nupkg Deps
````

This will add the tool to the manifest.
