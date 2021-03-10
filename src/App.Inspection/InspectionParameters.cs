using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace App.Inspection
{
    public class InspectionParameters
    {
        private readonly ICollection<string> _excludedProjects;
        private readonly ICollection<string> _excludedPackages;

        public InspectionParameters(IEnumerable<string> excludedProjects, IEnumerable<string> excludedPackages)
        {
            _excludedProjects = excludedProjects
                .Select(p => p.ToUpperInvariant())
                .ToList();
            
            _excludedPackages = excludedPackages
                .Select(p => p.ToUpperInvariant())
                .ToList();
        }

        public bool IsProjectExcluded(Project project)
        {
            var name = project.Name.ToUpperInvariant();

            return _excludedProjects.Contains(name);
        }
    }
}
