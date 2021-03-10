using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

namespace App.Inspection.Metrics
{
    internal sealed class UsageMetric : IMetric
    {
        private readonly InspectionParameters _parameters;

        public UsageMetric(InspectionParameters parameters)
        {
            _parameters = parameters;
        }

        public string GetName()
        {
            return "usage";
        }
        
        public Task CollectAsync(Project project, Compilation compilation, SyntaxTree tree, SemanticModel model, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        public IMetricResult Compute()
        {
            // TODO: Implement computation logic of the usage metric.
            
            return new UsageMetricResult();
        }
    }
}
