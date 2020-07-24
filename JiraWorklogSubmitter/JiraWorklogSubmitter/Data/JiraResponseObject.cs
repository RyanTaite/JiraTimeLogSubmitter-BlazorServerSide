using System.Collections.Generic;

namespace JiraWorklogSubmitter.Data
{
    /// <summary>
    /// This class is intended to map Jira response values
    /// </summary>
    public class JiraResponseObject
    {
        /// <summary>
        /// Used to indicate we didn't set this value with a JIRA response
        /// </summary>
        private const int DefaultIntValue = -1;

        public string Expand { get; set; } = string.Empty;

        public string Id { get; set; } = string.Empty;

        public string Self { get; set; } = string.Empty;

        public int StartAt { get; set; } = DefaultIntValue;

        public int MaxResults { get; set; } = DefaultIntValue;

        public int Total { get; set; } = DefaultIntValue;

        public Dictionary<string, string> Fields { get; set; } = new Dictionary<string, string>();

        public List<Dictionary<string, string>> Issues { get; set; } = new List<Dictionary<string, string>>();
    }

    //public class Issues
    //{
    //    public string Expand { get; set; }

    //    public string Id { get; set; }

    //    public string Self { get; set; }

    //    public string Key { get; set; }
    //}
}
