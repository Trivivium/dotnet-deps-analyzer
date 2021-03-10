using System;
using System.IO;

namespace App.Extensions
{
    internal static class FileInfoExtensions
    {
        public static bool HasExtension(this FileInfo info, string extension)
        {
            return info.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase);
        }
    }
}
