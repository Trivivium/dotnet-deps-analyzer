using System.Linq;
using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;

using App.Inspection;
using App.Inspection.Metrics;

namespace App.Rendering
{
    internal class MetricsTableView : StackLayoutView
    {
        public static MetricsTableView CreateFromResult(ProjectInspectionResult result)
        {
            var formatter = new TextSpanFormatter();
            
            formatter.AddFormatter<float>(f =>
            {
                if (f == 0f)
                {
                    return new ContentSpan("-".PadLeft(12));
                }

                return new ContentSpan($"{f:0.00}".PadLeft(12));
            });
            
            var view = new MetricsTableView();
            var table = new TableView<MetricsTableLine>
            {
                Items = GetItems(result)
            };

            table.AddColumn(
                line => line.Package.PadRight(35),
                new ContentView("Package".PadRight(35))
            );
            
            table.AddColumn(
                line => line.Version.PadLeft(7),
                new ContentView("Version")
            );
            
            table.AddColumn(
                line => line.PackageType,
                new ContentView("Type")
            );
            
            table.AddColumn(
                line => formatter.Format(line.Usage),
                new ContentView("Usage (%)".PadLeft(12))
            );
            
            table.AddColumn(
                line => formatter.Format(line.Scatter),
                new ContentView("Scatter (%)".PadLeft(12))
            );
            
            table.AddColumn(
                line => line.TransientCount.ToString().PadLeft(16),
                new ContentView("# of Dependencies")
            );
            
            view.Add(table);
            
            return view;
        }

        private MetricsTableView() : base(Orientation.Vertical)
        { }
        
        private static IReadOnlyList<MetricsTableLine> GetItems(ProjectInspectionResult result)
        {
            var items = new List<MetricsTableLine>(result.Packages.Count);

            foreach (var package in result.Packages.OrderBy(p => p.Name))
            {
                float? usageValue = null;
                float? scatterValue = null;
                int transientCount = 0;

                foreach (var metric in package.Metrics)
                {
                    if (metric is UsageMetricResult usage)
                        usageValue = usage.Percentage;

                    if (metric is ScatteringMetricResult scatter)
                        scatterValue = scatter.Percentage;

                    if (metric is TransientCountMetricResult transients)
                        transientCount = transients.Count;
                }

                items.Add(new MetricsTableLine
                {
                    Package = package.Name,
                    PackageType = package.Type,
                    Version = package.Version,
                    Usage = usageValue,
                    Scatter = scatterValue,
                    TransientCount = transientCount
                });
            }
            
            return items;
        }
        
        private sealed class MetricsTableLine
        {
            #nullable disable warnings
            
            public string Package { get; set; }
            public string PackageType { get; set; }
            public string Version { get; set; }
            public float? Usage { get; set; }
            public float? Scatter { get; set; }
            public int TransientCount { get; set; }
        }
    }
}
