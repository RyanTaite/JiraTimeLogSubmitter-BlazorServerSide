using JiraWorklogSubmitter.Data.Bases;
using System.Collections.Generic;

namespace JiraWorklogSubmitter.Data.ResponseObjects
{
    /// <summary>
    /// This class is intended to map the Jira response from their /api/latest/issue/{key}/worklog endpoint
    /// </summary>
    public class WorklogResponseObject : CommonJsonFields
    {
        public List<Worklog> Worklogs { get; set; }
    }
}
