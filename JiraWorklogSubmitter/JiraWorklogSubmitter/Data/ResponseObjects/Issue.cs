using JiraWorklogSubmitter.Data.Bases;

namespace JiraWorklogSubmitter.Data.ResponseObjects
{
    public class Issue : CommonJsonFields
    {
        public Fields Fields { get; set; }
    }
}
