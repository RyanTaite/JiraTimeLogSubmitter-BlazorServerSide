namespace JiraWorklogSubmitter.Data.Bases
{
    public abstract class CommonJsonFields
    {
        public string Id { get; set; }

        public string Expand { get; set; }

        public string Self { get; set; }

        public string Key { get; set; }

        public int StartAt { get; set; }

        public int MaxResults { get; set; }

        public int Total { get; set; }
    }
}
