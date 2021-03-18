using System;
using System.Linq;
using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Text;

using NuGet.Versioning;

using App.Inspection;
using App.Inspection.Metrics;
using App.Inspection.Packages;
using App.Inspection.Extensions;

namespace App.Rendering
{
    internal class MetricsTableView : StackLayoutView
    {
        public static MetricsTableView CreateFromResult(ProjectInspectionResult result, ITableFilter filter)
        {
            var view = new MetricsTableView();
            var table = new TableView<MetricsTableLine>
            {
                Items = GetItems(result, filter)
            };
            
            table.AddColumn(
                line => line.Index.ToString().PadLeft(2), 
                "#".PadLeft(2)
            );

            table.AddColumn(
                line => FormatPackageName(line.Package, line.Depth),
                new ContentView("Package".PadRight(35))
            );
            
            table.AddColumn(
                line => line.Version.ToString().PadLeft(7),
                new ContentView("Version")
            );
            
            table.AddColumn(
                line => line.PackageType.PadRight(10),
                new ContentView("Type".PadRight(10))
            );
            
            table.AddColumn(
                line => FormatFloat(line.Usage, 12),
                new ContentView("Usage (%)".PadLeft(12))
            );
            
            table.AddColumn(
                line => FormatFloat(line.Scatter, 12),
                new ContentView("Scatter (%)".PadLeft(12))
            );
            
            table.AddColumn(
                line => line.TransientCount.ToString().PadLeft(17),
                new ContentView("# of Dependencies")
            );
            
            view.Add(table);
            
            return view;

            static string FormatPackageName(string name, int depth, int padding = 35)
            {
                if (depth == 0)
                {
                    return name.PadRight(padding);
                }
                
                return string.Concat(new string(' ', depth * 2), name).PadRight(padding);
            }
            
            static string FormatFloat(float? value, int padding = 0)
            {
                if (!value.HasValue)
                {
                    return "-".PadLeft(padding);
                }

                return $"{value:0.00}".PadLeft(padding);
            }
        }

        private MetricsTableView() : base(Orientation.Vertical)
        { }
        
        private static IReadOnlyList<MetricsTableLine> GetItems(ProjectInspectionResult project, ITableFilter filter)
        {
            var lines = new List<MetricsTableLine>();
            var index = 1;
            
            var roots = project.PackageResults
                .Where(result => result.Package.ReferenceType == PackageReferenceType.Explicit)
                .OrderBy(result => result.Package.Name);
            
            foreach (var root in roots)
            {
                foreach (var line in GetTableLines(project, root, filter, 0))
                {
                    line.Index = index++;
                    
                    lines.Add(line);
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

                line.Index = index++;
                lines.Add(line);
            }
            
            return lines;
        }

        private static List<MetricsTableLine> GetTableLines(ProjectInspectionResult project, PackageInspectionResult result, ITableFilter filter, int depth)
        {
            var lines = new List<MetricsTableLine>();
            
            var (package, metrics) = result;

            if (filter.IsLineExcluded(package))
            {
                return lines;
            }
            
            lines.Add(CreateTableLine(package, metrics, depth));

            var children = project.PackageResults
                .Where(r => r.Package.Parents?.Any(parent => parent.ID == package.ID) ?? false)
                .OrderBy(r => r.Package.Name);
            
            foreach (var child in children)
            {
                foreach (var line in GetTableLines(project, child, filter, depth + 1))
                {
                    lines.Add(line);
                }
            }
            
            return lines;
        }

        private static MetricsTableLine CreateTableLine(IPackage package, IReadOnlyCollection<IMetricResult?> metrics, int depth)
        {
            float? usageValue = null;
            float? scatterValue = null;

            foreach (var metric in metrics)
            {
                if (metric is UsageMetricResult usage)
                    usageValue = usage.Percentage;

                if (metric is ScatteringMetricResult scatter)
                    scatterValue = scatter.Percentage;
            }

            return new MetricsTableLine
            {
                ID = package.ID,
                Depth = depth,
                Package = package.Name,
                PackageType = Enum.GetName(package.ReferenceType) ?? "<unknown>",
                Version = package.Version,
                Usage = usageValue,
                Scatter = scatterValue,
                TransientCount = package.GetUniqueDependenciesCount()
            };
        }
        
        private sealed class MetricsTableLine
        {
            #nullable disable warnings
            
            public Guid ID { get; set; }
            
            public int Index { get; set; }
            
            public int Depth { get; set; }
            public string Package { get; set; }
            public string PackageType { get; set; }
            public SemanticVersion Version { get; set; }
            public float? Usage { get; set; }
            public float? Scatter { get; set; }
            public int TransientCount { get; set; }
            
            #nullable restore warnings
        }
    }
}
