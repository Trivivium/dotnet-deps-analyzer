namespace App.Inspection
{
    public interface IInspectionProgress
    {
        /// <summary>
        /// Begins the display of the progress indicator in the console.
        /// </summary>
        /// <param name="count">The number of tasks the progress bar should go through. The indicator starts at zero allowing for additional initialization.</param>
        /// <param name="title">The initial title of the progress indicator before the first <see cref="BeginTask"/> is invoked.</param>
        public void Begin(int count, string title);

        /// <summary>
        /// Changes the title of the current task in the progress indicator.
        /// </summary>
        /// <param name="title"></param>
        public void BeginTask(string title);
        
        /// <summary>
        /// Steps the progress indicator to the first completion task.
        /// </summary>
        public void CompleteTask();
        
        /// <summary>
        /// Informs the progress indicator that all steps has been completed and it can be removed.
        /// </summary>
        public void Complete();
    }
}
