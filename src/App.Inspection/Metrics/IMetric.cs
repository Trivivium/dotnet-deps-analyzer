using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

namespace App.Inspection.Metrics
{
    internal interface IMetric
    {
        public string GetName();
        
        public Task CollectAsync(Project project, Compilation compilation, SyntaxTree tree, SemanticModel model, CancellationToken ct);

        public IMetricResult Compute();
    }

    public interface IMetricResult
    {
        /// <summary>
        /// Gets the name of the metric for display purposes.
        /// </summary>
        public string GetDisplayName();
    }
}
