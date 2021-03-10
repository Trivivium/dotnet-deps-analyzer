using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using System.CommandLine;
using System.CommandLine.Rendering;
using System.CommandLine.Invocation;
using System.Linq;

using App.Rendering;
using App.Extensions;
using App.Inspection;

namespace App.Commands
{
    public class InspectCommand : Command
    {
        public InspectCommand() : base("inspect", "Inspects a specific project or solution and generates a report of the results")
        {
            foreach (var option in CreateArguments())
            {
                Add(option);
            }

            Handler = CommandHandler.Create((InspectCommandArgs args, IConsole console) => Handle(args, console));
        }

        private static async Task<int> Handle(InspectCommandArgs args, IConsole console)
        {
            ITerminal terminal = new SystemConsoleTerminal(console);
            
            try
            {
                IInspectionProgress progress = IsProgressIndicatorDisabled(args, terminal)
                    ? new NoopProgressIndicator()
                    : new ConsoleProgressIndicator(terminal, Console.BufferWidth);

                try
                {
                    var inspector = new Inspector(new CommandLineConsoleLogger(console, args.Verbose));

                    return await Inspect(inspector, progress, terminal, args);
                }
                finally
                {
                    if (progress is IAsyncDisposable disposable)
                    {
                        await disposable.DisposeAsync();
                    }
                }
            }
            catch (Exception)
            {
                terminal.WriteErrorLine($"Failed to inspect: {args.Path}");

                throw;
            }
        }
        
        private static async Task<int> Inspect(Inspector inspector, IInspectionProgress progress, ITerminal terminal, InspectCommandArgs args)
        {
            var path = args.Path;
            
            if (path.HasExtension(".sln"))
            {
                var parameters = new InspectionParameters(
                    Enumerable.Empty<string>(),
                    Enumerable.Empty<string>()
                );
                
                await inspector.InspectSolution(progress, args.Path, parameters);
                
                return 0;
            }
        
            if (path.HasExtension(".csproj"))
            {
                inspector.InspectProject(args.Path);
                
                return 0;
            }
            
            terminal.WriteErrorLine("The provided path points to a file of an unsupported file type.");
            
            return 1;
        }

        private static bool IsProgressIndicatorDisabled(InspectCommandArgs args, ITerminal terminal)
        {
            return args.Verbose || args.Headless || terminal.IsOutputRedirected;
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
