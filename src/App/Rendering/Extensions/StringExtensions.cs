using System.CommandLine.Rendering;

namespace App.Rendering.Extensions
{
    internal static class StringExtensions
    {
        public static TextSpan Underlined(this string value)
        {
            return new ContainerSpan(StyleSpan.UnderlinedOn(), new ContentSpan(value), StyleSpan.UnderlinedOff());
        }
        
        public static TextSpan LightGreen(this string value) =>
            new ContainerSpan(ForegroundColorSpan.LightGreen(),
                new ContentSpan(value),
                ForegroundColorSpan.Reset());
    }
}
