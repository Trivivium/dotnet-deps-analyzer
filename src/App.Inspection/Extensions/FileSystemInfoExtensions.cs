using System;
using System.IO;

namespace App.Inspection.Extensions
{
    internal static class FileSystemInfoExtensions
    {
        public static bool HasExtension(this FileSystemInfo info, string extension)
        {
            return info.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase);
        }
    }
}
