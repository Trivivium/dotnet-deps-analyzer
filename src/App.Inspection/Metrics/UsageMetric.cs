using System.Linq;

using Microsoft.CodeAnalysis;

namespace App.Inspection.Metrics
{
    internal sealed class UsageMetric : IMetric
    {
        public IMetricResult Compute(Project project, Compilation compilation, Registry registry, Package package)
        {
            // Find registry matches for the package.
            var matches = registry.GetMatchesForPackage(package).ToList();

            var percentage = matches.Count / (float) package.UniqueTypeCount * 100;
            var locations = matches.Select(match => new SourceFileLocation(
                match.Location?.File ?? "<unknown>",
                match.Location?.Line ?? 0
            )).ToList();

            return new UsageMetricResult(percentage, locations);
        }
    }
}
