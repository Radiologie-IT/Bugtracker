using Bugtracker.Targeting;
using System.Collections.Generic;

namespace Bugtracker.Problem_Descriptors
{
    /// <summary>
    /// Problem Category Object. Contains all information about a problem category.
    /// </summary>
    public class ProblemCategory
    {
        public string TicketAbbreviation { get; internal set; }

        public string Name { get; internal set; }
        public List<string> Descriptions { get; internal set; }
        public List<InternalApplication.Application> SelectedApplications { get; internal set; }
        public bool SelectAllApplications { get; internal set; }
        public bool SelectScreenshot { get; internal set; }
        public List<Target> Targets { get; internal set; }

        public ProblemCategory()
        {
            SelectAllApplications = false;
            SelectScreenshot = false;

            Descriptions = new List<string>();
            SelectedApplications = new List<InternalApplication.Application>();
            Targets = new List<Target>();
        }
    }
}
