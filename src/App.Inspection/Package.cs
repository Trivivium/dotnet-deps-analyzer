using System.Diagnostics;

namespace App.Inspection
{
    [DebuggerDisplay("{Name,nq}")]
    public class Package
    {
        /// <summary>
        /// The portable executable (DLL) file on disk where exported types of the package
        /// is loaded.
        /// </summary>
        internal readonly PortableExecutableWrapper Executable;

        /// <summary>
        /// The name of the package.
        /// </summary>
        public string Name;
        
        internal Package(string name, PortableExecutableWrapper executable)
        {
            Name = name;
            Executable = executable;
        }
    }
}
