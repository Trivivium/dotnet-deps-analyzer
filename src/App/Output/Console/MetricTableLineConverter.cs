using System.Collections.Generic;
using System.Linq;

using App.Inspection;
using App.Inspection.Metrics;
using App.Inspection.Packages;
using App.Output.Filters;

namespace App.Output.Console
{
    internal class MetricTableLineConverter
    {
        public IReadOnlyList<(int Index, MetricsTableLine Line)> GetIndexedTableLines(ProjectInspectionResult project, IResultsFilter filter)
        {
            var lines = new List<(int Index, MetricsTableLine Line)>();
            var index = 1;
            
            var roots = project.PackageResults
                .Where(result => result.Package.ReferenceType == PackageReferenceType.Explicit)
                .OrderBy(result => result.Package.Name);
            
            foreach (var root in roots)
            {
                foreach (var line in GetTableLines(project, root, filter, 0))
                {
                    lines.Add((index++, line));
                }
            }

            var unknowns = project.PackageResults
                .Where(result => result.Package.ReferenceType == PackageReferenceType.Unknown)
                .OrderBy(result => result.Package.Name);

            foreach (var (package, metrics) in unknowns)
            {
                if (filter.IsLineExcluded(package))
                {
                    continue;
                }
                
                var line = CreateTableLine(package, metrics, 0);
                
                lines.Add((index++, line));
            }
            
            return lines;
        }

        private static IEnumerable<MetricsTableLine> GetTableLines(ProjectInspectionResult project, PackageInspectionResult result, IResultsFilter filter, int depth)
        {
            var (package, metrics) = result;

            if (filter.IsLineExcluded(package))
            {
                yield break;
            }

            yield return CreateTableLine(package, metrics, depth);

            var children = project.PackageResults
                .Where(r => r.Package.Parents?.Any(parent => parent.ID == package.ID) ?? false)
                .OrderBy(r => r.Package.Name);
            
            foreach (var child in children)
            {
                foreach (var line in GetTableLines(project, child, filter, depth + 1))
                {
                    yield return line;
                }
            }
        }

        private static MetricsTableLine CreateTableLine(IPackage package, IReadOnlyCollection<IMetricResult?> metrics, int depth)
        {
            var line = new MetricsTableLine(package.ID, depth, package.Name, package.ReferenceType, package.Version);

            foreach (var metric in metrics)
            {
                if (metric is UsageMetricResult usage)
                    line.Usage = usage.Percentage;

                if (metric is ScatteringMetricResult scatter)
                    line.Scatter = scatter.Percentage;

                if (metric is TransitiveCountMetricResult transitiveCount)
                    line.TransientCount = transitiveCount.Count;
            }

            return line;
        }
    }
}
