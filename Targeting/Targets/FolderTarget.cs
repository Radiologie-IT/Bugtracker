using Bugtracker.Configuration;
using Bugtracker.Globals_and_Information;
using Bugtracker.Logging;
using Bugtracker.Problem_Descriptors;
using Bugtracker.Utils;
using System;
using System.IO;
using static Bugtracker.Logging.Log;

namespace Bugtracker.Targeting.Targets
{
    /// <summary>
    /// Target that copies bugtracker folders to a network path or local folder
    /// </summary>
    [TargetType("folder")]
    public class FolderTarget : Target
    {
        public override string TypeIdentifier => "folder";

        /// <summary>
        /// The destination path where bugtracker folders should be copied
        /// </summary>
        [XmlConfig("path", required: true)]
        public string Path { get; set; }

        /// <summary>
        /// The network address of the target (for display purposes)
        /// </summary>
        [XmlConfig("address")]
        public string Address { get; set; }

        /// <summary>
        /// Optional custom folder name template with variable substitution
        /// If not specified, uses the original bugtracker folder name
        /// </summary>
        [XmlConfig("foldername", applyVariables: false)]
        public string CustomBugtrackerFolderName { get; set; }

        public override bool ValidateConfiguration(out string errorMessage)
        {
            if (string.IsNullOrEmpty(Path))
            {
                errorMessage = "Folder target requires 'path' attribute";
                return false;
            }

            // Check if path exists (only check parent folder for network paths)
            try
            {
                string pathToCheck = Path;

                // For UNC paths, just check if the server is reachable
                if (Path.StartsWith("\\\\"))
                {
                    // Extract server name from UNC path
                    string[] parts = Path.TrimStart('\\').Split('\\');
                    if (parts.Length > 0)
                    {
                        pathToCheck = $"\\\\{parts[0]}";
                    }
                }

                if (!Directory.Exists(pathToCheck))
                {
                    errorMessage = $"Path does not exist or is not accessible: {Path}";
                    Logger.Log(errorMessage, LoggingSeverity.Warning);
                    // Return true anyway - path might become available later
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Cannot validate path {Path}: {ex.Message}";
                Logger.Log(errorMessage, LoggingSeverity.Warning);
                // Return true anyway - path might become available later
                return true;
            }

            errorMessage = null;
            return true;
        }

        public override SendResult Send(ProblemDescriptor problemDescriptor = null)
        {
            if (string.IsNullOrEmpty(Path))
            {
                return SendResult.Fail("Path is not configured");
            }

            try
            {
                bool useCustomBTFolderName = !string.IsNullOrEmpty(CustomBugtrackerFolderName);
                string resolvedCustomFolderName = null;

                // Resolve custom bugtracker folder name with variable substitution
                if (useCustomBTFolderName)
                {
                    resolvedCustomFolderName = ResolveCustomFolderName(CustomBugtrackerFolderName, problemDescriptor);
                }

                var bugtrackerFolders = RunningConfiguration.GetInstance().BugtrackerFolders;

                if (bugtrackerFolders.Count == 0)
                {
                    return SendResult.Fail("No bugtracker folders to send");
                }

                foreach (DirectoryInfo di in bugtrackerFolders)
                {
                    string bugtrackerFolderName = useCustomBTFolderName ? resolvedCustomFolderName : di.Name;
                    string targetPath = System.IO.Path.Combine(Path, bugtrackerFolderName);

                    // Create bugtracker folder at target path
                    Directory.CreateDirectory(targetPath);

                    // Create problem description file
                    string problemDescFileName = problemDescriptor?.ProblemCategory != null
                        ? $"{problemDescriptor.ProblemCategory.Name}_Problem_Description"
                        : "Problem_Description";

                    CreateProblemDescriptionFile(System.IO.Path.Combine(targetPath, problemDescFileName), problemDescriptor);

                    // Copy content of bugtracker folder to target path
                    BugtrackerUtils.DirectoryCopy(di.FullName, targetPath, true);

                    // Create blackhole folder at target path
                    string blackholePath = System.IO.Path.Combine(targetPath, "blackhole");
                    Directory.CreateDirectory(blackholePath);

                    // Copy content of blackhole folder to target path
                    BugtrackerUtils.DirectoryCopy(Globals.LOCAL_BLACKHOLE_FODLER_PATH, blackholePath, true);

                    Logger.Log($"Successfully copied bugtracker folder to {targetPath}", LoggingSeverity.Info);
                }

                return SendResult.Ok($"Successfully copied {bugtrackerFolders.Count} bugtracker folder(s) to {Path}");
            }
            catch (Exception ex)
            {
                string errorMsg = $"Failed to copy bugtracker folders to {Path}: {ex.Message}";
                Logger.Log(errorMsg, LoggingSeverity.Error);
                return SendResult.Fail(errorMsg, ex);
            }
        }

        public override string GetSummary()
        {
            return base.GetSummary() +
                   $"\nPath: {Path}" +
                   $"\nAddress: {Address ?? "n/a"}" +
                   (string.IsNullOrEmpty(CustomBugtrackerFolderName) ? "" : $"\nFolder Name: {CustomBugtrackerFolderName}");
        }
    }
}
