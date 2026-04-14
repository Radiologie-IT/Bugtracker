#if WEBUPLOAD
using BugTrackerUploader;
using Bugtracker.Configuration;
using Bugtracker.Logging;
using Bugtracker.Problem_Descriptors;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Bugtracker.Logging.Log;

namespace Bugtracker.Targeting.Targets
{
    /// <summary>
    /// Target that uploads bugtracker folders to a web server via HTTP REST API.
    /// Uses the BugTrackerUploader library to communicate with the Django backend.
    /// </summary>
    [TargetType("webupload")]
    public class WebUploadTarget : Target
    {
        public override string TypeIdentifier => "webupload";

        /// <summary>
        /// The URL of the BugTracker web server (e.g., http://bugtracker.radiologie.intern:9000)
        /// </summary>
        [XmlConfig("serverurl", required: true)]
        public string ServerUrl { get; set; }

        /// <summary>
        /// Whether to enable verbose logging output from the uploader
        /// </summary>
        [XmlConfig("verbose")]
        public bool Verbose { get; set; } = false;

        public override bool ValidateConfiguration(out string errorMessage)
        {
            if (string.IsNullOrEmpty(ServerUrl))
            {
                errorMessage = "WebUpload target requires 'serverurl' attribute";
                return false;
            }

            // Debug: Log the actual ServerUrl value
            Logger.Log($"Validating ServerUrl: '{ServerUrl}' (length: {ServerUrl.Length})", LoggingSeverity.Debug);

            // Trim whitespace and quotes that might have been added during XML parsing
            ServerUrl = ServerUrl?.Trim().Trim('"', '\'');
            Logger.Log($"After trimming: '{ServerUrl}' (length: {ServerUrl.Length})", LoggingSeverity.Debug);

            // Validate URL format
            if (!Uri.TryCreate(ServerUrl, UriKind.Absolute, out Uri uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                errorMessage = $"Invalid server URL: '{ServerUrl}'. Must be a valid HTTP or HTTPS URL.";
                return false;
            }

            // Try to ping the server (optional validation)
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var response = client.GetAsync($"{ServerUrl.TrimEnd('/')}/api/").Result;
                    // Don't fail if server is unreachable during validation - just log a warning
                    if (!response.IsSuccessStatusCode)
                    {
                        Logger.Log($"Warning: Server at {ServerUrl} returned status {response.StatusCode}", LoggingSeverity.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Warning: Could not connect to server at {ServerUrl}: {ex.Message}", LoggingSeverity.Warning);
                // Don't fail validation - server might be temporarily unavailable
            }

            errorMessage = null;
            return true;
        }

        public override SendResult Send(ProblemDescriptor problemDescriptor = null)
        {
            if (string.IsNullOrEmpty(ServerUrl))
            {
                return SendResult.Fail("Server URL is not configured");
            }

            try
            {
                // Create uploader instance
                var uploader = new BugTrackerUploaderLib(ServerUrl, Verbose);

                var bugtrackerFolders = RunningConfiguration.GetInstance().BugtrackerFolders;

                if (bugtrackerFolders.Count == 0)
                {
                    return SendResult.Fail("No bugtracker folders to upload");
                }

                int successCount = 0;
                int totalCount = bugtrackerFolders.Count;
                string lastBugtrackerId = null;

                foreach (var folderInfo in bugtrackerFolders)
                {
                    try
                    {
                        // Generate ticket number from problem descriptor or use auto-generated
                        string ticket = GenerateTicketNumber(problemDescriptor);

                        // Get problem description
                        string description = problemDescriptor?.ProblemDescription;
                        if (string.IsNullOrEmpty(description))
                        {
                            description = problemDescriptor?.ProblemCategory?.Name ?? "Automatischer Upload";
                        }

                        // Get problem category name for backend
                        string problemCategoryName = problemDescriptor?.ProblemCategory?.Name;

                        Logger.Log($"Uploading folder '{folderInfo.Name}' to {ServerUrl}...", LoggingSeverity.Info);
                        if (!string.IsNullOrEmpty(problemCategoryName))
                        {
                            Logger.Log($"Problem category: {problemCategoryName}", LoggingSeverity.Info);
                        }

                        // Upload folder using the library
                        // Use Task.Run to avoid deadlock when calling async method synchronously
                        var result = Task.Run(async () => await uploader.UploadFolderAsync(
                            folderPath: folderInfo.FullName,
                            ticket: ticket,
                            description: description,
                            problem: problemCategoryName,
                            recursive: true
                        )).Result;

                        lastBugtrackerId = result.bugtrackerId;
                        successCount++;

                        Logger.Log($"Successfully uploaded '{folderInfo.Name}': {result.uploadedCount} files, BugTracker ID: {result.bugtrackerId}", LoggingSeverity.Info);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Failed to upload folder '{folderInfo.Name}': {ex.Message}", LoggingSeverity.Error);
                        // Continue with next folder instead of failing completely
                    }
                }

                if (successCount == 0)
                {
                    return SendResult.Fail($"Failed to upload all {totalCount} bugtracker folder(s)");
                }
                else if (successCount < totalCount)
                {
                    string webUrl = uploader.GetWebInterfaceUrl(lastBugtrackerId);
                    return new SendResult
                    {
                        Success = true,
                        Message = $"Partially successful: Uploaded {successCount} of {totalCount} folder(s). Last BugTracker ID: {lastBugtrackerId}",
                        Url = webUrl
                    };
                }
                else
                {
                    string webUrl = uploader.GetWebInterfaceUrl(lastBugtrackerId);
                    return new SendResult
                    {
                        Success = true,
                        Message = $"Successfully uploaded {successCount} bugtracker folder(s)",
                        Url = webUrl
                    };
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Failed to upload to web server {ServerUrl}: {ex.Message}";
                Logger.Log(errorMsg, LoggingSeverity.Error);
                return SendResult.Fail(errorMsg, ex);
            }
        }

        /// <summary>
        /// Generates a ticket number for the upload.
        /// Uses problem category abbreviation if available, otherwise generates timestamp-based ticket.
        /// </summary>
        private string GenerateTicketNumber(ProblemDescriptor problemDescriptor)
        {
            if (problemDescriptor?.ProblemCategory != null)
            {
                string abbrev = problemDescriptor.ProblemCategory.TicketAbbreviation;
                if (!string.IsNullOrEmpty(abbrev))
                {
                    return $"{abbrev.ToUpper()}-{DateTime.Now:yyyyMMdd-HHmmss}";
                }
            }

            // Fallback: Auto-generated ticket
            return $"AUTO-{DateTime.Now:yyyyMMdd-HHmmss}";
        }

        public override string GetSummary()
        {
            return base.GetSummary() +
                   $"\nServer URL: {ServerUrl}" +
                   $"\nVerbose: {Verbose}";
        }
    }
}
#endif
