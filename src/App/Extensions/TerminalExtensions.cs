using System;
using System.CommandLine.IO;
using System.CommandLine.Rendering;

namespace App.Extensions
{
    internal static class TerminalExtensions
    {
        public static void WriteLine(this ITerminal terminal, string text)
        {
            terminal.Out.WriteLine(text);    
        }
        
        public static void WriteErrorLine(this ITerminal terminal, string text)
        {
            terminal.ForegroundColor = ConsoleColor.Red;
            terminal.Error.WriteLine(text);
            terminal.ResetColor();
        }
    }
}
