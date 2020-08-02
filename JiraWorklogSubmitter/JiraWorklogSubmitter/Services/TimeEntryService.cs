using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using JiraWorklogSubmitter.Data;
using JiraWorklogSubmitter.Services.Interfaces;
using JiraWorklogSubmitter.Config;
using System;

namespace JiraWorklogSubmitter.Services
{
    public class TimeEntryService : ITimeEntryService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TimeEntryService> _logger;
        private readonly IOptions<JiraSettings> _jiraSettings;

        private const string ApplicationJson = "application/json";

        public static JsonSerializerOptions DefaultJsonSerializerOptions => new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public TimeEntryService(ILogger<TimeEntryService> logger, IHttpClientFactory httpClientFactory, IOptions<JiraSettings> jiraSettings)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _jiraSettings = jiraSettings;
        }

        /// <inheritdoc/>
        public async Task<string> SubmitTimeLogAsync(ICollection<JiraWorklogEntry> jiraWorkLogEntries)
        {
            var httpClientFactory = _httpClientFactory.CreateClient(HttpClientFactoryNameEmum.Jira.ToString());

            foreach (var jiraWorklogEntry in jiraWorkLogEntries.Where(j => !string.IsNullOrEmpty(j.Ticket) && !string.IsNullOrEmpty(j.TimeSpent)))
            {
                var worklogUrl = $"{_jiraSettings.Value.ApiUrl}issue/{jiraWorklogEntry.Ticket}/worklog";

                var request = new HttpRequestMessage(HttpMethod.Post, worklogUrl);

                var jsonBody = JsonSerializer.Serialize(jiraWorklogEntry);

                var jiraWorkLogEntryHttpRequestContent = new StringContent(
                        jsonBody,
                        Encoding.UTF8,
                        ApplicationJson
                    );

                request.Content = jiraWorkLogEntryHttpRequestContent;

                _logger.LogDebug($"Attempting to submit: {jsonBody} to the url: {worklogUrl}");
                using var httpResponse = await httpClientFactory.SendAsync(request);

                //TODO: Need to handle a scenario where one of the submit fails in the middle
                var responseBody = httpResponse.EnsureSuccessStatusCode();
            }

            //TODO: Return some kind of response indicating a success or failure
            return string.Empty;
        }

        /// <inheritdoc/>
        public async Task<string> GetJiraTicketSummaryAsync(string issueKey)
        {
            var httpClientFactory = _httpClientFactory.CreateClient(HttpClientFactoryNameEmum.Jira.ToString());
            var summaryUrl = $"{_jiraSettings.Value.ApiUrl}issue/{issueKey}?fields=summary";

            var request = new HttpRequestMessage(HttpMethod.Get, summaryUrl);

            _logger.LogDebug($"Attempt to get ticket summary for {issueKey} from the url: {summaryUrl}");

            using var httpResponse = await httpClientFactory.SendAsync(request);

            var responseBody = await httpResponse.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            var jiraResponseObject = JsonSerializer.Deserialize<JiraResponseObject>(responseBody, DefaultJsonSerializerOptions);

            _logger.LogDebug($"responseBody: {responseBody}");

            return jiraResponseObject.Fields.Summary;
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetCurrentWeekTicketKeys()
        {
            try
            {
                //TODO: Make a more generic endpoint so we can send different lengths of time (thius week, last week, last two weeks) and maybe authors

                // This query gets a list of the tickets that an author submitted worklogs too, not the actual work logs
                // https://colyar.atlassian.net/rest/api/latest/search?jql=worklogDate >= startOfWeek() and worklogAuthor = "Ryan Taite"&fields=key

                var httpClientFactory = _httpClientFactory.CreateClient(HttpClientFactoryNameEmum.Jira.ToString());
                var dateTime = new DateTime(2020, 7, 31);
                var url = $"{_jiraSettings.Value.ApiUrl}search?jql=worklogDate = \"{dateTime:yyyy-MM-dd}\" and worklogAuthor = \"{_jiraSettings.Value.FullName}\"&fields=summary"; // Returns both the key and summary

                var request = new HttpRequestMessage(HttpMethod.Get, url);

                _logger.LogDebug($"Attempt to get list of worked on tickets from the url: {url}");

                using var httpResponse = await httpClientFactory.SendAsync(request);

                var responseBody = await httpResponse.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

                var jiraResponseObject = JsonSerializer.Deserialize<JiraResponseObject>(responseBody, DefaultJsonSerializerOptions);

                var allKeys = jiraResponseObject.Issues
                    .Select(issue =>
                    {
                        return issue.Key;
                    })
                    .ToList();

                _logger.LogDebug($"responseBody: {responseBody}");

                //TOOD: Return something
                return allKeys;

                /*  Now that I have a list of keys that were worked on for a certain period of time, I should use those keys to look up the issues themselves.
                    How do I filter the worklogs retrived to that of the time period I requested?
                    Do I have to grab all of them from JIRA and filter them here?
                    This link ran into the same scenario, and started off with the same idea: https://community.atlassian.com/t5/Jira-questions/How-to-get-list-of-worklogs-through-JIRA-REST-API/qaq-p/533633
                        Use GET /rest/api/3/worklog/updated to get the IDs of worklogs in the time period. The timestamp refers to the time the worklog has been created/updated, 
                            not the date to which the entry refers. To make sure I have everything, I just go later in the past. The call is paginated, and the response is small, so listing too much is not a big problem. 
                            You just need to remove the worklogs you don't want afterwards
                            Those values look like: 
                            {
                                "worklogId": 10005,
                                "updatedTime": 1550079715574, // The timestamp refers to the time the worklog has been created/updated, not the date to which the entry refers
                                "properties": []
                            },
                            which isn't all that helpful.
                        Use POST /rest/api/3/worklog/list to get the actual worklogs. The payload is the list of IDs to you got in the first step. This is limited to 1000 entries, but you can call it multiple times
                            JSON Payload example: 
                            {
                                "ids": [
                                    10005
                                ]
                            }
                        Bonus - If you need the issues for the retrieved worklogs, use POST /rest/api/3/search. You need to use POST, because the query will be very long and does not fit in the URL. 
                            You can build the query from the issue ids in the worklogs retrieved in step 2 (`id in (12345, 456789, ...)`

                */
            }
            catch (System.Exception exception)
            {
                var innerExceptionMessage = exception.InnerException == null ? "" : exception.InnerException.Message;
                _logger.LogError(exception, $"Something went wrong in {nameof(GetCurrentWeekTicketKeys)}!{Environment.NewLine}Error: {exception.Message}{Environment.NewLine}Inner Exception: {innerExceptionMessage}");
                throw;
            }
            
        }
    }
}
