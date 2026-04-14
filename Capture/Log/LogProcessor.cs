using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Bugtracker.Configuration;
using Bugtracker.Logging;
using Bugtracker.Utils;
using Windows.ApplicationModel.DataTransfer;
using static System.Environment;

namespace Bugtracker.Capture.Log
{
    public static class LogProcessor
    {
        //private List<InternalApplication.Application> targetApplications;
        private static readonly string bugtrackerFolderPath = RunningConfiguration.GetInstance().NewestBugtrackerFolder.FullName;

        public static event EventHandler RenamedLogs;
        public static event EventHandler DeletedLogs;
        public static event EventHandler FetchedLogs;

        /// <summary>
        /// Returns a list of paths for each log file existing
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, (InternalApplication.Application application, Logging.Log log)> InitLogFilesFromAllTargetedApplications(List<InternalApplication.Application> targetApplications)
        {
            Dictionary<string, (InternalApplication.Application application
                , Logging.Log log)> allLogFiles = new();

            foreach (InternalApplication.Application app in targetApplications)
            {
                foreach (Logging.Log logF in app.LogFiles)
                {
                    Logger.Log("Path: " + logF.Path, LoggingSeverity.Debug);
                    Logger.Log("Filename: " + logF.Filename, LoggingSeverity.Debug);

                    FileInfo[] allFiles = null;
                    FileInfo newestFile = null;

                    DirectoryInfo logDir = new(logF.Path);

                    if (Directory.Exists(logF.Path))
                    {
                        switch(logF.Find)
                        {
                            case Logging.Log.LogFindSpecifier.ALL:
                                allFiles = logDir.GetFiles(logF.Filename);
                                foreach (FileInfo file in allFiles)
                                {
                                    allLogFiles[file.FullName] = (app, logF);
                                }
                                break;

                            case Logging.Log.LogFindSpecifier.NEW:
                                if (logDir.GetFiles(logF.Filename).Length > 0)
                                    newestFile = logDir.GetFiles(logF.Filename).OrderByDescending(f => f.LastWriteTime).First();
                                if (newestFile != null)
                                    allLogFiles[newestFile.FullName] = (app, logF);
                                break;

                            case Logging.Log.LogFindSpecifier.AGE:
                                allFiles = logDir.GetFiles(logF.Filename);
                                foreach (FileInfo file in allFiles)
                                {
                                    DateTime lastModified = file.LastWriteTime;
                                    int fileAge = (int)DateTime.Now.Subtract(lastModified).TotalMinutes;
                                    if ((fileAge >= logF.MinAge && fileAge <= logF.MaxAge))
                                    {
                                        allLogFiles[file.FullName] = (app, logF);
                                    }
                                }
                                break;
                        }

                    }
                }
            }

            return allLogFiles;
        }

        /// <summary>
        /// Method intended to build destination Path
        /// </summary>
        /// <returns></returns>
        public static string BuildDestinationPath()
        {
            // TODO Build Destionation Path
            string destinationPath = "";

            return destinationPath;
        }


        /// <summary>
        /// Deletes all Targeted log files by Application
        /// </summary>
        /// <param name="targetApplications"></param>
        public static void DeleteAllTargeted(List<InternalApplication.Application> targetApplications)
        {
            Dictionary<string, (InternalApplication.Application application, Logging.Log log)> appLogs =
                InitLogFilesFromAllTargetedApplications(targetApplications);

            foreach (string log in appLogs.Keys)
            {
                File.Delete(log);
            }

            DeletedLogs?.Invoke(null, null);
        }

        /// <summary>
        /// Renames All Targeted Application Logs, intended for Log Capture From Timepoint to Timepoint
        /// </summary>
        /// <param name="targetApplications"></param>
        public static void RenameAllTargeted(List<InternalApplication.Application> targetApplications)
        {
            Dictionary<string, (InternalApplication.Application application, Logging.Log log)> appLogs =
                InitLogFilesFromAllTargetedApplications(targetApplications);

            string newFilename;
            string date;
            string filenameWithoutEx;

            foreach (string log in appLogs.Keys)
            {
                date = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss");
                filenameWithoutEx = Path.GetFileNameWithoutExtension(log);

                newFilename = $"{filenameWithoutEx}[{date}].log";

                File.Move(log, $"{Path.GetDirectoryName(log)}\\{newFilename}");
            }

            RenamedLogs?.Invoke(null, null);
        }


