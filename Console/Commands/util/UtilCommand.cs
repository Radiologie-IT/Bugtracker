using System.Collections.Generic;
using System.IO;
using Bugtracker.Attributes;
using Bugtracker.Capture.Log;
using Bugtracker.Configuration;
using Bugtracker.Globals_and_Information;
using Bugtracker.InternalApplication;
using Bugtracker.Utils;

namespace Bugtracker.Console.Commands.util
{
    [Command("util", "utl", "A collection of utitly commands.")]
    [Arguments(new[] { "subcommand" })]
    class UtilCommand : Command
    {
        public override string Execute()
        {
            return base.Execute();
        }
    }

    [Command("delete", "del", "Delete specified target application logs.", typeof(UtilCommand))]
    [Arguments(new[] { "application" }, new[] { "application2", "application3..." })]
    class DeleteCommand : Command
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

            LogProcessor.DeleteAllTargeted(targetedApplications);

            return "Deleteted all log files from given applications(s).";
        }
    }

    [Command("rename", "rnm", "Tename specified target applications logs.", typeof(UtilCommand))]
    [Arguments(new[] { "application" }, new[] { "application2", "application3..." })]
    class RenameCommand : Command
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

            LogProcessor.RenameAllTargeted(targetedApplications);

            return "Renamed all log files from given applications(s).";
        }
    }

    [Command("init", "init", "Initializes BugtrackerFolder", typeof(UtilCommand))]
    class UtilInitCommand : Command
    {
        public override string Execute()
        {
            string folderName = BugtrackerUtils.CreateBugtrackFolderName();
            DirectoryInfo directory = BugtrackerUtils.CreateBugtrackFolder();

            RunningConfiguration.GetInstance().NewestBugtrackerFolder = directory;
            return "Created Bugtrackerfolder under: " + directory.FullName;
        }
    }
}
