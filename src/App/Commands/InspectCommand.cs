using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using System.CommandLine;
using System.CommandLine.Rendering;
using System.CommandLine.Invocation;

using App.Logging;
using App.Extensions;
using App.Inspection;
using App.Inspection.Exceptions;
using App.Inspection.Metrics;

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
                var results = await Inspect(inspector, args);
                
                WriteResults(terminal, results);
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

        private static void WriteResults(ITerminal terminal, InspectionResult inspection)
        {
            terminal.WriteLine("Results:");

            foreach (var project in inspection.Projects)
            {
                terminal.WriteLine($"  {project.Name}");

                foreach (var package in project.Packages)
                {
                    terminal.WriteLine($"    {package.Name}");

                    foreach (var metric in package.Metrics)
                    {
                        if (metric is UsageMetricResult usage)
                        {
                            terminal.WriteLine($"      {metric.GetDisplayName()}: {usage.Percentage:#.##}%");
                        }

                        if (metric is ScatteringMetricResult scatter)
                        {
                            terminal.WriteLine($"      {metric.GetDisplayName()}: {scatter.Percentage:#.##}%");
                        }
                    }
                }
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
