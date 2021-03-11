using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

namespace App.Inspection.Metrics
{
    internal sealed class ScatteringMetric : IMetric
    {
        public IMetricResult Compute(Project project, Compilation compilation, Package package, List<ReferencedSymbol> refs)
        {
            var uniqueLocationsIds = refs.SelectMany(r => r.Locations).Select(r => r.Document.Id).Distinct().ToList();

            var useCount = project.DocumentIds.Intersect(uniqueLocationsIds).Count();
            var totalCount = project.DocumentIds.Count;

            var percentage = (float) useCount / totalCount * 100;
            
            return new ScatteringMetricResult(percentage);
        }
    }
}
