namespace App.Inspection.Metrics
{
    public sealed class SourceFileLocation
    {
        public string File { get; }
        public int Line { get; }

        public SourceFileLocation(string file, int line)
        {
            File = file;
            Line = line;
        }
    }
}
