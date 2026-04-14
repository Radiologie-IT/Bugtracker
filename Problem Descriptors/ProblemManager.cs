using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bugtracker.Configuration;

namespace Bugtracker.Problem_Descriptors
{
    /// <summary>
    /// Manages all problem categories and their descriptions. Would be useful in real life....
    /// </summary>
    public class ProblemManager
    {
        public List<ProblemCategory> ProblemCategories { get; set; }

        public ProblemManager()
        {
            ProblemCategories = new List<ProblemCategory>();
        }

        /// <summary>
        /// Returns First Problem Description for Given Problem by name
        /// </summary>
        /// <param name="problemName"></param>
        /// <returns></returns>
        public string GetDescriptionForProblemByName(string problemName)
        {
            foreach (var problemCategory in ProblemCategories)
            {
                if (problemName == problemCategory.Name)
                    return problemCategory.Descriptions[0];
            }

            return null;
        }

        /// <summary>
        /// Get Problem Category by Name
        /// </summary>
        /// <param name="problemName"></param>
        /// <returns></returns>
        public ProblemCategory GetProblemCategoryByName(string problemName)
        {
            foreach (var problemCategory in ProblemCategories)
            {
                if (problemCategory.Name == problemName)
                    return problemCategory;
            }

            return null;
        }
    }
}
