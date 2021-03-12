using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using System.CommandLine;
using System.CommandLine.Rendering;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering.Views;
using System.Diagnostics;

using App.Logging;
using App.Rendering;
using App.Extensions;
using App.Inspection;
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

            Handler = CommandHandler.Create((InspectCommandArgs args, IConsole console) => Handle(args, console));
        }

        private static async Task<int> Handle(InspectCommandArgs args, IConsole console)
        {
            var terminal = new SystemConsoleTerminal(console);
            var inspector = new Inspector(new SystemConsoleLoggerAdapter(terminal, args.Verbose));
            
            try
            {
                var sw = new Stopwatch();
                
                sw.Restart();

                await Inspect(terminal, inspector, args);
                
                terminal.WriteLine("\n");
                terminal.WriteLine($"Total time elapsed: {sw.Elapsed.Hours}h {sw.Elapsed.Minutes}m {sw.Elapsed.Seconds}s");
            }
            catch (InspectionException exception)
            {
                terminal.WriteErrorLine(exception.Message);

                return 1;
            }
            
            return 0;
        }
        
        private static async Task Inspect(ITerminal terminal, Inspector inspector, InspectCommandArgs args)
        {
            // TODO: Construct parameters from command line arguments.
            var parameters = InspectionParameters.CreateWithDefaults(
                Enumerable.Empty<string>(), 
                Enumerable.Empty<string>(), 
                Enumerable.Empty<string>()
            );
            
            var renderer = new ConsoleRenderer(terminal, OutputMode.PlainText);


            await foreach (var project in inspector.InspectAsync(args.Path, parameters, CancellationToken.None))
            {
                var view = new StackLayoutView
                {
                    new ContentView("\n"),
                    new ContentView($"Project: {project.Name} (took: {project.Elapsed.Seconds}s {project.Elapsed.Milliseconds}ms)"),
                    new ContentView("\n")
                };
                
                if (!project.Packages.Any())
                {
                    view.Add(new ContentView("-- No packages found in this project --"));
                }
                else
                {                
                    view.Add(MetricsTableView.CreateFromResult(project));
                }
                
                view.Add(new ContentView("\n"));
                
                view.Render(renderer, Region.Scrolling);
            }
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
}
