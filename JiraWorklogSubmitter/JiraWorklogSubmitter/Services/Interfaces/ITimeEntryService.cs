using JiraWorklogSubmitter.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JiraWorklogSubmitter.Services.Interfaces
{
    public interface ITimeEntryService
    {
        /// <summary>
        /// Submits a worklog entry for each ticket in <paramref name="jiraWorkLogEntries"/>
        /// </summary>
        /// <param name="jiraWorkLogEntries">The collection of worklogs to submit</param>
        /// <returns>Any <see cref="JiraWorklogEntry"> values that failed to submit</returns>
        Task<ICollection<JiraWorklogEntry>> SubmitJiraWorklogEntriesAsync(ICollection<JiraWorklogEntry> jiraWorkLogEntries);

        /// <summary>
        /// Gets the summary (aka Title) of a ticket
        /// </summary>
        /// <param name="issueKey">The ticket number you want the summary of, ex: CTS-302</param>
        /// <returns>The ticket's summary</returns>
        Task<string> GetJiraTicketSummaryAsync(string issueKey);

        /// <summary>
        /// Get the worklog comments for tickets worked on for the <paramref name="TargetDate"/>
        /// </summary>
        /// <returns>A list of <see cref="IssuesWithComments"/> that has the worklog keys, summaries, and comments</returns>
        Task<List<IssuesWithComments>> GetWorklogsForTargetDate(System.DateTime targetDate);
    }
}
