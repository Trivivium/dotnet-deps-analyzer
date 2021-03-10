using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using App.Inspection.Collectors;

using Microsoft.CodeAnalysis.CSharp;

namespace App.Inspection
{
    public class Inspector
    {
        private readonly ILogger _logger;

        public Inspector(ILogger logger)
        {
            _logger = logger;
        }
        
        public void InspectProject(FileInfo file)
        {
            // TODO: Implement inspection of specific projects.

            throw new NotImplementedException();
        }

        public async Task InspectSolution(IInspectionProgress progress, FileInfo file, InspectionParameters parameters)
        {
            _logger.LogVerbose($"Inspecting solution: {file.FullName}");

            var solution = await GetSolutionAsync(file);
            
            var collector = new UsingDirectiveCollector();

            _logger.LogVerbose($"Found {solution.ProjectIds.Count} projects in solution");
            
            progress.Begin(solution.ProjectIds.Count, "Initializing solution workspace...");
            
            foreach (var project in solution.Projects)
            {
                progress.BeginTask($"Inspecting: {project.Name}");

                if (parameters.IsProjectExcluded(project))
                {
                    _logger.LogInformation($"Ignoring excluded project: {project.Name}");

                    progress.CompleteTask();
                }

                //var metadata = GetProjectMetadata(project, solution);
                var compilation = await GetCompilationAsync(project);

                if (compilation is null)
                {
                    _logger.LogError($"Failed to create compilation of project: {project.Name}, skipping...");
                    
                    continue;
                }

                foreach (var syntaxTree in compilation.SyntaxTrees)
                {
                    var root = await syntaxTree.GetRootAsync();
                
                    collector.Visit(root);
                }
                
                progress.CompleteTask();
            }
            
            progress.Complete();
            
            var usings = collector.UsingDirectives
                .Where(d => !IsDirectiveIgnored(d))
                .ToList();
            
            if (usings.Count > 0)
            {
                _logger.LogInformation("Found the following using directives of external packages:");
                
                foreach (var @using in usings)
                {
                    _logger.LogInformation($"\t{@using}");
                }
            }
            else
            {
                _logger.LogInformation("No using directives found across any of the projects contained in the solution.");
            }
        }

        private static async Task<Solution> GetSolutionAsync(FileSystemInfo file)
        {
            MSBuildLocator.RegisterDefaults();
            
            var msWorkspace = MSBuildWorkspace.Create();

            return await msWorkspace.OpenSolutionAsync(file.FullName);
        }
        
        private async Task<object> GetProjectMetadata(Project project, Solution solution)
        {
            foreach (var reference in project.MetadataReferences.Where(mr => mr.Properties.Kind == MetadataImageKind.Module))
            {
                _logger.LogVerbose($"\t\t{reference.Display}");
            }
            
            foreach (var reference in project.ProjectReferences)
            {
                var p = solution.GetProject(reference.ProjectId);
                
                if (p == null)
                {
                    _logger.LogError($"Cannot find project with ID {reference.ProjectId.Id} in solution: {solution.FilePath ?? "Unknown"}");
                }
            }

            return new object();    // TODO: Return a type with the necessary metadata.
        }

        private async Task<Compilation?> GetCompilationAsync(Project project)
        {
            Compilation? compilation;
                
            try
            {
                compilation = await project.GetCompilationAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Compilation of project {project.Name} failed");

                return null;
            }

            return compilation;
        }
        
        private static async Task AnalyzeProject(Project project, IEnumerable<CSharpSyntaxWalker> collectors)
        {

        }
        
        private static bool IsDirectiveIgnored(in string directive)
        {
            return directive == "System" || directive.StartsWith("System.") || directive.StartsWith("Microsoft.");
        }
    }
}
