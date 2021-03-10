using System.IO;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

using App.Inspection.Metrics;

namespace App.Inspection
{
    /// <summary>
    /// Stores the result of an analysis of a project.
    /// </summary>
    public sealed class InspectionResult
    {
        public IReadOnlyCollection<ProjectInspectionResult> Projects;

        internal InspectionResult(IReadOnlyCollection<ProjectInspectionResult> projects)
        {
            Projects = projects;
        }
    }

    public sealed class ProjectInspectionResult
    {
        internal static ProjectInspectionResult Ok(Project project, IReadOnlyCollection<PackageInspectionResult> results)
        {
            return new ProjectInspectionResult(ProjectInspectionState.Ok, project.Name, results);
        }

        internal static ProjectInspectionResult Ignored(Project project)
        {
            return new ProjectInspectionResult(ProjectInspectionState.Ignored, project.Name, new List<PackageInspectionResult>());
        }

        internal static ProjectInspectionResult LoadFailed(FileSystemInfo file)
        {
            var name = Path.GetFileName(file.FullName);
            
            return new ProjectInspectionResult(ProjectInspectionState.LoadFailed, name, new List<PackageInspectionResult>());
        }
        
        internal static ProjectInspectionResult CompilationFailed(Project project)
        {
            return new ProjectInspectionResult(ProjectInspectionState.CompilationFailed, project.Name, new List<PackageInspectionResult>());
        }
        
        public string Name { get; }
        
        public ProjectInspectionState State { get; }
        
        public IReadOnlyCollection<PackageInspectionResult> Packages { get; }

        internal ProjectInspectionResult(ProjectInspectionState state, string name, IReadOnlyCollection<PackageInspectionResult> packages)
        {
            Name = name;
            State = state;
            Packages = packages;
        }
    }

    public enum ProjectInspectionState
    {
        /// <summary>
        /// Indicates the analysis ran to completion successfully.
        /// </summary>
        Ok,
        
        /// <summary>
        /// Indicates the analysis was skipped because the project is ignored by the user.
        /// </summary>
        Ignored,
        
        /// <summary>
        /// Indicates the load of the project failed.
        /// </summary>
        LoadFailed,
        
        /// <summary>
        /// Indicates the compilation of the corresponding project failed.
        /// </summary>
        CompilationFailed
    }

    public sealed class PackageInspectionResult
    {
        private readonly Package _package;
        
        public IReadOnlyCollection<IMetricResult> Metrics { get; }

        public string Name => _package.Namespace.Value;

        internal PackageInspectionResult(Package package, IReadOnlyCollection<IMetricResult> metrics)
        {
            _package = package;
            Metrics = metrics;
        }
    }
    
}
