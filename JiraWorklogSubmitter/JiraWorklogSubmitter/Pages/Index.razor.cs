using JiraWorklogSubmitter.Data;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JiraWorklogSubmitter.Pages
{
    public partial class Index : ComponentBase
    {
        private readonly ICollection<JiraWorklogEntry> _jiraWorklogEntries = new List<JiraWorklogEntry>() { new JiraWorklogEntry() };
        private string _totalTimeSpent = string.Empty;

        protected override async Task OnInitializedAsync()
        {
        }

        private void AddNewTimeEntry()
        {
            var newTimeEntry = new JiraWorklogEntry();
            _jiraWorklogEntries.Add(newTimeEntry);
        }

        private async Task HandleValidSubmitAsync()
        {
            // I believe this is still being called, even when a JiraWorklogEntry has invalid data, because I'm working with a collection of objects rather than individual objects.
            // ".NET Core 3.1 Preview 2 introduces experimental support for object graph validation using data annotations" tells me that my list validation simply isn't supported yet.
            Console.WriteLine("Handling Valid Submit");
            await TimeEntryService.SubmitTimeLogAsync(_jiraWorklogEntries);
        }

        private void DeleteEntry(JiraWorklogEntry jiraWorklogEntry)
        {
            _jiraWorklogEntries.Remove(jiraWorklogEntry);
        }

        private async Task GetJiraTicketSummaryAsync(JiraWorklogEntry jiraWorklogEntry)
        {
            var result = await TimeEntryService.GetJiraTicketSummaryAsync(jiraWorklogEntry.Ticket);
            jiraWorklogEntry.Summary = result;
        }

        private void UpdateTotalTime()
        {
            //TODO: Need to handle the fact that 8h is considered 1d to Jira, so maybe manipulate the time span to show that? Might be company based though...

            const string patternDay = @"((?<day>\d+)\s?[dD])";
            const string patternHour = @"((?<hour>\d+)\s?[hH])";
            const string patternMinute = @"((?<minute>\d+)\s?[mM])";

            var patternCombined = $@"{patternDay}?\s?{patternHour}?\s?{patternMinute}?";

            var regex = new Regex(patternCombined);

            var allTimeStrings = _jiraWorklogEntries.Select(worklogEntry => worklogEntry.TimeSpent).ToList();
            var totalTimeSpan = new TimeSpan(); // Clear the current value since we are going to build a new one

            foreach (var timeString in allTimeStrings.Where(t => !string.IsNullOrEmpty(t)))
            {
                try
                {
                    var match = regex.Match(timeString);
                    if (match.Success)
                    {
                        int.TryParse(match.Groups["day"].Value, out var day);
                        int.TryParse(match.Groups["hour"].Value, out var hour);
                        int.TryParse(match.Groups["minute"].Value, out var minute);
                        var newTimeSpan = new TimeSpan(day, hour, minute);
                        totalTimeSpan = totalTimeSpan.Add(newTimeSpan);
                    }
                    else
                    {
                        Console.WriteLine($"No match on {timeString}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occured when trying to parse the time for: {timeString}{Environment.NewLine}Error: {ex.Message}");
                }
                
            }

            _totalTimeSpent = totalTimeSpan.ToString();
        }
    }
}
