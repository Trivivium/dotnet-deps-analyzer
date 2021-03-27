using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

using App.Factories;
using App.Inspection;
using App.Inspection.Exceptions;
using App.Logging;
using App.Output;
using App.Output.Console;

namespace App.Commands
{
    public class InspectCommand : Command
    {
        // These fields has the 'Ctor' prefix to avoid name conflicts with similar protected properties it inherits.
        private const string CtorName = "inspect";
        private const string CtorDescription = "Inspects the integration of NuGet packages in a C# solution or a specific project.";
        
        public InspectCommand() : base(CtorName, CtorDescription)
        {
            foreach (var option in CreateArguments())
            {
                Add(option);
            }

            Handler = CommandHandler.Create((InspectCommandArgs args, IConsole console) =>
            {
                return Handle(args, console, CancellationToken.None);
            });
        }

        private static async Task<int> Handle(InspectCommandArgs args, IConsole console, CancellationToken ct)
        {
            var inspector = new CSharpInspector(new SystemConsoleLoggerAdapter(console, args.Verbose));
            
            if (!InspectionParametersFactory.TryCreate(args, out var parameters, out var message))
            {
                console.Error.WriteLine(message);
                
                return 1;
            }

            var output = GetOutputDestination(console, args);
            
            try
            {
                var results = inspector.InspectAsync(args.Path, parameters, ct);

                await output.GenerateFromResults(results);
            }
            catch (InspectionException exception)
            {
                console.Error.WriteLine(exception.Message);

                return 1;
            }

            return 0;
        }

        private static IOutputDestination GetOutputDestination(IConsole console, InspectCommandArgs args)
        {
            return new MetricsView(console, args);
        }
        
        private static IEnumerable<Symbol> CreateArguments()
        {
            yield return new Argument<FileInfo>("path")
            {
                Arity = ArgumentArity.ExactlyOne,
                Description = "An absolute file path to the .csproj or .sln file of the project/solution to inspect"
            }.ExistingOnly();

            yield return new Option<bool>(
                new[]
                {
                    "--verbose",
                    "-v"
                },
                "Enables verbose logging of the inspection process."
            );

            yield return new Option<string>(
                new[]
                {
                    "--metrics"
                },
                "An optional comma-separated list of metrics to compute for all projects. The defaults is all."
            );

            yield return new Option<string>(
                new[]
                {
                    "--excluded-namespaces"
                },
                "An optional comma-separated list of namespaces to exclude from the inspection results." +
                "A namespace is matched as a prefix to the types of a package. The option is additive to the " +
                "defaults: 'System', 'Microsoft', and '.NETStandard'."
            );

            yield return new Option<string>(
                new[]
                {
                    "--excluded-projects"
                },
                "An optional comma-separated list of project names to exclude from the inspection results." +
                "The default is none."
            );

            yield return new Option<bool>(
                new []
                {
                    "--show-all"
                },
                "Enables showing all packages regardless of whether it has metrics relevant for the project. Use " +
                "this to show the entire dependency graph. Note: This shows all packages reachable, which means " +
                "transitive packages inherited from a project reference is also shown."
            );
            
            yield return new Option<int>(
                new[]
                {
                    "--max-concurrency"
                },
                "Determines the max number of tasks inspecting projects in parallel. This only have an effect when " +
                "inspecting a solution with more than 1 project. The default is the number of logical CPU cores on " +
                "the system."
            );
        }
    }
}
