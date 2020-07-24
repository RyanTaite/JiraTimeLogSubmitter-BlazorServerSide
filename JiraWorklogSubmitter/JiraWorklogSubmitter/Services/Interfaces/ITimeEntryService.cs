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
        /// <returns></returns>
        Task<string> SubmitTimeLogAsync(ICollection<JiraWorklogEntry> jiraWorkLogEntries);

        /// <summary>
        /// Gets the summary (aka Title) of a ticket
        /// </summary>
        /// <param name="issueKey">The ticket number you want the summary of, ex: CTS-302</param>
        /// <returns>The ticket's summary</returns>
        Task<string> GetJiraTicketSummaryAsync(string issueKey);

        /// <summary>
        /// Get the keys of tickets worked on for this week
        /// </summary>
        /// <returns>A list of ticket keys worked on this week</returns>
        Task<List<string>> GetCurrentWeekTicketKeys();
    }
}
