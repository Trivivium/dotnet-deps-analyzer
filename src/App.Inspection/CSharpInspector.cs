using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using App.Inspection.Metrics;
using App.Inspection.Packages;
using App.Inspection.Extensions;
using App.Inspection.Exceptions;
using App.Inspection.Executables;

namespace App.Inspection
{
    /// <summary>
    /// Provides a method for inspecting entire C# solutions or a single C# project.
    /// </summary>
    public class CSharpInspector
    {
        private readonly ILogger _logger;

        public CSharpInspector(ILogger logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Inspects the provided <paramref name="file"/>. If the file is a solution (.sln) file all
        /// non-excluded projects are inspected. If the file is a project (.csproj) file that project is
        /// inspected.
        /// </summary>
        /// <param name="file">An absolute file path to either a solution (.sln) or project (.csproj) file.</param>
        /// <param name="parameters">The inspection parameters provided by the user.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <exception cref="InspectionException">This exception is thrown if the inspection process encounters an error.</exception>
        public async IAsyncEnumerable<ProjectInspectionResult> InspectAsync(FileSystemInfo file, InspectionParameters parameters, [EnumeratorCancellation] CancellationToken ct)
        {
            MSBuildLocator.RegisterDefaults();
            
            _logger.LogInformation("Initializing");
            
            using(var workspace = MSBuildWorkspace.Create())
            {
                workspace.LoadMetadataForReferencedProjects = true;
                
                var context = new InspectionContext(file, workspace, parameters);
                var metrics = context.CreateMetricInstances(context.Parameters).ToList();
                
                if (file.HasExtension(".csproj"))
                {
                    _logger.LogVerbose($"Inspecting project: {file}");

                    yield return await InspectProject(context, metrics, ct);
                }
                else if (file.HasExtension(".sln"))
                {
                    _logger.LogVerbose($"Inspecting solution: {file}");

                    await foreach (var result in InspectSolution(context, metrics, ct))
                    {
                        yield return result;
                    }
                }
                else
                {
                    throw new InspectionException("The specified file path is not a reference to a solution or project file");
                }
            }
        }
        
        /// <summary>
        /// Inspects a single project.
        /// </summary>
        /// <param name="context">The inspection context.</param>
        /// <param name="metrics">The metrics to compute on each package of each project in the solution.</param>
        /// <param name="ct">A cancellation token.</param>
        private async Task<ProjectInspectionResult> InspectProject(InspectionContext context, IList<IMetric> metrics, CancellationToken ct)
        {
            var project = await context.Workspace.OpenProjectAsync(context.File.FullName, progress: null, ct);
            
            if (project is null)
            {
                _logger.LogInformation("Project: Load failed");
                
                throw new InspectionException("Failed to load the project");
            }
            
            _logger.LogInformation("Done");

            var sw = Stopwatch.StartNew();
                
            var result = await ProcessAsync(context.Parameters, project, metrics, ct);

            sw.Stop();

            result.Elapsed = sw.Elapsed;
                
            return result;
        }

        /// <summary>
        /// Inspects all non-excluded projects in a solution.
        /// </summary>
        /// <param name="context">The inspection context.</param>
        /// <param name="metrics">The metrics to compute on each package of each project in the solution.</param>
        /// <param name="ct">A cancellation token.</param>
        private async IAsyncEnumerable<ProjectInspectionResult> InspectSolution(InspectionContext context, IList<IMetric> metrics, [EnumeratorCancellation] CancellationToken ct)
        {
            var solution = await context.Workspace.OpenSolutionAsync(context.File.FullName, progress: null, ct);

            if (solution is null)
            {
                _logger.LogInformation("Solution: Load failed");
                
                throw new InspectionException("Failed to load the solution");
            }
            
            _logger.LogInformation("Done");

            var sw = new Stopwatch();
            
            foreach (Project project in solution.Projects)
            {
                if (context.Parameters.ExcludedProjects.Contains(project.Name.ToUpperInvariant()))
                {
                    _logger.LogVerbose($"Skipping ignored project: {project.Name}");
                    
                    yield return ProjectInspectionResult.Ignored(project);
                }
                
                sw.Restart();
                
                var result = await ProcessAsync(context.Parameters, project, metrics, ct);

                sw.Stop();

                result.Elapsed = sw.Elapsed;
                
                yield return result;
            }
        }

        /// <summary>
        /// Processes the provided <paramref name="project"/>, and computes the provided <paramref name="metrics"/>.
        /// </summary>
        /// <param name="parameters">The inspection parameters provided by the user.</param>
        /// <param name="project">The project to inspect.</param>
        /// <param name="metrics">The metrics to compute.</param>
        /// <param name="ct">A cancellation token.</param>
        private async Task<ProjectInspectionResult> ProcessAsync(InspectionParameters parameters, Project project, IList<IMetric> metrics, CancellationToken ct)
        {
            _logger.LogVerbose($"Inspecting project: {project.Name}");
            
            var compilation = await project.GetCompilationAsync(ct);
                
            if (compilation is null)
            {
                throw new InspectionException($"Failed to compile project: {project.Name}");
            }

            var registry = new Registry();
            var resolver = new PackageResolver(_logger);
            var results = new List<PackageInspectionResult>();
            
            var documents = project.Documents.ToImmutableHashSet();
            var exclusions = NamespaceExclusionList.CreateFromParameters(parameters, project);

            await resolver.CreatePackageGraph(project, exclusions, ct);
            
            var memberLookupTable = await GetMemberAccessLookupTable(compilation, exclusions, ct);
            
            using (var portableExecutableLoadContext = new PortableExecutableLoadContext(project, _logger))
            {
                foreach (var executable in portableExecutableLoadContext.GetExecutables(exclusions))
                {
                    var package = resolver.CreatePackage(executable);
                    
                    registry.AddPackage(package);
                
                    foreach (var type in executable.ExportedTypes)
                    {
                        var compilationType = GetCompilationType(compilation, type);
            
                        if (compilationType is null)
                        {
                            continue;
                        }
            
                        var isAdded = await AddConstructorReferences(project.Solution, documents, package, registry, compilationType, ct);
            
                        if (isAdded)
                        {
                            await AddMemberReferences(project.Solution, documents, package, registry, compilationType, memberLookupTable, ct);
                        }
                    }
                    
                    results.Add(ComputeMetrics(project, compilation, package, metrics, registry));
                }
            }

            results.AddRange(AddMissingExplicitPackages(resolver, exclusions, results));
            
            return ProjectInspectionResult.Ok(project, results);
        }

        /// <summary>
        /// Gets the corresponding type of a publicly exported type in a referenced executable.
        /// </summary>
        /// <param name="compilation">The Roslyn compilation of the project source-files.</param>
        /// <param name="type">The exported type from a referenced executable.</param>
        /// <returns></returns>
        private static INamedTypeSymbol? GetCompilationType(Compilation compilation, Type type)
        {
            var fqn = type.FullName;

            if (fqn is null)
            {
                return null;
            }
                    
            return compilation.GetTypeByMetadataName(fqn);
        }

        /// <summary>
        /// Gets all member access syntax tokens from the Roslyn compilation syntax trees, and maps
        /// them into a lookup table.
        /// </summary>
        /// <param name="compilation">The Roslyn compilation of the project source-files.</param>
        /// <param name="exclusions">The list of namespace excluded from the inspection.</param>
        /// <param name="ct">A cancellation token.</param>
        private static async Task<IReadOnlyDictionary<INamespaceSymbol, ICollection<ISymbol>>> GetMemberAccessLookupTable(Compilation compilation, NamespaceExclusionList exclusions, CancellationToken ct)
        {
            var symbols = new List<ISymbol>();
            
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var root = await syntaxTree.GetRootAsync(ct);
                var model = compilation.GetSemanticModel(syntaxTree);

                var intermediate = root
                    .DescendantNodes()
                    .OfType<MemberAccessExpressionSyntax>()
                    .Select(node => model.GetSymbolInfo(node, ct).Symbol)
                    .Where(symbol => symbol != null)
                    .Where(symbol => !exclusions.IsExcluded(symbol!.ContainingNamespace));

                symbols.AddRange(intermediate!);
            }

            var comparer = SymbolEqualityComparer.Default;
            var lookup = new Dictionary<INamespaceSymbol, ICollection<ISymbol>>(comparer);
            
            foreach (var group in symbols.GroupBy(symbol => symbol.ContainingNamespace, comparer))
            {
                lookup.Add((INamespaceSymbol) group.Key!, group.ToList());
            }
               
            return lookup;
        }

