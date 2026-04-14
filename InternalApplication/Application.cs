using Bugtracker.Logging;
using System.Collections.Generic;
using System.IO;
using Bugtracker.Utils;
using Bugtracker.Configuration;

namespace Bugtracker.InternalApplication
{
    public class Application
    {
        public enum ShowAppSpecifier
        {
            installed,
            always,
            never
        }


        #region Properties
        /// <summary>
        /// The Name specifies the name of the Application
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specifies the location of the executable as a string 
        /// </summary>
        public string ExecutableLocation { get; set; }

        /// <summary>
        /// Specifies if Application is enabled in Bugtracker
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Specifies if its the Standard Application
        /// </summary>
        public bool IsStandard { get; set; }

        public bool IsInstalled
        {
            get
            {
                int timeout = RunningConfiguration.GetInstance().FileCheckTimeoutMs;

                // Log file check only if LogFiles exist
                if (LogFiles != null && LogFiles.Count > 0 && LogFiles[0] != null)
                {
                    bool logPathExists = !string.IsNullOrEmpty(LogFiles[0].Path) && BugtrackerUtils.DirectoryExistsWithTimeout(LogFiles[0].Path, timeout);
                    Logger.Log("File check exist: " + Name + ": " + logPathExists, LoggingSeverity.Debug);
                }

                // Check if executable location exists (handle null safely, use timeout to prevent network share hangs)
                return !string.IsNullOrEmpty(ExecutableLocation) && BugtrackerUtils.DirectoryExistsWithTimeout(ExecutableLocation, timeout);
            }
        }

        /// <summary>
        /// Specifier if show "onExist" -> show Application in when installed on PC
        /// </summary>
        public ShowAppSpecifier ShowSpecifier { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<Log> LogFiles { get; set; }

        public string? PreFetchExecutionPath { get; set; }
        public string? PostFetchExecutionPath { get; set; }

        public List<PowershellUtils.PowershellExecution> PowershellPre { get; set; }
        public List<PowershellUtils.PowershellExecution> PowershellPost { get; set; }


        #endregion

        public Application()
        {
            LogFiles = new List<Log>();

            PowershellPre = new List<PowershellUtils.PowershellExecution>();
            PowershellPost = new List<PowershellUtils.PowershellExecution>();
        }

        public override string ToString()
        {
            return "Name: " + this.Name + "\t \t" +
                   "Executable: " + this.ExecutableLocation + "\t" +
                   "Enabled: " + this.Enabled + "\t" +
                   "Standard: " + this.IsStandard + "\t" +
                   "Show: " + this.ShowSpecifier;
        }

        public void ExecutePreFetching()
        {
            if(PreFetchExecutionPath != null)
                BugtrackerUtils.ExecuteScript(PreFetchExecutionPath);
        }

        public void ExecutePostFetching()
        {
            if(PostFetchExecutionPath != null)
                BugtrackerUtils.ExecuteScript(PostFetchExecutionPath);
        }
    }
}
