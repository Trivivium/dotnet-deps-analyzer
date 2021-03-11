using System.CommandLine.Rendering;

namespace App.Extensions
{
    internal static class StringExtensions
    {
        public static TextSpan Underlined(this string value)
        {
            return new ContainerSpan(StyleSpan.UnderlinedOn(), new ContentSpan(value), StyleSpan.UnderlinedOff());
        }
    }
}
