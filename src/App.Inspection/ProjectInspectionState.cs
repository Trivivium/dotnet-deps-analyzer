namespace App.Inspection
{
    public enum ProjectInspectionState
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
