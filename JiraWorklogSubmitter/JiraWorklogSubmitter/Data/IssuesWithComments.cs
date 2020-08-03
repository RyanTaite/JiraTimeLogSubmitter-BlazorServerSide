using System.Collections.Generic;
using System;

namespace JiraWorklogSubmitter.Data
{
    public class IssuesWithComments
    {
        public string Key { get; set; }

        public string Summary { get; set; }

        public List<string> Comments { get; set; } = new List<string>();

        public IssuesWithComments()
        {
        }

        public override bool Equals(object obj)
        {
            return obj is IssuesWithComments other &&
                   Key == other.Key &&
                   Summary == other.Summary &&
                   EqualityComparer<List<string>>.Default.Equals(Comments, other.Comments);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Comments);
        }
    }
}
