using System;
using Bugtracker.Configuration;
using Bugtracker.Problem_Descriptors;
using Bugtracker.Targeting;
using Bugtracker.Utils;
using System.Collections.Generic;
using System.IO;
using Bugtracker.Ticketing;
using Bugtracker.Globals_and_Information;
using System.Net.Mail;
using System.Net;
using Bugtracker.Variables;
using System.Drawing;
using System.Net.Mime;
using System.Windows.Forms;
using System.Management.Automation;
using Bugtracker.Logging;
using System.Text;
using System.Collections.ObjectModel;
using System.Drawing.Text;
using Microsoft.VisualBasic;
using System.Linq;

namespace Bugtracker.Send
{
    /// <summary>
    /// Providing methods to send Bugtracker folders of local session to given targets.
    /// </summary>
    public class SendHandler
    {
        enum TicketType
        {
            GLPI
        }

        List<Target> targets;
        public SendHandler(List<Target> targets)
        {
            this.targets = targets;
        }

        /// <summary>
        /// Returns completion status tuple from SendResult list
        /// </summary>
        /// <param name="results">List of SendResult objects</param>
        /// <returns>Tuple of (total count, success count)</returns>
        public static (int, int) ReturnCompletionStatus(List<SendResult> results)
        {
            int size = results.Count;
            int complete = 0;

            foreach (SendResult result in results)
            {
                if (result.Success)
                    complete++;
            }

            return (size, complete);
        }

        /// <summary>
        /// Returns completion status tuple ex. 1 (legacy overload for bool list)
        /// </summary>
        /// <returns></returns>
        public static (int, int) ReturnCompletionStatus(List<bool> completionStatus)
        {
            int size = completionStatus.Count;
            int complete = 0;

            foreach (bool b in completionStatus)
            {
                if (b == true)
                    complete++;
            }

            return (size, complete);
        }
        /// <summary>
        /// Never used. Returns completion status in percent ex. 0.5
        /// </summary>
        /// <param name="completionsStatus"></param>
        /// <returns></returns>
        public static float ReturnCompletionStatusPercent(List<bool> completionsStatus)
        {
            (int, int) completionsStat = ReturnCompletionStatus(completionsStatus);

            return completionsStat.Item2 / completionsStat.Item1;
        }

        /// <summary>
        /// Sends bugtracker data to all configured targets.
        /// Each target handles its own send logic (copy, mail, powershell, etc.).
        /// </summary>
        /// <param name="problemDescriptor">Optional problem descriptor with category and description</param>
        /// <returns>List of SendResult objects containing success status, messages, and URLs</returns>
        public List<SendResult> Send(ProblemDescriptor problemDescriptor = null)
        {
            List<SendResult> results = new List<SendResult>();

            foreach (Target target in targets)
            {
                try
                {
                    Logger.Log($"Sending to target: {target.Name} ({target.TypeIdentifier})", LoggingSeverity.Info);
                    SendResult result = target.Send(problemDescriptor);

                    if (result.Success)
                    {
                        Logger.Log($"Target '{target.Name}' completed successfully: {result.Message}", LoggingSeverity.Info);
                        if (!string.IsNullOrEmpty(result.Url))
                        {
                            Logger.Log($"Target '{target.Name}' URL: {result.Url}", LoggingSeverity.Info);
                        }
                    }
                    else
                    {
                        Logger.Log($"Target '{target.Name}' failed: {result.Message}", LoggingSeverity.Error);
                    }

                    results.Add(result);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Exception sending to target '{target.Name}': {ex.Message}", LoggingSeverity.Error);
                    results.Add(SendResult.Fail($"Exception: {ex.Message}", ex));
                }
            }

            return results;
        }

        /// <summary>
        /// Create a ticket in an external ticketing system
        /// </summary>
        /// <param name="ticketObject">Ticket creation object containing ticket details</param>
        /// <returns>True if ticket was created successfully, false otherwise</returns>
        public bool CreateTicket(TicketCreationObject ticketObject)
        {
            return ticketObject.Create();
        }
    }
}
