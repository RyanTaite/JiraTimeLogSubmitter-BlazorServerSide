using JiraWorklogSubmitter.Data.Bases;
using System;

namespace JiraWorklogSubmitter.Data.ResponseObjects
{
    public class Worklog : CommonJsonFields
    {
        /// <summary>
        /// Information of the person who created this worklog
        /// </summary>
        public Author Author { get; set; }

        public Author UpdateAuthor { get; set; }

        public string Comment { get; set; }

        public string TimeSpent { get; set; }

        public int TimeSpentSeconds { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public DateTime Started { get; set; }
    }
}