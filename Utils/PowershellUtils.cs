using Bugtracker.Configuration;
using Bugtracker.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bugtracker.Utils
{
    public static class PowershellUtils
    {
        /// <summary>
        /// Resolves the PowerShell script path by attempting download from downloadLink,
        /// falling back to cached file or scriptPath
        /// </summary>
        /// <param name="scriptPath">The fallback script path</param>
        /// <param name="downloadLink">Optional HTTP/HTTPS URL to download from</param>
        /// <param name="saveAs">Optional local path to save downloaded script</param>
        /// <returns>The resolved script path to use</returns>
        public static string ResolveScriptPath(string scriptPath, string downloadLink = null, string saveAs = null)
        {
            // If no downloadLink is configured, use scriptPath directly
            if (string.IsNullOrWhiteSpace(downloadLink))
            {
                Logger.Log($"No downloadLink configured, using scriptPath: {scriptPath}", LoggingSeverity.Debug);
                return scriptPath;
            }

            // Determine the saveAs path
            string targetPath = saveAs;
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                // Default to [InstallDir]\scripts\[filename]
                string scriptDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts");
                string fileName = Path.GetFileName(new Uri(downloadLink).AbsolutePath);
                targetPath = Path.Combine(scriptDir, fileName);
                Logger.Log($"No saveAs configured, using default path: {targetPath}", LoggingSeverity.Debug);
            }

            // Ensure the directory exists
            string directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                    Logger.Log($"Created directory for script download: {directory}", LoggingSeverity.Debug);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to create directory {directory}: {ex.Message}", LoggingSeverity.Warning);
                }
            }

            // Try to download the script
            if (TryDownloadScript(downloadLink, targetPath))
            {
                Logger.Log($"Successfully downloaded script from {downloadLink} to {targetPath}", LoggingSeverity.Info);
                return targetPath;
            }

            // Download failed, check if cached file exists from previous execution
            if (File.Exists(targetPath))
            {
                Logger.Log($"Download failed, using cached script from previous execution: {targetPath}", LoggingSeverity.Warning);
                return targetPath;
            }

            // Both download and cache failed, fall back to scriptPath
            Logger.Log($"Download and cache failed, falling back to scriptPath: {scriptPath}", LoggingSeverity.Warning);
            return scriptPath;
        }

        /// <summary>
        /// Attempts to download a script from a URL to a local path
        /// </summary>
        /// <param name="url">The URL to download from</param>
        /// <param name="targetPath">The local path to save to</param>
        /// <returns>True if download succeeded, false otherwise</returns>
        private static bool TryDownloadScript(string url, string targetPath)
        {
            try
            {
                Logger.Log($"Attempting to download PowerShell script from: {url}", LoggingSeverity.Debug);

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    string scriptContent = client.GetStringAsync(url).Result;

                    File.WriteAllText(targetPath, scriptContent);
                    Logger.Log($"Downloaded and saved script to: {targetPath}", LoggingSeverity.Debug);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to download script from {url}: {ex.Message}", LoggingSeverity.Debug);
                return false;
            }
        }

        public class PowershellExecution
        {
            public string Path {  get; set; }
            public string DownloadLink { get; set; }
            public string SaveAs { get; set; }
            public bool PassVariables { get; set; }
            public bool PassFolders { get; set; }
            public bool PassProblemCategory { get; set; }
            public bool LogDefault { get; set; }
            public bool LogErrors { get; set; }
            public bool LogWarnings { get; set; }
            public bool LogInformations { get; set; }
            public bool LogProgress { get; set; }

            /// <summary>
            /// Constructor for Powershell Execution Class
            /// </summary>
            /// <param name="path">location of the powershell script</param>
            /// <param name="passVariables">whether to pass the variable dictionary as a hashmap. Default=false</param>
            /// <param name="passFolders">whether to pass the list of bugtracker folders. Default=false</param>
            /// <param name="passProblemCategory">whether to pass the selected problem category. Default=false</param>
            /// <param name="logDefault">whether to log the default stream. Default=true</param>
            /// <param name="logErrors">whether to log the error stream. Default=true</param>
            /// <param name="logWarnings">whether to log the warning stream. Default=true</param>
            /// <param name="logInformations">whether to log the information stream. Default=true</param>
            /// <param name="logProgress">whether to log the progress stream. Default=false</param>
            public PowershellExecution(string path,
            bool passVariables = false, bool passFolders = false, bool passProblemCategory = false,
            bool logDefault = true, bool logErrors = true, bool logWarnings = true, bool logInformations = true, bool logProgress = false)
            { 
                this.Path = path;
                this.PassVariables = passVariables;
                this.PassFolders = passFolders;
                this.PassProblemCategory = passProblemCategory;
                this.LogDefault = logDefault;
                this.LogErrors = logErrors;
                this.LogWarnings = logWarnings;
                this.LogInformations = logInformations;
                this.LogProgress = logProgress;
            }

            /// <summary>
            /// passes the objects information to PowershellUtils.RunPSScript to execute the script in path
            /// </summary>
            public void Execute()
            {
                // Resolve the script path (download if needed, use cache, or fall back to Path)
                string scriptPath = PowershellUtils.ResolveScriptPath(this.Path, this.DownloadLink, this.SaveAs);

                Logger.Log("Executing Powershell Script: " + scriptPath, LoggingSeverity.Info);
                try
                {
                    PowershellUtils.RunPsScript(scriptPath, this.PassVariables, this.PassFolders, this.PassProblemCategory, this.LogDefault, this.LogErrors, this.LogWarnings, this.LogInformations, this.LogProgress);
                } catch
                {
                    Logger.Log("Error when trying to execute powershell script.", LoggingSeverity.Error);
                }

            }
        }

        /// <summary>
        /// sends the log entries of a Powershell object's various output streams to 
        /// the bugtracker log with appropriate LoggingSeverity
        /// </summary>
        /// <param name="sender">event originator</param>
        /// <param name="evtArgs">event data</param>
        public static void LogPsStreams(object sender, DataAddedEventArgs evtArgs)
        {
            var data = ((System.Collections.IList)sender)[evtArgs.Index];

            switch (data)
            {
                case ErrorRecord er:
                    Logger.Log(er.ToString(), LoggingSeverity.Error);
                    break;
                case WarningRecord wr:
                    Logger.Log(wr.ToString(), LoggingSeverity.Warning);
                    break;
                case ProgressRecord pr:
                    Logger.Log(pr.ToString(), LoggingSeverity.Info);
                    break;
                case InformationRecord ir:
                    Logger.Log(ir.ToString(), LoggingSeverity.Info);
                    break;
                default:
                    Logger.Log(data.ToString(), LoggingSeverity.Info);
                    break;
            }
        }

        /// <summary>
        /// executes a powershell script at a given path.
        /// bugtracker data can be passed to the script as parameters and output stream data can be written to the bugtracker log
        /// </summary>
        /// <param name="path">location of the powershell script</param>
        /// <param name="passVariables">whether to pass the variable dictionary as a hashmap. Default=false</param>
        /// <param name="passFolders">whether to pass the list of bugtracker folders. Default=false</param>
        /// <param name="passProblemCategory">whether to pass the selected problem category. Default=false</param>
        /// <param name="logDefault">whether to log the default stream. Default=true</param>
        /// <param name="logErrors">whether to log the error stream. Default=true</param>
        /// <param name="logWarnings">whether to log the warning stream. Default=true</param>
        /// <param name="logInformations">whether to log the information stream. Default=true</param>
        /// <param name="logProgress">whether to log the progress stream. Default=false</param>
        public static void RunPsScript(string path, 
            bool passVariables=false, bool passFolders=false, bool passProblemCategory=false,
            bool logDefault = true, bool logErrors=true, bool logWarnings=true, bool logInformations=true, bool logProgress=false)
        {
            PowerShell ps = PowerShell.Create();

            ps.AddScript(File.ReadAllText(path))
                .AddParameter("ErrorAction", "Continue")
                .AddParameter("WarningAction", "Continue")
                .AddParameter("InformationAction", "Continue");
            if (passVariables) ps.AddParameter("BugtrackerVariables", PowershellUtils.GetVariableHashmap());
            if (passFolders) ps.AddParameter("BugtrackerFolders", PowershellUtils.GetFolderArray());
            if (passProblemCategory) ps.AddParameter("BugtrackerProblemCategory", PowershellUtils.GetSelectedCategoryString());

            if (logErrors) ps.Streams.Error.DataAdded += PowershellUtils.LogPsStreams;
            if (logWarnings) ps.Streams.Warning.DataAdded += PowershellUtils.LogPsStreams;
            if (logInformations) ps.Streams.Information.DataAdded += PowershellUtils.LogPsStreams;
            if (logProgress) ps.Streams.Progress.DataAdded += PowershellUtils.LogPsStreams;
            
            if(logDefault)
            {
                var outputCollection = new PSDataCollection<PSObject>();
                outputCollection.DataAdded += PowershellUtils.LogPsStreams;

                ps.Invoke<PSObject, PSObject>(null, outputCollection, null);
            }
            else
            {
                ps.Invoke();
            }

            ps.Dispose();
        }

        /// <summary>
        /// converts the RunningConfig's VariableManager's Variable Dictionary to a hashtable that can be used in a powershell script
        /// </summary>
        /// <returns></returns>
        public static Hashtable GetVariableHashmap()
        {
            Dictionary<string,(dynamic value,bool isDynamic)> originalDictionary = RunningConfiguration.GetInstance().Variables.VariableDictionary;
            Hashtable formatted = new Hashtable();

            foreach (KeyValuePair<string, (dynamic value, bool isDynamic)> originalEntry in originalDictionary)
            {
                formatted.Add(originalEntry.Key.ToString(), originalEntry.Value.ToTuple<dynamic, bool>().Item1.ToString());
            }

            return formatted;

        }

        /// <summary>
        /// Converts the RuuningConfigurations' List of BugtrackerFolders to a string array that can be used in powershell scripts
        /// </summary>
        /// <returns></returns>
        public static string[] GetFolderArray()
        {
            List<DirectoryInfo> folders = RunningConfiguration.GetInstance().BugtrackerFolders;
            List<string> psfolders = new List<string>();

            foreach (DirectoryInfo folder in folders)
            {
                psfolders.Add(folder.FullName);
            }

            return psfolders.ToArray();

        }

        /// <summary>
        /// returns the name of the currently selected problem category
        /// </summary>
        /// <returns></returns>
        public static string GetSelectedCategoryString()
        {
            return RunningConfiguration.GetInstance().SelectedProblemCategory is null ? "" : RunningConfiguration.GetInstance().SelectedProblemCategory.Name.ToString();
        }
    }
}
