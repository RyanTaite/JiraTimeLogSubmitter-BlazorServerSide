using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace JiraWorklogSubmitter.Data
{
    public class TimeEntryService
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

        public async Task<string> SubmitTimeLogAsync(ICollection<JiraWorklogEntry> jiraWorkLogEntries)
        {
            //TODO: Make a named httpClient for the jira site
            var httpClientFactory = _httpClientFactory.CreateClient("jira");

            var test = await GetJiraTicketSummaryAsync(jiraWorkLogEntries.First().Ticket);

            
            foreach (var jiraWorklogEntry in jiraWorkLogEntries.Where(j => !string.IsNullOrEmpty(j.Ticket) && !string.IsNullOrEmpty(j.TimeSpent)))
            {
                var worklogUrl = $"{_jiraSettings.Value.ApiUrl}{jiraWorklogEntry.Ticket}worklog";

                var request = new HttpRequestMessage(HttpMethod.Post, worklogUrl);

                var jsonBody = JsonSerializer.Serialize(jiraWorklogEntry);

                var jiraWorkLogEntryHttpRequestContent = new StringContent(
                        jsonBody,
                        Encoding.UTF8,
                        ApplicationJson
                    );

                request.Content = jiraWorkLogEntryHttpRequestContent;

                _logger.LogDebug($"Attempting to submit: {jsonBody} to the url: {worklogUrl}");

                //TURN OFF WHILE MESSING AROUND
                //using var httpResponse = await httpClientFactory.SendAsync(request);

                //var responseBody = httpResponse.EnsureSuccessStatusCode();
            }

            return "Pretend this is a nicely formatted Teams message";
        }

        public async Task<string> GetJiraTicketSummaryAsync(string issueKey)
        {
            var httpClientFactory = _httpClientFactory.CreateClient("jira");
            var summaryUrl = $"{_jiraSettings.Value.ApiUrl}{issueKey}?fields=summary";

            var request = new HttpRequestMessage(HttpMethod.Get, summaryUrl);

            _logger.LogDebug($"Attempt to get ticket summary for {issueKey} from the url: {summaryUrl}");

            using var httpResponse = await httpClientFactory.SendAsync(request);

            var responseBody = await httpResponse.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var jiraResponseObject = JsonSerializer.Deserialize<JiraResponseObject>(responseBody, jsonSerializerOptions);
            jiraResponseObject.Fields.TryGetValue("summary", out var summary);

            _logger.LogDebug($"responseBody: {responseBody}");

            return summary;
        }
    }
}
