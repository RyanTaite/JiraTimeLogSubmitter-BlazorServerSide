namespace JiraWorklogSubmitter.Data.Bases
{
    public abstract class CommonJsonFields
    {
        /// <summary>
        /// Used to indicate we didn't set this value with a JIRA response
        /// </summary>
        protected const int DefaultIntValue = -1;

        public string Id { get; set; } = string.Empty;

        public string Expand { get; set; } = string.Empty;

        public string Self { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;
    }
}
