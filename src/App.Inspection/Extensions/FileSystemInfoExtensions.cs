using System;
using System.IO;

namespace App.Inspection.Extensions
{
    internal static class FileSystemInfoExtensions
    {
        /// <summary>
        /// Checks if a file has the provided <paramref name="extension"/>.
        /// </summary>
        /// <param name="info">The file to check.</param>
        /// <param name="extension">The extension to check for.</param>
        /// <returns></returns>
        public static bool HasExtension(this FileSystemInfo info, string extension)
        {
            return info.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase);
        }
    }
}
