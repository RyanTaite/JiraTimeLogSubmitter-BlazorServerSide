using JiraWorklogSubmitter.Data.Bases;

namespace JiraWorklogSubmitter.Data.ResponseObjects
{
    public class Author : CommonJsonFields
    {
        /// <summary>
        /// First and Last Name of the person who created this ticket
        /// </summary>
        public string DisplayName { get; set; }
    }
}