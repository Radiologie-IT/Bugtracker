using System;
using System.IO;
using System.Reflection;
using Bugtracker.Configuration;
using Bugtracker.Globals_and_Information;

namespace Bugtracker.Logging
{
    /// <summary>
    /// All availabe logging severities
    /// </summary>
    public enum LoggingSeverity
    {
        //TODO: Remove Null
        Null = 99,
        Debug = 4,
        Info = 3,
        Warning = 2,
        Error = 1
    }

    /// <summary>
    /// Event Args for LoggedNewLine Event
    /// </summary>
    public class LoggedNewLineEventArgs : EventArgs
    {
        public LoggingSeverity LoggingSeverity { get; set; }
        public string DateAndTime { get; set; }
        public string Message { get; set; }

        public LoggedNewLineEventArgs(LoggingSeverity loggingSeverity, string dateAndTime, string message)
        {
            this.LoggingSeverity = loggingSeverity;
            this.DateAndTime = dateAndTime;
            this.Message = message;
        }
    }

    /// <summary>
    /// Logger class. Used to log messages to a file.
    /// </summary>
    public static class Logger
    {
        public static event EventHandler LoggedNewLine;

        private static readonly string ProgramVersion =
            Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

        /// <summary>
        /// Minimum logging severity level. Only messages at or below this level will be logged.
        /// Default is Info (3). Lower numbers = more severe/important.
        /// </summary>
        public static LoggingSeverity MinimumSeverity { get; set; } = LoggingSeverity.Info;

        /// <summary>
        /// Check if log file exists
        /// or has to be created
        /// </summary>
        public static void InitializeLogging()
        {
            if(!Directory.Exists(Globals.APPLICATION_DIRECTORY))
            {
                Directory.CreateDirectory(Globals.APPLICATION_DIRECTORY);
            }

            if (!Directory.Exists(Globals.LOG_DIRECTORY))
            {
                Directory.CreateDirectory(Globals.LOG_DIRECTORY);
            }

            if (File.Exists(Globals.LOG_FILE_PATH))
            {
                RotateLogs();
                //File.Delete(Globals.LOG_FILE_PATH);
            }
            
            File.Create(Globals.LOG_FILE_PATH).Dispose();

            // check if config file exists
            //Logger.CheckConfigFile();
        }

        public static void RotateLogs()
        {
            //deleted oldest log
            if(File.Exists(Globals.LOG_FILE_PATH + "." + (Globals.LOG_ROTATION_COUNT - 1)))
            {
                File.Delete(Globals.LOG_FILE_PATH + "." + (Globals.LOG_ROTATION_COUNT - 1));
            }

            //increase number of numbered log files by 1
            for (int i = (Globals.LOG_ROTATION_COUNT - 2); i>=0; i--)
            {
                string filename_old = Globals.LOG_FILE_PATH + "." + i;
                string filename_new = Globals.LOG_FILE_PATH + "." + (i + 1);

                if (File.Exists(filename_old))
                {
                    File.Move(filename_old, filename_new);
                }
            }

            //add number 0 to previous log file name
            if (File.Exists(Globals.LOG_FILE_PATH))
            {
                File.Move(Globals.LOG_FILE_PATH, Globals.LOG_FILE_PATH + ".0");
            }

        }


        /// <summary>
        /// return content of bugtrackerv2.config.xml
        /// 
        /// TODO: Change static path
        /// </summary>
        /// <returns></returns>
        public static string GetConfigFileContent()
        {
            // get content of config file
            string fileContent = File.ReadAllText(@"C:\Users\Daniel Bretschneider\source\repos\Bugtracker Version 2\bugtrackerv2.config.xml");

            // return content as string
            return fileContent;
        }


        /// <summary>
        /// This method checks if a config file already exists in local
        /// if so, nothing will be done. If not, then the default configuration
        /// will be copied to specified location.
        /// </summary>
        public static void CheckConfigFile()
        {
            // check if local config exists
            if (!File.Exists(Globals.LOCAL_STARTUP_CONFIG_FILE_PATH))
            {
                // Create a file to write to.
                using StreamWriter sw = File.CreateText(Globals.LOCAL_STARTUP_CONFIG_FILE_PATH);
                // write xml content to file
                sw.WriteLine(GetConfigFileContent());
            }
        }



        /// <summary>
        /// log message with priority
        /// 0 = ERRROR
        /// 1 = WARNING
        /// 2 = INFO
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="priority"></param>
        public static void Log(string msg, LoggingSeverity loggingSeverity)
        {
            // Filter out messages that are less important than the minimum severity
            // Lower enum values = more severe (Error=1, Warning=2, Info=3, Debug=4)
            if ((int)loggingSeverity > (int)MinimumSeverity)
            {
                return;
            }

            // get local date and time
            DateTime localDate = DateTime.Now;

            // de-DE datetime
            string dateAndTime = localDate.ToString("dd.MM.yyyy HH:mm:ss");

            // full message
            string fmsg = "";

            // checking if logging is enabled
            //TODO: Fix -> Singleton -> Stackoverflow RunningConfiguration.GetInstance().LoggerEnabled

            // different message, depends on priority
            switch (loggingSeverity)
            {
                case LoggingSeverity.Error:
                    fmsg = "[ERROR][" + ProgramVersion + "][" + dateAndTime + "]: " + msg;
                    AppendToFile(fmsg);
                    System.Diagnostics.Debug.WriteLine(fmsg);
                    break;

                case LoggingSeverity.Warning:
                    fmsg = "[WARNING][" + ProgramVersion + "][" + dateAndTime + "]: " + msg;
                    AppendToFile(fmsg);
                    System.Diagnostics.Debug.WriteLine(fmsg);
                    break;

                case LoggingSeverity.Info:
                    fmsg = "[INFO][" + ProgramVersion + "][" + dateAndTime + "]: " + msg;
                    AppendToFile(fmsg);
                    System.Diagnostics.Debug.WriteLine(fmsg);
                    break;

                case LoggingSeverity.Debug:
                    fmsg = "[DEBUG][" + ProgramVersion + "][" + dateAndTime + "]: " + msg;
                    AppendToFile(fmsg);
                    System.Diagnostics.Debug.WriteLine(fmsg);
                    break;
            }

            LoggedNewLine?.Invoke(null, new LoggedNewLineEventArgs(loggingSeverity, dateAndTime, fmsg));
        }


        /// <summary>
        /// Append message to log file
        /// </summary>
        /// <param name="msg"></param>
        public static void AppendToFile(String msg)
        {
            // if logging is enabled in config file, log
            if (true)
            {
                try
                {
                    using StreamWriter writer = File.AppendText(Globals.LOG_FILE_PATH);
                    writer.WriteLine(msg);
                }
                catch
                {
                    // Cannot recursively call Logger.Log here, so use Debug.WriteLine as fallback
                    System.Diagnostics.Debug.WriteLine("Cannot write to log file - file is being used by another process");
                }
            }
        }
    }
}
