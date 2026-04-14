using Bugtracker.Attributes;
using Bugtracker.Capture.Log;
using Bugtracker.Configuration;
using Bugtracker.InternalApplication;
using Bugtracker.Send;
using Bugtracker.Utils;
using System.Collections.Generic;
using System.Text;

namespace Bugtracker.Console.Commands.run
{
    [Command("run", "run", "Captures logs and a screenshot then sends to targets in one step. Use 'problem select' beforehand to target a specific category.")]
    [Arguments(null, new[] { "application", "application2..." })]
    class RunCommand : Command
    {
        public override string Execute()
        {
            var rc = RunningConfiguration.GetInstance();
            var sb = new StringBuilder();

            // Create a fresh bugtrack folder for this run
            var folder = BugtrackerUtils.CreateBugtrackFolder();
            rc.NewestBugtrackerFolder = folder;
            sb.AppendLine("Folder:    " + folder.FullName);

            // Step 1: Screenshot
            string screenPath = BugtrackerUtils.GenerateScreenCapture();
            sb.AppendLine("Screenshot: " + screenPath);

            // Step 2: Collect logs — use specified apps or all installed apps
            var am = rc.Applications;
            var apps = new List<Application>();

            if (GivenArguments.Count == 0)
            {
                foreach (Application a in am.GetApplications())
                {
                    if (a.IsInstalled)
                        apps.Add(a);
                }
            }
            else
            {
                foreach (string arg in GivenArguments)
                {
                    Application app = am.GetApplicationByName(arg);
                    if (app == null)
                        return $"Application \"{arg}\" not found.";
                    apps.Add(app);
                }
            }

            LogProcessor.FetchAllLogFiles(apps);
            sb.AppendLine($"Logs:      {apps.Count} application(s) captured.");

            // Step 3: Send using applicable targets (respects selected problem category)
            var targets = rc.GetApplicableTargets();

            if (targets.Count == 0)
            {
                sb.AppendLine("No targets configured — capture saved locally only.");
                return sb.ToString();
            }

            var sh = new SendHandler(targets);
            (int total, int succeeded) = SendHandler.ReturnCompletionStatus(sh.Send());
            sb.AppendLine($"Sent:      {succeeded} of {total} target(s).");

            if (rc.SelectedProblemCategory != null)
                sb.AppendLine($"Category:  {rc.SelectedProblemCategory.Name}");

            return sb.ToString();
        }
    }
}
