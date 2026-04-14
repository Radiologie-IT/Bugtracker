using Bugtracker.Logging;
using Bugtracker.Problem_Descriptors;
using Bugtracker.Utils;
using System;
using System.IO;
using static Bugtracker.Logging.Log;

namespace Bugtracker.Targeting.Targets
{
    /// <summary>
    /// Target that executes a PowerShell script with bugtracker data as parameters
    /// </summary>
    [TargetType("powershell")]
    public class PowershellTarget : Target
    {
        public override string TypeIdentifier => "powershell";

        /// <summary>
        /// Path to the PowerShell script file (fallback if downloadLink fails)
        /// </summary>
        [XmlConfig("path", required: true)]
        public string Path { get; set; }

        /// <summary>
        /// HTTP/HTTPS URL to download the script from
        /// </summary>
        [XmlConfig("downloadLink")]
        public string DownloadLink { get; set; }

        /// <summary>
        /// Local path where to save the downloaded script
        /// Defaults to [InstallDir]\scripts\[filename] if not specified
        /// </summary>
        [XmlConfig("saveAs")]
        public string SaveAs { get; set; }

        /// <summary>
        /// Whether to pass the variable dictionary as a hashtable to the script
        /// </summary>
        [XmlConfig("passvariables")]
        public bool PassVariables { get; set; } = false;

        /// <summary>
        /// Whether to pass the list of bugtracker folders to the script
        /// </summary>
        [XmlConfig("passfolders")]
        public bool PassFolders { get; set; } = false;

        /// <summary>
        /// Whether to pass the selected problem category to the script
        /// </summary>
        [XmlConfig("passproblemcat")]
        public bool PassProblemCategory { get; set; } = false;

        /// <summary>
        /// Whether to log the default output stream
        /// </summary>
        [XmlConfig("logdefault")]
        public bool LogDefault { get; set; } = true;

        /// <summary>
        /// Whether to log the error stream
        /// </summary>
        [XmlConfig("logerrors")]
        public bool LogErrors { get; set; } = true;

        /// <summary>
        /// Whether to log the warning stream
        /// </summary>
        [XmlConfig("logwarnings")]
        public bool LogWarnings { get; set; } = true;

        /// <summary>
        /// Whether to log the information stream
        /// </summary>
        [XmlConfig("loginformations")]
        public bool LogInformations { get; set; } = true;

        /// <summary>
        /// Whether to log the progress stream
        /// </summary>
        [XmlConfig("logprogress")]
        public bool LogProgress { get; set; } = false;

        public override bool ValidateConfiguration(out string errorMessage)
        {
            if (string.IsNullOrEmpty(Path))
            {
                errorMessage = "PowerShell target requires 'path' attribute";
                return false;
            }

            // Check if script file exists
            if (!System.IO.File.Exists(Path))
            {
                errorMessage = $"PowerShell script file not found: {Path}";
                Logger.Log(errorMessage, LoggingSeverity.Warning);
                // Return true anyway - script might be created later
                return true;
            }

            // Check if file has .ps1 extension
            if (!Path.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = $"PowerShell script should have .ps1 extension: {Path}";
                Logger.Log(errorMessage, LoggingSeverity.Warning);
                // Return true anyway - might still work
                return true;
            }

            errorMessage = null;
            return true;
        }

        public override SendResult Send(ProblemDescriptor problemDescriptor = null)
        {
            if (string.IsNullOrEmpty(Path))
            {
                return SendResult.Fail("PowerShell script path is not configured");
            }

            // Resolve the script path (download if needed, use cache, or fall back to Path)
            string scriptPath = PowershellUtils.ResolveScriptPath(Path, DownloadLink, SaveAs);

            if (string.IsNullOrEmpty(scriptPath) || !File.Exists(scriptPath))
            {
                return SendResult.Fail($"PowerShell script not found at resolved path: {scriptPath}");
            }

            try
            {
                // Create PowershellExecution object with current settings
                PowershellUtils.PowershellExecution psExec = new PowershellUtils.PowershellExecution(
                    scriptPath,
                    PassVariables,
                    PassFolders,
                    PassProblemCategory,
                    LogDefault,
                    LogErrors,
                    LogWarnings,
                    LogInformations,
                    LogProgress
                );

                // Execute the script
                psExec.Execute();

                Logger.Log($"Successfully executed PowerShell script: {scriptPath}", LoggingSeverity.Info);
                return SendResult.Ok($"Successfully executed PowerShell script: {scriptPath}");
            }
            catch (Exception ex)
            {
                string errorMsg = $"Failed to execute PowerShell script {scriptPath}: {ex.Message}";
                Logger.Log(errorMsg, LoggingSeverity.Error);
                return SendResult.Fail(errorMsg, ex);
            }
        }

        public override string GetSummary()
        {
            string summary = base.GetSummary() +
                   $"\nScript Path: {Path}";

            if (!string.IsNullOrWhiteSpace(DownloadLink))
            {
                summary += $"\nDownload Link: {DownloadLink}";
                summary += $"\nSave As: {(string.IsNullOrWhiteSpace(SaveAs) ? "[Default: InstallDir/scripts/filename]" : SaveAs)}";
            }

            summary += $"\nPass Variables: {PassVariables}" +
                       $"\nPass Folders: {PassFolders}" +
                       $"\nPass Problem Category: {PassProblemCategory}";

            return summary;
        }
    }
}
