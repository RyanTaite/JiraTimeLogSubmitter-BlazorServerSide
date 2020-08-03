using JiraWorklogSubmitter.Data.Bases;
using System.Collections.Generic;

namespace JiraWorklogSubmitter.Data.ResponseObjects
{
    /// <summary>
    /// This class is intended to map the Jira response from their /api/latest/search endpoint
    /// </summary>
    public class SearchResponseObject : CommonJsonFields
    {
        public List<Issue> Issues { get; set; } = new List<Issue>();
    }
}
