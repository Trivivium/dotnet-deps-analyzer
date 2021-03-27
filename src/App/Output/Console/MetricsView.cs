using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Linq;
using System.Threading.Tasks;

using App.Commands;
using App.Inspection;
using App.Output.Filters;

namespace App.Output.Console
{
    internal class MetricsView : IOutputDestination
    {
        private static readonly View _newline = new ContentView("\n");
        
        private readonly ConsoleRenderer _renderer;
        private readonly MetricTableLineConverter _converter;
        private readonly bool _showAll;

        public MetricsView(IConsole console, InspectCommandArgs args)
        {
            _renderer = new ConsoleRenderer(console, OutputMode.PlainText);
            _converter = new MetricTableLineConverter();
            _showAll = args.ShowAll;
        }

        public async Task GenerateFromResults(IAsyncEnumerable<ProjectInspectionResult> results)
        {
            await foreach (var project in results)
            {
                var view = GetLayoutView(project);
                
                if (!project.PackageResults.Any())
                {
                    view.Add(new ContentView("-- No packages available --"));
                }
                else
                {
                    view.Add(MetricsTableView.CreateFromResult(project, GetTableFilter(project), _converter));
                }
                
                view.Add(_newline);
                
                view.Render(_renderer, Region.Scrolling);
            }
            
            _newline.Render(_renderer, Region.Scrolling);
        }

        private static LayoutView<View> GetLayoutView(ProjectInspectionResult project)
        {
            return new StackLayoutView(Orientation.Vertical)
            {
                _newline,
                new ContentView($"Project: {project.Name} (took: {project.Elapsed.Seconds}s {project.Elapsed.Milliseconds}ms)"),
                _newline
            };
        }

        private IResultsFilter GetTableFilter(ProjectInspectionResult project)
        {
            IResultsFilter filter = _showAll
                ? new NoopResultsFilter()
                : new HasMetricsResultsFilter(project);

            return filter;
        }
    }
}
