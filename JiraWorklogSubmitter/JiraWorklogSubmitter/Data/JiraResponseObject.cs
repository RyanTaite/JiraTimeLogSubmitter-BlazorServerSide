using System.Collections.Generic;

namespace JiraWorklogSubmitter.Data
{
    /// <summary>
    /// This class is intended to map Jira response values
    /// </summary>
    public class JiraResponseObject
    {
        public string Expand { get; set; }
        public string Id { get; set; }
        public string Self { get; set; }
        public Dictionary<string, string> Fields { get; set; }
    }

    public class Fields
    {
        public string Summary { get; set; }
    }
}
