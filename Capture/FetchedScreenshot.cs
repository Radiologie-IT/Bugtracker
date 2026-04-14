using System.IO;

namespace Bugtracker.Capture
{
    /// <summary>
    /// Represents a fetched screenshot. But isnt used yet.
    /// </summary>
    class FetchedScreenshot
    {
        public DirectoryInfo Directory { get; internal set; }
        public string Name { get; internal set; }
    }
}
