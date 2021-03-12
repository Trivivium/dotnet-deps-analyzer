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

using App.Inspection.Metrics;
using App.Inspection.Extensions;
using App.Inspection.Exceptions;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace App.Inspection
{
    public class Inspector
    {
        private readonly ILogger _logger;

        public Inspector(ILogger logger)
        {
            _logger = logger;
        }
        
        public async IAsyncEnumerable<ProjectInspectionResult> InspectAsync(FileSystemInfo file, InspectionParameters parameters, [EnumeratorCancellation] CancellationToken ct)
        {
            MSBuildLocator.RegisterDefaults();
            
            _logger.LogInformation("Workspace: Initializing");
            
            using(var workspace = MSBuildWorkspace.Create())
            {
                workspace.LoadMetadataForReferencedProjects = true;
                
                _logger.LogInformation("Workspace: Done");
                
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
        
        private async Task<ProjectInspectionResult> InspectProject(InspectionContext context, IList<IMetric> metrics, CancellationToken ct)
        {
            _logger.LogInformation("Project: initializing");
            
            var project = await context.Workspace.OpenProjectAsync(context.File.FullName, progress: null, ct);
            
            if (project is null)
            {
                _logger.LogInformation("Project: Load failed");
                
                throw new InspectionException("Failed to load the project");
            }
            
            _logger.LogInformation("Project: Done");
            
            return await ProcessAsync(context.Parameters, project, metrics, ct);
        }

        private async IAsyncEnumerable<ProjectInspectionResult> InspectSolution(InspectionContext context, IList<IMetric> metrics, [EnumeratorCancellation] CancellationToken ct)
        {
            _logger.LogInformation("Solution: Initializing");
            
            var solution = await context.Workspace.OpenSolutionAsync(context.File.FullName, progress: null, ct);

            if (solution is null)
            {
                _logger.LogInformation("Solution: Load failed");
                
                throw new InspectionException("Failed to load the solution");
            }
            
            _logger.LogInformation("Solution: Done");
            
            foreach (Project project in solution.Projects)
            {
                if (context.Parameters.ExcludedProjects.Contains(project.Name.ToUpperInvariant()))
                {
                    _logger.LogVerbose($"Skipping ignored project: {project.Name}");
                    
                    yield return ProjectInspectionResult.Ignored(project);
                }
                
                yield return await ProcessAsync(context.Parameters, project, metrics, ct);
            }
        }

        private async Task<ProjectInspectionResult> ProcessAsync(InspectionParameters parameters, Project project, IList<IMetric> metrics, CancellationToken ct)
        {
            _logger.LogVerbose($"Inspecting project: {project.Name}");

            var sw = Stopwatch.StartNew();
            
            var compilation = await project.GetCompilationAsync(ct);
                
            if (compilation is null)
            {
                _logger.LogError($"Failed to compile project: {project.Name}");
                    
                return ProjectInspectionResult.CompilationFailed(project);
            }

            var exclusions = NamespaceExclusionList.CreateFromParameters(parameters, project);
            var memberLookupTable = await GetMemberAccessLookupTable(compilation, exclusions, ct);

            var registry = new Registry();
            var documents = project.Documents.ToImmutableHashSet();

            var results = new List<PackageInspectionResult>();
            
            using (var packageLoadContext = new PackageLoadContext(project, _logger))
            {
                var resolver = packageLoadContext.GetResolver();
            
                foreach (var package in resolver.GetPackages(project, exclusions))
                {
                    registry.AddPackage(package);
                
                    foreach (var type in package.ExportedTypes)
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
                    
                    results.Add(GetPackageInspectionResult(project, compilation, package, metrics, registry));
                }
            }

            var elapsed = sw.Elapsed;
            
            sw.Stop();
            
            return new ProjectInspectionResult(ProjectInspectionState.Ok, project.Name, elapsed, results);
        }

        private static INamedTypeSymbol? GetCompilationType(Compilation compilation, Type type)
        {
            var fqn = type.FullName;

            if (fqn is null)
            {
                return null;
            }
                    
            return compilation.GetTypeByMetadataName(fqn);
        }

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
        private static async Task<bool> AddConstructorReferences(Solution solution, IImmutableSet<Document> documents, Package package, Registry registry, ISymbol compilationType, CancellationToken ct)
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
        private static async Task AddMemberReferences(Solution solution, IImmutableSet<Document> documents, Package package, Registry registry, ISymbol compilationType, IReadOnlyDictionary<INamespaceSymbol, ICollection<ISymbol>> lookup, CancellationToken ct)
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

        private static PackageInspectionResult GetPackageInspectionResult(Project project, Compilation compilation, Package package, IList<IMetric> metrics, Registry registry)
        {
            var results = new List<IMetricResult>();
                
            foreach (var metric in metrics)
            {
                results.Add(metric.Compute(project, compilation, package, registry));
            }

            return new PackageInspectionResult(package, results);
        }
    }
}
