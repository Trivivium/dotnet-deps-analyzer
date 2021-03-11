using System.Linq;
using System.CommandLine.Rendering.Views;

using App.Inspection;

namespace App.Rendering
{
    public class MetricsView : StackLayoutView
    {
        public static MetricsView Create(InspectionResult result)
        {
            var view = new MetricsView();
            
            foreach (var project in result.Projects)
            {
                view.Add(new ContentView("\n"));
                view.Add(new ContentView($"Project: {project.Name}"));
                view.Add(new ContentView("\n"));
                
                if (!project.Packages.Any())
                {
                    view.Add(new ContentView("-- No packages found in this project --"));
                    
                    continue;
                }
                
                var table = MetricsTableView.CreateFromResult(project);
                
                view.Add(table);
            }

            view.Add(new ContentView("\n"));
            view.Add(new ContentView("\n"));

            return view;
        }
        
        private MetricsView() : base(Orientation.Vertical)
        { }
    }
}
