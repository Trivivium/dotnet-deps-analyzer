using System;
using System.Linq;
using System.Collections.Generic;
using System.CommandLine.Rendering.Views;

using NuGet.Versioning;

using App.Inspection;
using App.Inspection.Packages;
using App.Output.Filters;

namespace App.Output.Console
{
    internal class MetricsTableView : TableView<(int Index, MetricsTableLine Line)>
    {
        private MetricsTableView()
        { }
        
        public static MetricsTableView CreateFromResult(ProjectInspectionResult result, IResultsFilter filter, MetricTableLineConverter converter)
        {
            var view = new MetricsTableView
            {
                Items = converter.GetIndexedTableLines(result, filter)
            };
            
            view.AddIndexColumn();
            
            view.AddColumn(
                header: "Package",
                padding: 35, 
                selector: item => FormatPackageName(item.Package, item.Depth)
            );
            
            view.AddColumn(
                header: "Version",
                selector: item => FormatPackageVersion(item.PackageVersion)
            );
            
            view.AddColumn(
                header: "Type",
                selector: item => FormatPackageType(item.PackageType)
            );

            view.AddConditionalColumn(
                header:    "Usage",
                condition: items => items.Any(item => item.Line.Usage != null),
                selector:     item => item.Usage
            );
            
            view.AddConditionalColumn(
                header:    "Scattering",
                condition: items => items.Any(item => item.Line.Scatter != null),
                selector:  item => item.Scatter
            );
            
            view.AddConditionalColumn(
                header:    "Transitive Count",
                condition: items => items.Any(item => item.Line.TransientCount != null),
                selector:  item => item.TransientCount
            );
            
            return view;
        }
        
        private static string FormatFloat(float? value)
        {
            return value.HasValue
                ? $"{value:0.00}"
                : "-";
        }

        private static string FormatPackageName(string name, int depth)
        {
            return depth == 0
                ? name
                : string.Concat(new string(' ', depth * 2), name);
        }

        private static string FormatPackageVersion(SemanticVersion version)
        {
            return version.ToString();
        }

        private static string FormatPackageType(PackageReferenceType type)
        {
            return Enum.GetName(type) ?? "<unknown>";
        }
        
        private void AddIndexColumn()
        {
            AddColumn(item => item.Index.ToString().PadLeft(2), " #");
        }
        
        private void AddColumn(string header, Func<MetricsTableLine, string> selector)
        {
            AddColumn(header, padding: null, ColumnAlign.Left, selector);
        }
        
        private void AddColumn(string header, int? padding, Func<MetricsTableLine, string> selector)
        {
            AddColumn(header, padding, ColumnAlign.Left, selector);
        }
        
        private void AddColumn(string header, int? padding, ColumnAlign align, Func<MetricsTableLine, string> selector)
        {
            var pad = padding ?? header.Length;

            if (align == ColumnAlign.Left)
            {
                AddColumn(item => selector(item.Line).PadRight(pad), new ContentView(header.PadRight(pad)));
            }
            else
            {
                AddColumn(item => selector(item.Line).PadLeft(pad), new ContentView(header.PadLeft(pad)));
            }
        }

        private void AddConditionalColumn(string header, Func<IReadOnlyList<(int Index, MetricsTableLine Line)>, bool> condition, Func<MetricsTableLine, int?> selector)
        {
            if (condition(Items))
            {
                AddColumn(header, padding: null, ColumnAlign.Right, item =>
                {
                    var number = selector(item);

                    if (!number.HasValue || number == 0)
                    {
                        return "-";
                    }

                    return number.Value.ToString();
                });
            }
        }
        
        private void AddConditionalColumn(string header, Func<IReadOnlyList<(int Index, MetricsTableLine Line)>, bool> condition, Func<MetricsTableLine, float?> selector)
        {
            if (condition(Items))
            {
                AddColumn(header, padding: null, ColumnAlign.Right, item => FormatFloat(selector(item)));
            }
        }
        
        private enum ColumnAlign
        {
            Left,
            Right
        }
    }
}
