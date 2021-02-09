using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using JiraWorklogSubmitter.Data;
using JiraWorklogSubmitter.Services.Interfaces;
using JiraWorklogSubmitter.Config;
using System;
using JiraWorklogSubmitter.Data.ResponseObjects;
using Newtonsoft.Json;

namespace JiraWorklogSubmitter.Services
{
    public class TimeEntryService : ITimeEntryService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TimeEntryService> _logger;
        private readonly IOptions<JiraSettings> _jiraSettings;

        private const string ApplicationJson = "application/json";

        public TimeEntryService(ILogger<TimeEntryService> logger, IHttpClientFactory httpClientFactory, IOptions<JiraSettings> jiraSettings)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _jiraSettings = jiraSettings;
        }

        /// <inheritdoc/>
        public async Task<ICollection<JiraWorklogEntry>> SubmitJiraWorklogEntriesAsync(ICollection<JiraWorklogEntry> jiraWorkLogEntries)
        {
            try
            {
                var jiraClient = _httpClientFactory.CreateClient(HttpClientFactoryNameEnum.Jira.ToString());
                var jiraWorkLogEntriesToSubmit = jiraWorkLogEntries.Where(j => !string.IsNullOrEmpty(j.Ticket) && !string.IsNullOrEmpty(j.TimeSpent)).ToList();

                foreach (var jiraWorklogEntry in jiraWorkLogEntriesToSubmit)
                {
                    await SubmitJiraWorklogEntryAsync(jiraClient, jiraWorklogEntry, jiraWorkLogEntries);
                }

                //TODO: Return something with more details about the success/failures of the entries

                // Populate the list with at least one blank JiraWorklogEntry
                if (!jiraWorkLogEntries.Any()) {
                    jiraWorkLogEntries.Add(new JiraWorklogEntry());
                }
                
                return jiraWorkLogEntries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(SubmitJiraWorklogEntriesAsync)} when trying to submit entries{Environment.NewLine}Error: {ex.Message}");
                throw;
            }
        }

        private async Task SubmitJiraWorklogEntryAsync(HttpClient jiraClient, JiraWorklogEntry jiraWorklogEntry, ICollection<JiraWorklogEntry> jiraWorkLogEntries)
        {
            try
            {
                var worklogUrl = $"{_jiraSettings.Value.ApiUrl}issue/{jiraWorklogEntry.Ticket}/worklog";

                var request = new HttpRequestMessage(HttpMethod.Post, worklogUrl);

                var jsonBody = JsonConvert.SerializeObject(jiraWorklogEntry);

                var jiraWorkLogEntryHttpRequestContent = new StringContent(
                        jsonBody,
                        Encoding.UTF8,
                        ApplicationJson
                    );

                request.Content = jiraWorkLogEntryHttpRequestContent;

                _logger.LogDebug($"Attempting to submit: {jsonBody} to the url: {worklogUrl}");

                using var httpResponse = await jiraClient.SendAsync(request);

                var responseBody = httpResponse.EnsureSuccessStatusCode();

                // Remove the successful entry from the list so we can return it
                jiraWorkLogEntries.Remove(jiraWorklogEntry);
            }
            catch (HttpRequestException hre)
            {
                _logger.LogWarning(hre, $"An http response was not successful when in {nameof(SubmitJiraWorklogEntryAsync)} when trying to submit a worklog for: {jiraWorklogEntry.Ticket}{Environment.NewLine}Error: {hre.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(SubmitJiraWorklogEntryAsync)} when trying to submit a worklog for: {jiraWorklogEntry.Ticket}{Environment.NewLine}Error: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetJiraTicketSummaryAsync(string issueKey)
        {
            try
            {
                var jiraClient = _httpClientFactory.CreateClient(HttpClientFactoryNameEnum.Jira.ToString());
                var summaryUrl = $"{_jiraSettings.Value.ApiUrl}issue/{issueKey}?fields=summary";

                var request = new HttpRequestMessage(HttpMethod.Get, summaryUrl);

                _logger.LogDebug($"Attempt to get ticket summary for {issueKey} from the url: {summaryUrl}");

                using var httpResponse = await jiraClient.SendAsync(request);

                var responseBody = await httpResponse.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

                var issueResponseObject = JsonConvert.DeserializeObject<IssueResponseObject>(responseBody);

                _logger.LogDebug($"responseBody: {responseBody}");

                return issueResponseObject.Fields.Summary;
            }
            catch (HttpRequestException hre)
            {
                _logger.LogWarning(hre, $"An http response was not successful when in {nameof(GetJiraTicketSummaryAsync)} when trying to get the summary for the issue key: {issueKey}{Environment.NewLine}Error: {hre.Message}");
                return "ERROR: UNABLE TO GET SUMMARY FOR THAT ISSUE!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(GetJiraTicketSummaryAsync)} when trying to get the summary for the issue key: {issueKey}{Environment.NewLine}Error: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<IssuesWithComments>> GetWorklogsForTargetDate(DateTime targetDate)
        {
            try
            {
                //TODO: Make a more generic endpoint so we can send different lengths of time (this week, last week, last two weeks) and maybe authors

                // This query gets a list of the tickets that an author submitted worklogs too, not the actual work logs
                // https://colyar.atlassian.net/rest/api/latest/search?jql=worklogDate >= startOfWeek() and worklogAuthor = "Ryan Taite"&fields=key

                var jiraClient = _httpClientFactory.CreateClient(HttpClientFactoryNameEnum.Jira.ToString());
                // Using Jira's JQL we can get the tickets that were worked on by the signed in user, but not the actual worklogs themselves.
                var url = $"{_jiraSettings.Value.ApiUrl}search?jql=worklogDate = \"{targetDate:yyyy-MM-dd}\" and worklogAuthor = \"{_jiraSettings.Value.FullName}\"&fields=summary"; // Filter down the just the summary (ticket title), so we don't get too much info back.

                var request = new HttpRequestMessage(HttpMethod.Get, url);

                _logger.LogDebug($"Attempt to get list of worked on tickets from the url: {url}");

                using var httpResponse = await jiraClient.SendAsync(request);

                var responseBody = await httpResponse.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

                var searchResponseObject = JsonConvert.DeserializeObject<SearchResponseObject>(responseBody);

                var allKeys = searchResponseObject.Issues
                    .Select(issue =>
                    {
                        return issue.Key;
                    })
                    .ToList();

                _logger.LogDebug($"responseBody: {responseBody}");

                var allMatchingWorklogs = await GetWorklogsForDayAndKeysAsync(allKeys, targetDate);
                var worklogAndComments = GetWorklogSummariesAndComments(allMatchingWorklogs);
                await LinkWorklogsToSummariesAsync(worklogAndComments);

                return worklogAndComments;
            }
            catch (System.Exception exception)
            {
                var innerExceptionMessage = exception.InnerException == null ? "" : exception.InnerException.Message;
                _logger.LogError(exception, $"Something went wrong in {nameof(GetWorklogsForTargetDate)}!{Environment.NewLine}Error: {exception.Message}{Environment.NewLine}Inner Exception: {innerExceptionMessage}");
                throw;
            }

        }

        /// <summary>
        /// Because worklogs don't get returned with their summaries, we have to get them here and link them up
        /// </summary>
        /// <param name="worklogAndComments"></param>
        private async Task LinkWorklogsToSummariesAsync(List<IssuesWithComments> worklogAndComments)
        {
            foreach (var worklog in worklogAndComments)
            {
                worklog.Summary = await GetJiraTicketSummaryAsync(worklog.Key);
            }
        }

        /// <summary>
        /// Group the matching worklogs by their key, to get a list of keys and their associated comments
        /// </summary>
        /// <param name="allMatchingWorklogs"></param>
        /// <returns></returns>
        private List<IssuesWithComments> GetWorklogSummariesAndComments(List<Worklog> allMatchingWorklogs)
        {
            var result = allMatchingWorklogs
                .GroupBy(worklog => worklog.Key, worklog => worklog.Comment, (key, group) =>
                    new IssuesWithComments { Key = key, Comments = group.ToList() })
                .ToList();

            return result;
        }

        /// <summary>
        /// Given a list of worklog keys, get each of their worklogs, then filter them down to the <paramref name="targetDate"/> and where the Author matches the current users full name
        /// </summary>
        /// <param name="allKeys">The issue keys we want the worklogs for</param>
        /// <param name="targetDate">The day a worklog is entered that we want to retrieve</param>
        /// <returns>A list of matching <see cref="Worklog"/>s</returns>
        private async Task<List<Worklog>> GetWorklogsForDayAndKeysAsync(List<string> allKeys, DateTime targetDate)
        {
            var jiraClient = _httpClientFactory.CreateClient(HttpClientFactoryNameEnum.Jira.ToString());

            var allMatchingWorklogs = new List<Worklog>();

            foreach (var key in allKeys)
            {
                var url = $"{_jiraSettings.Value.ApiUrl}issue/{key}/worklog"; // Filter down the just the summary (ticket title), so we don't get too much info back.
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var httpResponse = await jiraClient.SendAsync(request);

                var responseBody = await httpResponse.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

                var worklogResponseObject = JsonConvert.DeserializeObject<WorklogResponseObject>(responseBody);

                // Get worklogs created by our user and created on our target date
                foreach (var worklog in worklogResponseObject.Worklogs.Where(worklog => worklog.Author.DisplayName == _jiraSettings.Value.FullName && worklog.Started.Date == targetDate.Date))
                {
                    worklog.Key = key; // The key doesn't come back with the response, so we have to do it here
                    allMatchingWorklogs.Add(worklog);
                }
            }

            return allMatchingWorklogs;
        }
    }
}
