using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.CodeAnalysis;

namespace App.Inspection
{
    public sealed class ProjectInspectionResult
    {
        internal static ProjectInspectionResult Ok(Project project, IReadOnlyCollection<PackageInspectionResult> results)
        {
            return new ProjectInspectionResult(ProjectInspectionState.Ok, project.Name, TimeSpan.Zero, results);
        }

        internal static ProjectInspectionResult Ignored(Project project)
        {
            return new ProjectInspectionResult(ProjectInspectionState.Ignored, project.Name, TimeSpan.Zero, new List<PackageInspectionResult>());
        }

        internal static ProjectInspectionResult LoadFailed(FileSystemInfo file)
        {
            var name = Path.GetFileName(file.FullName);
            
            return new ProjectInspectionResult(ProjectInspectionState.LoadFailed, name, TimeSpan.Zero, new List<PackageInspectionResult>());
        }
        
        internal static ProjectInspectionResult CompilationFailed(Project project)
        {
            return new ProjectInspectionResult(ProjectInspectionState.CompilationFailed, project.Name, TimeSpan.Zero, new List<PackageInspectionResult>());
        }
        
        public string Name { get; }
        
        public ProjectInspectionState State { get; }
        
        public TimeSpan Elapsed { get; }
        
        public IReadOnlyCollection<PackageInspectionResult> Packages { get; }

        internal ProjectInspectionResult(ProjectInspectionState state, string name, TimeSpan elapsed, IReadOnlyCollection<PackageInspectionResult> packages)
        {
            Name = name;
            State = state;
            Elapsed = elapsed;
            Packages = packages;
        }
    }
}
