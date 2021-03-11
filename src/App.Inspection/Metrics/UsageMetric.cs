using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

namespace App.Inspection.Metrics
{
    internal sealed class UsageMetric : IMetric
    {
        public IMetricResult Compute(Project project, Compilation compilation, Package package, List<ReferencedSymbol> refs)
        {
            var percentage = (float) refs.Count / package.ExportedTypesCount * 100;
            
            return new UsageMetricResult(percentage);
        }
    }
}
