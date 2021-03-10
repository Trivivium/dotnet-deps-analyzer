using System.Linq;

namespace App.Inspection.Metrics
{
    internal sealed class UsageMetric : IMetric
    {
        public IMetricResult Compute(Registry registry, Package package)
        {
            // Find registry matches for the package.
            var matches = registry.GetMatchesForPackage(package).ToList();

            var percentage = matches.Count / (float) package.UniqueTypeCount;
            var locations = matches.Select(match => new SourceFileLocation(
                match.Location?.File ?? "<unknown>",
                match.Location?.Line ?? 0
            )).ToList();

            return new UsageMetricResult(percentage, locations);
        }
    }
}
