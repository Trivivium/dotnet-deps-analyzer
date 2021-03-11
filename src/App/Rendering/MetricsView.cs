using System.CommandLine.Rendering;
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
                var table = MetricsTableView.CreateFromResult(project);
                
                view.Add(table);
            }

            view.Add(new ContentView("\n"));

            return view;
        }
        
        private MetricsView() : base(Orientation.Vertical)
        { }
    }
}
