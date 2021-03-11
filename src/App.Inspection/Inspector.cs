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
                result = await AnalyzeAsync(context.Parameters, project, metrics, ct);
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
                    result = await AnalyzeAsync(context.Parameters, project, metrics, ct);
                }
                
                results.Add(result);
            }

            return new InspectionResult(results);
        }

        private async Task<ProjectInspectionResult> AnalyzeAsync(InspectionParameters parameters, Project project, IList<IMetric> metrics, CancellationToken ct)
        {
            _logger.LogVerbose($"Inspecting project: {project.Name}");



            //return new ProjectInspectionResult(ProjectInspectionState.Ok, "test", new List<PackageInspectionResult>());

            var compilation = await project.GetCompilationAsync(ct);
            
            if (compilation is null)
            {
                _logger.LogError($"Failed to compile project: {project.Name}");
                
                return ProjectInspectionResult.CompilationFailed(project);
            }
            
            var registry = await CollectAsync(parameters, compilation, ct);
            
            return ComputeMetrics(project, compilation, registry, metrics);
        }

        private static async Task<Registry> CollectAsync(InspectionParameters parameters, Compilation compilation, CancellationToken ct)
        {
            var registry = Registry.CreateFromParameters(parameters);
            
            foreach (var tree in compilation.SyntaxTrees)
            {
                var model = compilation.GetSemanticModel(tree);
                var collector = new RegistryCollector(registry, model);

                var root = await tree.GetRootAsync(ct);
                
                collector.Visit(root);
            }

            return registry;
        }

        private static ProjectInspectionResult ComputeMetrics(Project project, Compilation compilation, Registry registry, IList<IMetric> metrics)
        {      
            // TODO: Compute packages 
            ICollection<Package> packages = new[]
            {
                new Package(new Namespace("Serilog.AspNetCore")),
                new Package(new Namespace("Swashbuckle.AspNetCore")),
            };
            
            var results = new List<PackageInspectionResult>();

            foreach (var package in packages)
            {
                var computations = new List<IMetricResult>();
                
                foreach (var metric in metrics)
                {
                    computations.Add(metric.Compute(project, compilation, registry, package));
                }
                
                results.Add(new PackageInspectionResult(package, computations));
            }

            return new ProjectInspectionResult(ProjectInspectionState.Ok, project.Name, results);
        }
    }
}
