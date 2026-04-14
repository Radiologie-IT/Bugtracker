using Bugtracker.Configuration;
using Bugtracker.Globals_and_Information;
using Bugtracker.Logging;
using Bugtracker.Problem_Descriptors;
using System;
using System.IO;

namespace Bugtracker.Targeting
{
    /// <summary>
    /// Abstract base class for all target types in the bugtracker system.
    /// Targets define where captured diagnostic data should be sent.
    /// Each concrete target type (folder, mail, powershell, etc.) inherits from this class.
    /// </summary>
    public abstract class Target
    {
        /// <summary>
        /// The name of the target (user-friendly identifier from XML)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether this target is selected by default
        /// </summary>
        public bool Default { get; set; }

        /// <summary>
        /// Whether this target should always be used (cannot be deselected)
        /// </summary>
        public bool Obligatory { get; set; }

        /// <summary>
        /// The type identifier for this target (e.g., "folder", "mail", "powershell")
        /// Used in XML configuration and factory creation.
        /// Must match the value in the TargetType attribute on the concrete class.
        /// </summary>
        public abstract string TypeIdentifier { get; }

        /// <summary>
        /// Send the captured bugtracker data to this target
        /// </summary>
        /// <param name="problemDescriptor">Optional problem descriptor with category and description</param>
        /// <returns>Result indicating success or failure with details</returns>
        public abstract SendResult Send(ProblemDescriptor problemDescriptor = null);

        /// <summary>
        /// Validate that this target's configuration is valid and ready to use.
        /// Called after properties are loaded from XML to ensure all required settings are present.
        /// </summary>
        /// <param name="errorMessage">Output parameter containing error details if validation fails</param>
        /// <returns>True if configuration is valid, false otherwise</returns>
        public abstract bool ValidateConfiguration(out string errorMessage);

        /// <summary>
        /// Get a human-readable summary of this target's configuration.
        /// Override this method to include target-specific details.
        /// </summary>
        /// <returns>Multiline string summarizing the target</returns>
        public virtual string GetSummary()
        {
            return $"Name: {Name}\nType: {TypeIdentifier}\nDefault: {Default}\nObligatory: {Obligatory}";
        }

        /// <summary>
        /// Override ToString to use GetSummary
        /// </summary>
        public override string ToString()
        {
            return GetSummary();
        }

        /// <summary>
        /// Helper method: Creates a problem description text file in the bugtracker folder.
        /// Used by targets that need to include problem details with the captured data.
        /// </summary>
        /// <param name="path">Path where the file should be created (without .txt extension)</param>
        /// <param name="problemDescriptor">Problem descriptor containing category and description</param>
        protected void CreateProblemDescriptionFile(string path, ProblemDescriptor problemDescriptor)
        {
            using (StreamWriter sw = File.CreateText(path + ".txt"))
            {
                sw.Write(PCInfo.Summary());

                sw.WriteLine("Problem Kategorie");
                if (problemDescriptor?.ProblemCategory != null)
                    sw.WriteLine(problemDescriptor.ProblemCategory.Name);
                else
                    sw.WriteLine("n/a");
                sw.WriteLine("---------------------------------------------" + Environment.NewLine);
                sw.WriteLine("Problem Beschreibung:");
                if (problemDescriptor?.ProblemDescription != null && problemDescriptor.ProblemDescription != "")
                    sw.WriteLine(problemDescriptor.ProblemDescription);
                else
                    sw.WriteLine("n/a");

                Logging.Logger.Log("Created Problem Description file", LoggingSeverity.Info);
            }
        }

        /// <summary>
        /// Helper method: Resolves a custom folder name template with variable substitution.
        /// Used by targets that support custom bugtracker folder naming.
        /// </summary>
        /// <param name="template">Template string containing variables like %date%, %hostname%, etc.</param>
        /// <param name="problemDescriptor">Problem descriptor (used to set %ticket% variable if available)</param>
        /// <returns>Resolved folder name with all variables substituted</returns>
        protected string ResolveCustomFolderName(string template, ProblemDescriptor problemDescriptor = null)
        {
            if (string.IsNullOrEmpty(template))
                return null;

            // Set ticket variable if problem category is available
            if (problemDescriptor?.ProblemCategory != null)
            {
                RunningConfiguration.GetInstance().Variables.VariableDictionary["ticket"] =
                    (problemDescriptor.ProblemCategory.TicketAbbreviation, false);
            }

            // Replace all variable keywords in the template
            return RunningConfiguration.GetInstance().Variables.ReplaceKeywords(template);
        }
    }
}
