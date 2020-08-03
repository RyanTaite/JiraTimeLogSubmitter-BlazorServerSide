using JiraWorklogSubmitter.Data.Bases;

namespace JiraWorklogSubmitter.Data.ResponseObjects
{
    /// <summary>
    /// This class is intended to map the Jira response from their /api/latest/issue/{key} endpoint
    /// </summary>
    public class IssueResponseObject : CommonJsonFields
    {
        public Fields Fields { get; set; }
    }
}
