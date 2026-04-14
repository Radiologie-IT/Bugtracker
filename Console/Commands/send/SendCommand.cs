using System.Collections.Generic;
using Bugtracker.Attributes;
using Bugtracker.Capture;
using Bugtracker.Configuration;
using Bugtracker.Send;
using Bugtracker.Targeting;

namespace Bugtracker.Console.Commands.send
{
    [Command("send", "snd", "Utility to send captures to target")]
    [Arguments(null, new[] { "target1", "target2", "..targetx" })]
    class SendCommand : Command
    {
        public override string Execute()
        {
            TargetManager tm = RunningConfiguration.GetInstance().Targets;
            List<Target> targetsToSend = new List<Target>();

            foreach (string target in GivenArguments)
            {
                if (tm.GetTargetByName(target) != null && tm.GetTargetByName(target).Default)
                    targetsToSend.Add(tm.GetTargetByName(target));
                else
                    return "One or more target names are not existing or are writen incorrectly";
            }

            SendHandler sh = new SendHandler(targetsToSend);

            sh.Send();
            return "Sent all current captures to targets.";
        }
    }

    [Command("default", "dft", "Sends to all default targets.", typeof(SendCommand))]
    class SendToDefaultCommand : Command
    {
        public override string Execute()
        {
            TargetManager tm = RunningConfiguration.GetInstance().Targets;
            List<Target> targetsToSend = new List<Target>();

            SendHandler sh = new SendHandler(tm.GetDefaultTargets());

            (int, int) completionStatus = SendHandler.ReturnCompletionStatus(sh.Send());

            return "Sent current captures to " + completionStatus.Item2 + " of " + completionStatus.Item1 + " default targets.";
        }
    }

}
