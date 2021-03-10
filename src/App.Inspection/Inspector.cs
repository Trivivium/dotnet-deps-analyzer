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
        
        public async Task<ICollection<InspectionResult>> InspectAsync(FileSystemInfo file, InspectionParameters parameters, CancellationToken ct)
        {
            MSBuildLocator.RegisterDefaults();

            using(var workspace = MSBuildWorkspace.Create())
            {
                var context = new InspectionContext(file, workspace, parameters);
                
                if (file.HasExtension(".csproj"))
                {
                    _logger.LogVerbose($"Inspecting project: {file}");
                    
                    var result = await InspectProject(context, ct);
                    
                    return new List<InspectionResult>
                    {
                        result
                    };
                }
            
                if (file.HasExtension(".sln"))
                {
                    _logger.LogVerbose($"Inspecting solution: {file}");
                    
                    return await InspectSolution(context, ct);
                }
                
                throw new InspectionException("The specified file path is not a reference to a solution or project file");
            }
        }
        
        private async Task<InspectionResult> InspectProject(InspectionContext context, CancellationToken ct)
        {
            var project = await context.Workspace.OpenProjectAsync(context.File.FullName, progress: null, ct);

            if (project is null)
            {
                return InspectionResult.LoadFailed(context.File);
            }
            
            var metrics = context.CreateMetricInstances(_logger, context.Parameters);
            
            return await ExecuteMetricsAsync(project, metrics, ct);
        }

        private async Task<ICollection<InspectionResult>> InspectSolution(InspectionContext context, CancellationToken ct)
        {
            var solution = await context.Workspace.OpenSolutionAsync(context.File.FullName, progress: null, ct);

            if (solution is null)
            {
                throw new InspectionException("Failed to load the solution");
            }

            var results = new List<InspectionResult>(solution.ProjectIds.Count);

            foreach (Project project in solution.Projects)
            {
                InspectionResult result;

                if (context.Parameters.IsProjectExcluded(project))
                {
                    _logger.LogInformation($"Skipping ignored project: {project.Name}");
                    
                    result = InspectionResult.Ignored(project);
                }
                else
                {
                    // A new set of metrics is instantiated for each project to avoid concurrency issues
                    // if the process is parallelized in the future.
                    var metrics = context.CreateMetricInstances(_logger, context.Parameters);
                    
                    result = await ExecuteMetricsAsync(project, metrics, ct);
                }
                
                results.Add(result);
            }

            return results;
        }

        private async Task<InspectionResult> ExecuteMetricsAsync(Project project, IReadOnlyCollection<IMetric> metrics, CancellationToken ct)
        {
            _logger.LogInformation($"Inspecting project: {project.Name}");
            
            var compilation = await project.GetCompilationAsync(ct);

            if (compilation is null)
            {
                _logger.LogError($"Failed to compile project: {project.Name}");
                
                return InspectionResult.CompilationFailed(project);
            }

            foreach (var tree in compilation.SyntaxTrees)
            {
                var model = compilation.GetSemanticModel(tree);

                foreach (var metric in metrics)
                {
                    await metric.CollectAsync(project, compilation, tree, model, ct);
                }
            }

            var results = new List<IMetricResult>(metrics.Count);

            foreach (var metric in metrics)
            {
                results.Add(metric.Compute());
            }

            return InspectionResult.Ok(project, results);
        }
    }
}