        /// <summary>
        /// Copies a log file to the destination directory, optionally limited to the last
        /// <paramref name="lineCount"/> lines. When a line limit is set, uses backwards
        /// seeking to locate the tail boundary so only the required portion of the file
        /// is ever read — no full-file load into memory. When no limit is set, the file
        /// is piped verbatim via a buffered stream copy.
        /// </summary>
        public static void CopyFile(string pathToLogFile, string destinationPath, int? lineCount = null)
        {
            string destinationFile = destinationPath + "\\" + Path.GetFileName(pathToLogFile);

            if (lineCount > 0)
            {
                using var readStream = new FileStream(pathToLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                long startOffset = FindLastLinesOffset(readStream, lineCount.Value);
                readStream.Seek(startOffset, SeekOrigin.Begin);

                using var reader = new StreamReader(readStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                using var writer = new StreamWriter(destinationFile, append: false, Encoding.UTF8);
                string line;
                while ((line = reader.ReadLine()) != null)
                    writer.WriteLine(line);
            }
            else
            {
                // No line limit: copy verbatim via buffered stream copy
                using var readStream = new FileStream(pathToLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var writeStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None);
                readStream.CopyTo(writeStream);
            }

            Logger.Log("log file path: " + destinationFile, LoggingSeverity.Debug);
        }

        /// <summary>
        /// Scans <paramref name="stream"/> backwards in fixed-size chunks to find the byte
        /// offset at which the last <paramref name="lineCount"/> lines begin. At most a few
        /// chunk-reads are needed — the full file is never loaded into memory.
        /// Returns 0 when the file has fewer lines than requested (copy from start).
        /// <para>
        /// UTF-8 safe: 0x0A only ever appears as an actual newline in valid UTF-8;
        /// continuation bytes (0x80–0xBF) never collide with it.
        /// </para>
        /// </summary>
        private static long FindLastLinesOffset(FileStream stream, int lineCount)
        {
            const int bufferSize = 4096;
            byte[] buffer = new byte[bufferSize];
            long fileSize = stream.Length;

            if (fileSize == 0)
                return 0;

            // A file that does not end with '\n' has one unterminated line after the
            // final '\n'. Pre-counting it means the loop condition works uniformly for
            // both trailing-newline and non-trailing-newline files.
            stream.Seek(-1, SeekOrigin.End);
            int newlinesFound = (stream.ReadByte() == '\n') ? 0 : 1;

            long scanPosition = fileSize;

            while (scanPosition > 0)
            {
                int chunkSize = (int)Math.Min(bufferSize, scanPosition);
                scanPosition -= chunkSize;
                stream.Seek(scanPosition, SeekOrigin.Begin);
                int bytesRead = stream.Read(buffer, 0, chunkSize);

                for (int i = bytesRead - 1; i >= 0; i--)
                {
                    if (buffer[i] == '\n' && ++newlinesFound > lineCount)
                        return scanPosition + i + 1;
                }
            }

            // Fewer lines in the file than requested — start from the beginning
            return 0;
        }

        /// <summary>
        /// Fetches All Log Files from targeted applications, main function for log aquiering
        /// </summary>
        /// <param name="targetApplications"></param>
        public static void FetchAllLogFiles(List<InternalApplication.Application> targetApplications)
        {
            Dictionary<string, (InternalApplication.Application application, Logging.Log log)> appLogs = InitLogFilesFromAllTargetedApplications(targetApplications);

            //pre Fetch PS Scripts
            foreach (InternalApplication.Application targetApplication in targetApplications)
            {
                foreach (PowershellUtils.PowershellExecution psex in targetApplication.PowershellPre)
                {
                    psex.Execute();
                }
            }
            

            foreach (string log in appLogs.Keys)
            {
                appLogs.TryGetValue(log, out (InternalApplication.Application application, Logging.Log log) appAndLog);

                FetchLog(log, appAndLog);
            }


            //post Fetch PS Scripts
            foreach (InternalApplication.Application targetApplication in targetApplications)
            {
                foreach (PowershellUtils.PowershellExecution psex in targetApplication.PowershellPost)
                {
                    psex.Execute();
                }
            }

            FetchedLogs?.Invoke(null, null);
        }

        /// <summary>
        /// This procedure fetches the last 2000 
        /// (default value is MAX_LINES_OF_LOG_FILE) lines
        /// </summary>
        /// <param name="logfilePath"></param>
        /// <param name="destination"></param>
        /// <returns>
        /// 
        /// fetchstatus
        /// 0 = successfull
        /// 1 = failed
        /// </returns>
        public static bool FetchLog(string logfilePath, (InternalApplication.Application application, Logging.Log log) appAndLog)
        {
            //pre Fetch Program
            appAndLog.application.ExecutePreFetching();

            // status of fetch
            bool fetch_status = false;

            // check if file exists
            if (File.Exists(logfilePath))
            {

                fetch_status = true;

                // copy file to desired destination
                string path = bugtrackerFolderPath + "\\" + appAndLog.log.LocationType + "\\" +
                              Path.GetDirectoryName(appAndLog.log.Path[3..]);

                Directory.CreateDirectory(path);
                CopyFile(logfilePath, path, appAndLog.log.LineCount);

            }

            //post Fetch Program
            appAndLog.application.ExecutePostFetching();

            // return status
            return fetch_status;
        }
    }
}
