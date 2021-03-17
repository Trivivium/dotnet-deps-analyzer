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

            table.AddColumn(line => 
                line.Package.PadRight(35),
                new ContentView("Package".PadRight(35))
            );
            
            table.AddColumn(
                line => formatter.Format(line.Usage),
                new ContentView("Usage (%)".PadLeft(12))
            );
            
            table.AddColumn(
                line => formatter.Format(line.Scatter),
                new ContentView("Scatter (%)".PadLeft(12))
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

                foreach (var metric in package.Metrics)
                {
                    if (metric is UsageMetricResult usage)
                        usageValue = usage.Percentage;

                    if (metric is ScatteringMetricResult scatter)
                        scatterValue = scatter.Percentage;
                }
                
                items.Add(new MetricsTableLine(package.Name, usageValue, scatterValue));
            }
            
            return items;
        }
        
        private sealed class MetricsTableLine
        {
            public readonly string Package;
            public readonly float? Usage;
            public readonly float? Scatter;

            public MetricsTableLine(string package, float? usage, float? scatter)
            {
                Package = package;
                Usage = usage;
                Scatter = scatter;
            }
        }
    }
}