        /// <summary>
        /// Finds any references to the constructor or the type itself when inherited from.
        /// </summary>
        /// <param name="solution">The solution the project being inspected is part of.</param>
        /// <param name="documents">A collection of documents (source-files) in the project.</param>
        /// <param name="package">The package being inspected.</param>
        /// <param name="registry">The registry of information collected from the Roslyn compilation.</param>
        /// <param name="compilationType">The corresponding Roslyn type of a publicly exported type in a referenced executable.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns></returns>
        private static async Task<bool> AddConstructorReferences(Solution solution, IImmutableSet<Document> documents, IPackageWithExecutableLoaded package, Registry registry, ISymbol compilationType, CancellationToken ct)
        {
            var refs = await SymbolFinder.FindReferencesAsync(compilationType, solution, documents, ct);

            if (refs is null)
            {
                return false;
            }

            registry.AddPackageSymbols(package, refs);

            return true;
        }

        /// <summary>
        /// Finds any references to members (methods, properties, fields, events) of the type.
        /// </summary>
        /// <param name="solution">The solution the project being inspected is part of.</param>
        /// <param name="documents">A collection of documents (source-files) in the project.</param>
        /// <param name="package">The package being inspected.</param>
        /// <param name="registry">The registry of information collected from the Roslyn compilation.</param>
        /// <param name="compilationType">The corresponding Roslyn type of a publicly exported type in a referenced executable.</param>
        /// <param name="lookup">A lookup table of member accesses symbols.</param>
        /// <param name="ct">A cancellation token.</param>
        private static async Task AddMemberReferences(Solution solution, IImmutableSet<Document> documents, IPackageWithExecutableLoaded package, Registry registry, ISymbol compilationType, IReadOnlyDictionary<INamespaceSymbol, ICollection<ISymbol>> lookup, CancellationToken ct)
        {
            if (lookup.TryGetValue(compilationType.ContainingNamespace, out var members))
            {
                foreach (var member in members)
                {
                    var refs = await SymbolFinder.FindReferencesAsync(member, solution, documents, ct);

                    if (refs is null)
                    {
                        continue;
                    }
                    
                    registry.AddPackageSymbols(package, refs);
                }
            }
        }

