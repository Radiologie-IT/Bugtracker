using Bugtracker.Attributes;
using Bugtracker.InternalApplication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Bugtracker.Configuration;
using Bugtracker.Utils;
using Bugtracker.Capture.Log;

namespace Bugtracker.Console.Commands.capture
{
    [Command("capture", "capt", "Captures data from Host PC")]
    [Arguments(null, new[] { "subcommand" })]

    class CaptureCommand : Command
    {
        public override string RootExecution()
        {
            BugtrackerUtils.CreateBugtrackFolder();
            return "Bugtracker folder created!";
        }
        public override string Execute()
        {
            return new CaptureFullCommand().Execute();
        }
    }

    [Command("-log", "-l", "Captures Log Files only", typeof(CaptureCommand))]
    [Arguments(new[] { "application / all" }, new[] { "application2", "application3..." })]
    class CaptureLogFilesCommand : Command
    {
        public override string Execute()
        {
            List<InternalApplication.Application> targetedApplications = new List<InternalApplication.Application>();
            ApplicationManager am = RunningConfiguration.GetInstance().Applications;

            foreach (string argument in GivenArguments)
            {
                if (am.GetApplicationByName(argument) != null)
                    targetedApplications.Add(am.GetApplicationByName(argument));
                else
                    return "One or more given applications do not exist or names do not match!";
            }

            LogProcessor.FetchAllLogFiles(targetedApplications);

            return "Fetched all log files from given applications(s).";
        }
    }

    [Command("all", "-a", "Captures log files from all installed applications", typeof(CaptureLogFilesCommand))]
    class CaptureAllLogFilesCommand : Command
    {
        public override string Execute()
        {
            List<InternalApplication.Application> targetedApplications = new List<InternalApplication.Application>();
            ApplicationManager am = RunningConfiguration.GetInstance().Applications;

            foreach (Application a in am.GetApplications())
            {
                if (a.IsInstalled)
                    targetedApplications.Add(a);
            }

            Logging.Logger.Log("Fetching log files from " + targetedApplications.Count + " installed applications", Logging.LoggingSeverity.Debug);

            LogProcessor.FetchAllLogFiles(targetedApplications);

            return "Fetched all log files from all installed applications(s).";
        }
    }

    [Command("-screen", "-s", "Captures screenshots only", typeof(CaptureCommand))]
    class CaptureScreenShotsCommand : Command
    {
        public override string Execute()
        {
            string screenFilePath;

            screenFilePath = BugtrackerUtils.GenerateScreenCapture();

            BugtrackConsole.Pause();

            return "Screencapture saved under: " + screenFilePath;
        }
    }

    [Command("-full", "-f", "Captures all log files and makes screenshots", typeof(CaptureCommand))]
    [Arguments(new [] { "application" }, new[] { "application2" , "application3", "application4..." })]
    class CaptureFullCommand : Command
    {
        public override string Execute()
        {
            string result = "";

            CaptureScreenShotsCommand captureScreenShotsCommand = new CaptureScreenShotsCommand();
            result += captureScreenShotsCommand.Execute() + Environment.NewLine;

            if (GivenArguments.Count == 0)
            {
                CaptureAllLogFilesCommand captureAllLogFilesCommand = new CaptureAllLogFilesCommand();
                result += captureAllLogFilesCommand.Execute() + Environment.NewLine;
            }
            else
            {
                CaptureLogFilesCommand captureLogFilesCommand = new CaptureLogFilesCommand();
                captureLogFilesCommand.GivenArguments = this.GivenArguments;

                result += captureLogFilesCommand.Execute() + Environment.NewLine;
            }

            return result;
        }
    }


    [Command("-path", "-p", "Shows current screenshot folder path", typeof(CaptureCommand))]
    class ShowCapturePathCommand : Command
    {
        public override string Execute()
        {
            return "Screencapture saved under: " + RunningConfiguration.GetInstance().NewestBugtrackerFolder;
        }
    }

    [Command("open", "o", "Opens current screenshot folder path", typeof(ShowCapturePathCommand))]
    class OpenCapturePathCommand : Command
    {
        public override string Execute()
        {
            if (RunningConfiguration.GetInstance().NewestBugtrackerFolder != null)
            {
                Process.Start(RunningConfiguration.GetInstance().NewestBugtrackerFolder.FullName);
                return "Opening Path";
            }

            return "No path set yet!";
        }
    }
}
