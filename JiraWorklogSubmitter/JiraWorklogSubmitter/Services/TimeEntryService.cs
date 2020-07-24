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
            jiraResponseObject.Fields.TryGetValue("summary", out var summary);

            _logger.LogDebug($"responseBody: {responseBody}");

            return summary;
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
                var url = $"{_jiraSettings.Value.ApiUrl}search?jql=worklogDate >= startOfWeek() and worklogAuthor = \"{_jiraSettings.Value.FullName}\"&fields=key";

                var request = new HttpRequestMessage(HttpMethod.Get, url);

                _logger.LogDebug($"Attempt to get list of worked on tickets from the url: {url}");

                using var httpResponse = await httpClientFactory.SendAsync(request);

                var responseBody = await httpResponse.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

                var jiraResponseObject = JsonSerializer.Deserialize<JiraResponseObject>(responseBody, DefaultJsonSerializerOptions);

                var allKeys = jiraResponseObject.Issues
                    .Select(issue =>
                    {
                        issue.TryGetValue("key", out var key);
                        return key;
                    })
                    .ToList();

                _logger.LogDebug($"responseBody: {responseBody}");

                //TOOD: Return something
                return allKeys;
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
