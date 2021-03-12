using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace App.Inspection
{
    public sealed class ProjectInspectionResult
    {
        internal static ProjectInspectionResult Ok(Project project, IReadOnlyCollection<PackageInspectionResult> results)
        {
            return new ProjectInspectionResult(project.Name, results);
        }

        internal static ProjectInspectionResult Ignored(Project project)
        {
            return new ProjectInspectionResult(project.Name, new List<PackageInspectionResult>());
        }

        public string Name { get; }
        
        public TimeSpan Elapsed { get; internal set; }
        
        public IReadOnlyCollection<PackageInspectionResult> Packages { get; }

        private ProjectInspectionResult(string name, IReadOnlyCollection<PackageInspectionResult> packages, TimeSpan? elapsed = null)
        {
            Name = name;
            Elapsed = elapsed ?? TimeSpan.Zero;
            Packages = packages;
        }
    }
}
