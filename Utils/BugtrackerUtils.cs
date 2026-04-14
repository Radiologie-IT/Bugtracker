using System;
using System.IO;
using Bugtracker.Logging;
using System.Diagnostics;
using System.Text;
using Bugtracker.Capture.Screen;
using Bugtracker.Configuration;
using Bugtracker.Console;
using Bugtracker.Globals_and_Information;
using System.Collections.Generic;
using System.Linq;
using Bugtracker.Variables;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.IO.Compression;
using Bugtracker.Problem_Descriptors;
using System.Threading.Tasks;

namespace Bugtracker.Utils
{
    public static class BugtrackerUtils
    {
        /// <summary>
        /// Run bugtracker as CMDlet using
        /// ConsoleHandler.
        ///
        /// As this project is define as windows
        /// forms application system.console.writeline (..)
        /// wont work, so we have to manually include
        /// kernel32.dll.
        ///
        /// For more info look into ConsoleHandler.cs
        /// </summary>
        public static void StartCommandLineApplication(string[] args)
        {
            // create console window
            ConsoleHandler.Create();

            // create new bugtrackerConsole object
            //BugtrackConsole console = new BugtrackConsole();

            // start console version of bugtracker
            BugtrackConsole.StartBugtrackerConsoleLogic(args);


            // close console session
            ConsoleHandler.Destroy();
        }

        /// <summary>
        /// Check if a directory exists with a timeout to prevent hanging on unreachable network shares.
        /// Default timeout is 1000ms (1 second), which is appropriate for network path checks during startup.
        /// </summary>
        /// <param name="path">The directory path to check</param>
        /// <param name="timeoutMilliseconds">Maximum time to wait in milliseconds (default: 1000ms)</param>
        /// <returns>True if directory exists and check completed within timeout; False if doesn't exist or timeout exceeded</returns>
        public static bool DirectoryExistsWithTimeout(string path, int timeoutMilliseconds = 1000)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            try
            {
                var task = Task.Run(() => Directory.Exists(path));

                if (task.Wait(timeoutMilliseconds))
                {
                    return task.Result;
                }
                else
                {
                    Logger.Log($"Directory existence check timed out after {timeoutMilliseconds}ms for path: {path}", LoggingSeverity.Debug);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error checking directory existence for '{path}': {ex.Message}", LoggingSeverity.Debug);
                return false;
            }
        }

        internal static void FirstStartupProcedure(RunningConfiguration runningConfiguration, object rc)
        {
            throw new NotImplementedException();
        }

        internal static void FirstStartupProcedure(RunningConfiguration rc)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create current bugtrack folder
        /// </summary>
        public static DirectoryInfo CreateBugtrackFolder()
        {
            // build name
            string folderName = CreateBugtrackFolderName();

            // full path 
            string fullPath = Path.Join(Globals.TMP_DIRECTORY, folderName);

            // create folder
            Logger.Log("Creating new bugtrack folder at '" + fullPath + "'", (LoggingSeverity)2);
            DirectoryInfo currentBugtrackFolder = new(fullPath);
            currentBugtrackFolder.Create();
            Logger.Log("Tmp folder created", (LoggingSeverity)2);

            return currentBugtrackFolder;
        }


        /// <summary>
        /// Create Bugtracker folder name
        /// </summary>
        public static string CreateBugtrackFolderName()
        {
            //To see 
            //
            //
            //

            // first part of folder name
            var bugtrackFolderName = "Bugtracker_";

            //
            if (PCInfo.IsRemoteSession)
                bugtrackFolderName += $"{PCInfo.Clientname}_on_{PCInfo.Hostname}";
            else
                bugtrackFolderName += PCInfo.Clientname;

            // build date format
            DateTime dt = DateTime.Now;
            string date = dt.ToString("_dd-MM-yy_HH-mm-ss");

            // finish folder name
            bugtrackFolderName += date;

            if(RunningConfiguration.GetInstance().SelectedProblemCategory != null)
                bugtrackFolderName += RunningConfiguration.GetInstance().SelectedProblemCategory.TicketAbbreviation;


            // return
            return bugtrackFolderName;
        }

        /// <summary>
        /// Generates screenshots of all
        /// monitors
        /// </summary>
        /// <returns>Screen capture file path</returns>
        public static string GenerateScreenCapture(bool inSequence = false)
        {
            // Handler class for screenshots
            var screenCaptureHandler = new ScreenCaptureHandler();

            // do it
            return screenCaptureHandler.GenerateScreenshot(RunningConfiguration.GetInstance().NewestBugtrackerFolder.FullName, inSequence);
        }


        /// <summary>
        /// Copys full direcotry
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="copySubDirs"></param>
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            var dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);

                file.CopyTo(tempPath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (!copySubDirs) return;
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }


        /// <summary>
        /// Executes Scripts or small programs
        /// can execute ps1, bat and exe files.
        /// </summary>
        /// <param name="path"></param>
        public static void ExecuteScript(string path)
        {
            if (path.Contains(".ps1"))
            {
                var ps1File = path;
                var startInfo = new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy unrestricted -file \"{ps1File}\"",
                    UseShellExecute = false
                };
                Process.Start(startInfo);
            }

            if (path.Contains(".bat") || path.Contains(".exe"))
            {
                System.Diagnostics.Process.Start(path);
            }
        }

        /// <summary>
        /// Loads file content as string
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string LoadFileContentAsString(string path)
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }

        /// <summary>
        /// Delete content of directory
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteContentOfDirectory(string path)
        {
            System.IO.DirectoryInfo di = new(path);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        /// <summary>
        /// Return all existing directories
        /// </summary>
        /// <param name="toCheck"></param>
        /// <returns></returns>
        public static List<DirectoryInfo> GetAllExisitingDirectories(List<DirectoryInfo> toCheck)
        {
            foreach (var di in toCheck.Where(di => di.Exists == false))
            {
                toCheck.Remove(di);
            }

            return toCheck;
        }

        /// <summary>
        /// Creates directory in bugtrack folder where the 
        /// different logfiles and screenshots will be gathered,
        /// zipped and sent out to management server 
        /// After successfully sending the file it gets deleted
        /// </summary>
        public static void CreateTempFolder()
        {
            // check if tmp path exits
            if (!File.Exists(Globals.TMP_DIRECTORY))
            {
                Logger.Log("Creating tmp directory at '" + Globals.TMP_DIRECTORY + "'", (LoggingSeverity)2);
                DirectoryInfo tmpfolder = new(Globals.TMP_DIRECTORY);
                tmpfolder.Create();
                Logger.Log("Tmp folder created", (LoggingSeverity)2);
            }
        }
    }
}
