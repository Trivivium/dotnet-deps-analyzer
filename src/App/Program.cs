using System.CommandLine;
using System.Threading.Tasks;

using App.Commands;

namespace App
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var root = new RootCommand("Provides commands to assess the integration of NuGet packages in a C# project.")
            {
                new InspectCommand()
            };
            
            return await root.InvokeAsync(args);
        }
    }
}
