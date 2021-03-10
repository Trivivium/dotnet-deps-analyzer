using System;
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
        
        private static async Task<ICollection<InspectionResult>> Inspect(Inspector inspector, InspectCommandArgs args)
        {
            // TODO: Construct parameters from command line arguments.
            var parameters = InspectionParameters.CreateWithDefaults(
                Enumerable.Empty<string>(), 
                Enumerable.Empty<string>(), 
                new []{ "usage" }
            );
            
            return await inspector.InspectAsync(args.Path, parameters, CancellationToken.None);
        }

        private static void WriteResults(ITerminal terminal, ICollection<InspectionResult> results)
        {
            // TODO: Write the results to the terminal properly.
            terminal.WriteLine("Results:");

            foreach (var result in results)
            {
                terminal.WriteLine($"  {result.ProjectName}: {Enum.GetName(typeof(InspectionResultState), result.State)}");
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
