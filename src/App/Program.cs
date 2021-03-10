using System.CommandLine;
using System.Threading.Tasks;

using App.Commands;

namespace App
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var root = new RootCommand("An automatic tool to inspect and assess encapsulation of external dependencies.")
            {
                new InspectCommand()
            };
            
            return await root.InvokeAsync(args);
        }
    }
}
