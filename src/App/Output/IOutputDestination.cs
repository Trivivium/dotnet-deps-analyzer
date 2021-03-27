using System.Collections.Generic;
using System.Threading.Tasks;

using App.Inspection;

namespace App.Output
{
    public interface IOutputDestination
    {
        public Task GenerateFromResults(IAsyncEnumerable<ProjectInspectionResult> results);
    }
}
