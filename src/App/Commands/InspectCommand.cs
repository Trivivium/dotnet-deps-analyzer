using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using System.CommandLine;
using System.CommandLine.Rendering;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering.Views;

using App.Logging;
using App.Extensions;
using App.Inspection;
using App.Inspection.Metrics;
using App.Inspection.Exceptions;

namespace App.Commands
{
    public class InspectCommand : Command
    {
        // These fields has the 'Ctor' prefix to avoid name conflicts with similar protected properties it inherits.
        private const string CtorName = "inspect";
        private const string CtorDescription = "Inspects a specific project or solution and generates a report of the results";
        
        public InspectCommand() : base(CtorName, CtorDescription)
        {
            foreach (var option in CreateArguments())
            {
                Add(option);
            }

            Handler = CommandHandler.Create((InspectCommandArgs args, IConsole console, InvocationContext ctx) => Handle(args, console, ctx));
        }

        private static async Task<int> Handle(InspectCommandArgs args, IConsole console, InvocationContext ctx)
        {
            var terminal = new SystemConsoleTerminal(console);
            var inspector = new Inspector(new SystemConsoleLoggerAdapter(terminal, args.Verbose));
            
            try
            {
                var results = await Inspect(inspector, args);
                
                WriteResults(ctx, terminal, results);
            }
            catch (InspectionException exception)
            {
                terminal.WriteErrorLine(exception.Message);

                return 1;
            }
            
            return 0;
        }
        
        private static async Task<InspectionResult> Inspect(Inspector inspector, InspectCommandArgs args)
        {
            // TODO: Construct parameters from command line arguments.
            var parameters = InspectionParameters.CreateWithDefaults(
                Enumerable.Empty<string>(), 
                new []{ "IdeaVault." }, 
                Enumerable.Empty<string>()
            );
            
            return await inspector.InspectAsync(args.Path, parameters, CancellationToken.None);
        }

        private static void WriteResults(InvocationContext context, ITerminal terminal, InspectionResult inspection)
        {
            var maxLength = inspection.Projects.SelectMany(p => p.Packages).Max(p => p.Name.Length);
            
            // Manually add underlines to the header.
            var delimiter = new InspectionTableRow
            {
                Package = new string('-', maxLength),
                Usage = new string('-', "Usage (%)".Length),
                Scatter = new string('-', "Scattering (%)".Length)
            };

            var frames = new StackLayoutView
            {
                new ContentView("\n"),
                new ContentView($"Date: {DateTimeOffset.UtcNow}"),
                new ContentView("\n")
            };
            
            foreach (var project in inspection.Projects)
            {
                var items = new List<InspectionTableRow>
                {
                    delimiter
                };
                
                foreach (var package in project.Packages)
                {
                    float? usageValue = null;
                    float? scatterValue = null;
                    
                    foreach (var metric in package.Metrics)
                    {
                        switch (metric)
                        {
                            case UsageMetricResult usage:
                                usageValue = usage.Percentage;
                                break;

                            case ScatteringMetricResult scatter:
                                scatterValue = scatter.Percentage;
                                break;
                        }
                    }
                    
                    items.Add(new InspectionTableRow
                    {
                        Package = package.Name,
                        Usage   = usageValue?.ToString("#.##") ?? "n/a",
                        Scatter = scatterValue?.ToString("#.##") ?? "n/a"
                    });
                }
                
                var table = new TableView<InspectionTableRow>
                {
                    Items = items
                };
            
                table.AddColumn(row => row.Package, new ContentView("Package"));
                table.AddColumn(row => row.Usage,   new ContentView("Usage (%)"));
                table.AddColumn(row => row.Scatter, new ContentView("Scattering (%)"));

                var grid = new GridView();
                
                grid.SetColumns(ColumnDefinition.Fixed(5), ColumnDefinition.SizeToContent());
                grid.SetRows(RowDefinition.SizeToContent());
                
                grid.SetChild(table, 1, 0);
                
                frames.Add(new StackLayoutView(Orientation.Vertical)
                {
                    new ContentView(project.Name),
                    grid,
                    new ContentView("\n")
                });
            }

            frames.Add(new ContentView("\n"));

            var renderer = new ConsoleRenderer(terminal, context.BindingContext.OutputMode(), false);
            
            using var screen = new ScreenView(renderer, terminal)
            {
                Child = frames
            };
            
            var region = renderer.GetRegion();
            
            screen.Render(new Region(0, Console.CursorTop, region.Width, region.Height));
        }

        private static IEnumerable<Symbol> CreateArguments()
        {
            yield return new Argument<FileInfo>("path")
            {
                Arity = ArgumentArity.ExactlyOne,
                Description = "An absolute path to the .csproj or .sln file of the project/solution to inspect"
            }.ExistingOnly();
            
            yield return new Option<bool>(new [] { "--verbose", "-v" }, "Enables verbose logging");

            yield return new Option<bool>("--headless", "Disables any any formatting of the console output (e.g., the progress indicator)");
        }
    }

    internal class InspectionTableRow
    {
        public string Package = string.Empty;
        public string Usage = string.Empty;
        public string Scatter = string.Empty;
    }
}
