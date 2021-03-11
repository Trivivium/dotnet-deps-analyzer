using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using App.Inspection.Metrics;
using App.Inspection.Extensions;
using App.Inspection.Exceptions;

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
        
        public async Task<InspectionResult> InspectAsync(FileSystemInfo file, InspectionParameters parameters, CancellationToken ct)
        {
            MSBuildLocator.RegisterDefaults();
            
            using(var workspace = MSBuildWorkspace.Create())
            {
                workspace.LoadMetadataForReferencedProjects = true;
                
                var context = new InspectionContext(file, workspace, parameters);
                var metrics = context.CreateMetricInstances(context.Parameters).ToList();
                
                if (file.HasExtension(".csproj"))
                {
                    _logger.LogVerbose($"Inspecting project: {file}");
                    
                    return await InspectProject(context, metrics, ct);
                }
            
                if (file.HasExtension(".sln"))
                {
                    _logger.LogVerbose($"Inspecting solution: {file}");
                    
                    return await InspectSolution(context, metrics, ct);
                }
                
                throw new InspectionException("The specified file path is not a reference to a solution or project file");
            }
        }
        
        private async Task<InspectionResult> InspectProject(InspectionContext context, IList<IMetric> metrics, CancellationToken ct)
        {
            ProjectInspectionResult result;
            
            var project = await context.Workspace.OpenProjectAsync(context.File.FullName, progress: null, ct);

            if (project is null)
            {
                result = ProjectInspectionResult.LoadFailed(context.File);
            }
            else
            {
                result = await ProcessAsync(context.Parameters, project, metrics, ct);
            }

            return new InspectionResult(new List<ProjectInspectionResult>
            {
                result
            });
        }

        private async Task<InspectionResult> InspectSolution(InspectionContext context, IList<IMetric> metrics, CancellationToken ct)
        {
            var solution = await context.Workspace.OpenSolutionAsync(context.File.FullName, progress: null, ct);

            if (solution is null)
            {
                throw new InspectionException("Failed to load the solution");
            }

            var results = new List<ProjectInspectionResult>(solution.ProjectIds.Count);

            foreach (Project project in solution.Projects)
            {
                ProjectInspectionResult result;

                if (context.Parameters.ExcludedProjects.Contains(project.Name.ToUpperInvariant()))
                {
                    _logger.LogVerbose($"Skipping ignored project: {project.Name}");
                    
                    result = ProjectInspectionResult.Ignored(project);
                }
                else
                {
                    result = await ProcessAsync(context.Parameters, project, metrics, ct);
                }
                
                results.Add(result);
            }

            return new InspectionResult(results);
        }

        private async Task<ProjectInspectionResult> ProcessAsync(InspectionParameters parameters, Project project, IList<IMetric> metrics, CancellationToken ct)
        {
            _logger.LogVerbose($"Inspecting project: {project.Name}");

            var compilation = await project.GetCompilationAsync(ct);
                
            if (compilation is null)
            {
                _logger.LogError($"Failed to compile project: {project.Name}");
                    
                return ProjectInspectionResult.CompilationFailed(project);
            }
            
            using (var packageLoadContext = new PackageLoadContext(project, _logger))
            {
                var resolver = packageLoadContext.GetResolver();
                
                var exclusions = NamespaceExclusionList.CreateFromParameters(parameters, project);
                var packages = resolver.GetPackages(project, exclusions);

                var registry = new Dictionary<Package, List<ReferencedSymbol>>();
                var solution = project.Solution;
                
                foreach (var package in packages)
                {
                    var symbols = new List<ReferencedSymbol>();

                    foreach (var type in package.ExportedTypes)
                    {
                        var fqn = type.FullName;

                        if (fqn is null)
                        {
                            _logger.LogError($"Type {type} has no FQN");

                            continue;
                        }
                        
                        var compilationType = compilation.GetTypeByMetadataName(fqn);

                        if (compilationType is null)
                        {
                            _logger.LogVerbose($"Type {type} was not found in the compilation");

                            continue;
                        }

                        var refs = await SymbolFinder.FindReferencesAsync(compilationType, solution, ct);

                        if (refs is null)
                        {
                            continue;
                        }

                        symbols.AddRange(refs);
                        
                        if (compilationType.IsType)
                        {
                            var members = compilationType.GetMembers();
                            var tasks = new List<Task<IEnumerable<ReferencedSymbol>>>(members.Length);
                            
                            foreach (var member in members)
                            {
                                tasks.Add(SymbolFinder.FindReferencesAsync(member, solution, ct));
                            }

                            var membersRefs = await Task.WhenAll(tasks);

                            foreach (var mRefs in membersRefs)
                            {
                                symbols.AddRange(mRefs);
                            }
                        }
                    }
                    
                    registry.Add(package, symbols.Where(s => s.Locations.Any()).ToList());
                }

                var results = new List<PackageInspectionResult>();
                
                foreach (var (package, refs) in registry)
                {
                    var computations = new List<IMetricResult>();
                    
                    foreach (var metric in metrics)
                    {
                        computations.Add(metric.Compute(project, compilation, package, refs));
                    }
                    
                    results.Add(new PackageInspectionResult(package, computations));
                }
                
                return new ProjectInspectionResult(ProjectInspectionState.Ok, project.Name, results);
            }
        }
    }
}