        /// <summary>
        /// Computes the metrics of the package.
        /// </summary>
        /// <param name="project">The project being inspected.</param>
        /// <param name="compilation">The Roslyn compilation of the project source-files.</param>
        /// <param name="package">The package being analyzed.</param>
        /// <param name="metrics">A collection of metrics to compute.</param>
        /// <param name="registry">The registry of information collected from the Roslyn compilation.</param>
        private static PackageInspectionResult ComputeMetrics(Project project, Compilation compilation, IPackageWithExecutableLoaded package, IList<IMetric> metrics, Registry registry)
        {
            var results = new List<IMetricResult?>();
                
            foreach (var metric in metrics)
            {
                results.Add(metric.Compute(project, compilation, package, registry));
            }

            return new PackageInspectionResult(package, results);
        }

        /// <summary>
        /// Adds any packages discovered from the NuGet package graph that was not part of any
        /// references in the code.
        /// </summary>
        private static IEnumerable<PackageInspectionResult> AddMissingExplicitPackages(PackageResolver resolver, NamespaceExclusionList exclusions, List<PackageInspectionResult> results)
        {
            foreach (var package in resolver.GetPackages())
            {
                if (exclusions.IsExcluded(package.Name))
                    continue;

                if (results.Any(result => result.Package.Equals(package)))
                    continue;

                yield return new PackageInspectionResult(package, new List<IMetricResult?>());
            }
        }
    }
}
