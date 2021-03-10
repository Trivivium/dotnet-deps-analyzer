using System.IO;
using System.Linq;
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
        internal static InspectionResult Ok(Project project, IEnumerable<IMetricResult> metrics)
        {
            return new InspectionResult(InspectionResultState.Ok, project.Name, metrics);
        }

        internal static InspectionResult Ignored(Project project)
        {
            return new InspectionResult(InspectionResultState.Ignored, project.Name, Enumerable.Empty<IMetricResult>());
        }

        internal static InspectionResult LoadFailed(FileSystemInfo file)
        {
            var name = Path.GetFileName(file.FullName);
            
            return new InspectionResult(InspectionResultState.LoadFailed, name, Enumerable.Empty<IMetricResult>());
        }
        
        internal static InspectionResult CompilationFailed(Project project)
        {
            return new InspectionResult(InspectionResultState.CompilationFailed, project.Name, Enumerable.Empty<IMetricResult>());
        }

        /// <summary>
        /// Declares the state of the analysis result.
        /// </summary>
        public InspectionResultState State { get; }
        
        /// <summary>
        /// Declares the name of the project analyzed.
        /// </summary>
        public string ProjectName { get; }

        public IReadOnlyCollection<IMetricResult> Metrics;

        private InspectionResult(InspectionResultState state, string name, IEnumerable<IMetricResult> metrics)
        {
            State = state;
            ProjectName = name;
            Metrics = metrics.ToList();
        }
    }
}
