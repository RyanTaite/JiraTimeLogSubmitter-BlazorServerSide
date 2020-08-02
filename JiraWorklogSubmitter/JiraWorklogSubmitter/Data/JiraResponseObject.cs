using JiraWorklogSubmitter.Data.Bases;
using System.Collections.Generic;

namespace JiraWorklogSubmitter.Data
{
    /// <summary>
    /// This class is intended to map Jira response values
    /// </summary>
    public class JiraResponseObject : CommonJsonFields
    {
        public int StartAt { get; set; } = DefaultIntValue;

        public int MaxResults { get; set; } = DefaultIntValue;

        public int Total { get; set; } = DefaultIntValue;

        public List<Issue> Issues { get; set; }

        public Fields Fields { get; set; }
    }
}
