using System.IO;

namespace Bugtracker.Capture
{
    /// <summary>
    /// Represents a fetched log file. But isnt used yet.
    /// </summary>
    class FetchedLogFile
    {
        public DirectoryInfo Directory { get; internal set; }
        public string Name { get; internal set; }
    }
}
