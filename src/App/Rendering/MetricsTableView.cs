using System;
using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;

using App.Inspection;
using App.Inspection.Metrics;
using App.Rendering.Extensions;

namespace App.Rendering
{
    internal class MetricsTableView : StackLayoutView
    {
        public static MetricsTableView CreateFromResult(ProjectInspectionResult result)
        {
            var formatter = new TextSpanFormatter();
            
            var view = new MetricsTableView();
            var table = new TableView<MetricsTableLine>
            {
                Items = GetItems(result)
            };

            table.AddColumn(line => 
                line.Package,
                new ContentView("Package".Underlined())
            );
            
            table.AddColumn(
                line => $"{line.Usage:#.##}",
                new ContentView("Usage".Underlined())
            );
            
            table.AddColumn(
                line => $"{line.Scatter:#.##}",
                new ContentView("Scatter".Underlined())
            );

            view.Add(new ContentView("\n"));
            view.Add(new ContentView(formatter.ParseToSpan($"Project: {result.Name.LightGreen()}")));
            view.Add(new ContentView("\n"));
            view.Add(table);
            
            return view;
        }

        private MetricsTableView() : base(Orientation.Vertical)
        { }
        
        private static IReadOnlyList<MetricsTableLine> GetItems(ProjectInspectionResult result)
        {
            var items = new List<MetricsTableLine>(result.Packages.Count);

            foreach (var package in result.Packages)
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
            public string Package;
            public float? Usage;
            public float? Scatter;

            public MetricsTableLine(string package, float? usage, float? scatter)
            {
                Package = package;
                Usage = usage;
                Scatter = scatter;
            }
        }
    }
}
