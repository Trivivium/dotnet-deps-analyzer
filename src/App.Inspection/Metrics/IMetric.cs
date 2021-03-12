using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

namespace App.Inspection.Metrics
{
    internal interface IMetric
    {
        public IMetricResult Compute(Project project, Compilation compilation, Package package, Registry registry);
    }
}
