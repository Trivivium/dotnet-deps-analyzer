namespace App.Inspection
{
    /// <summary>
    /// Declares the state of the results of an analysis of a project.
    /// </summary>
    public enum InspectionResultState
    {
        /// <summary>
        /// Indicates the analysis ran to completion successfully.
        /// </summary>
        Ok,
        
        /// <summary>
        /// Indicates the analysis was skipped because the project is ignored by the user.
        /// </summary>
        Ignored,
        
        /// <summary>
        /// Indicates the load of the project failed.
        /// </summary>
        LoadFailed,
        
        /// <summary>
        /// Indicates the compilation of the corresponding project failed.
        /// </summary>
        CompilationFailed
    }
}
