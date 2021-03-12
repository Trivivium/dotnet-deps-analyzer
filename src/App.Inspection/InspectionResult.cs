using System.Collections.Generic;

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
}
