using System;
using System.Collections.Generic;
using Bugtracker.Configuration;

namespace Bugtracker.Targeting
{
    /// <summary>
    /// The TargetManager class is responsible for managing the targets that are available to the user.
    /// </summary>
    public class TargetManager
    {
        public List<Target> Targets { get; set; }

        public TargetManager()
        {
            Targets = new List<Target>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Target> GetDefaultTargets()
        {
            List<Target> defaultTargets = new List<Target>();

            foreach (Target target in Targets)
            {
                if (target.Default)
                    defaultTargets.Add(target);
            }

            return defaultTargets;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Target> GetObligatoryTargets()
        {
            List<Target> obligatoryTargets = new List<Target>();

            foreach (Target target in Targets)
            {
                if (target.Obligatory)
                    obligatoryTargets.Add(target);
            }

            return obligatoryTargets;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Target GetTargetByName(string name)
        {
            foreach (Target t in Targets)
            {
                if (t.Name == name)
                    return t;
            }
            return null;
        }

        /// <summary>
        /// Summary used for console output.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string returnString = "";

            foreach (Target t in Targets)
            {
                returnString += t.ToString() + Environment.NewLine;
            }

            return returnString;
        }
    }
}
