using App.Inspection;

namespace App.Rendering
{
    public class NoopProgressIndicator : IInspectionProgress
    {
        public void Begin(int count, string title)
        { }

        public void BeginTask(string title)
        { }

        public void CompleteTask()
        { }

        public void Complete()
        { }
    }
}
