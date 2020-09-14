using JiraWorklogSubmitter.Data;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JiraWorklogSubmitter.Pages.Components
{
    public partial class WorklogEntry : ComponentBase
    {
        [Parameter]
        public JiraWorklogEntry JiraWorklogEntry { get; set; }

        [Parameter]
        public ICollection<JiraWorklogEntry> JiraWorklogEntries { get; set; }

        [Parameter]
        public string _totalTimeSpent { get; set; }

        protected override async Task OnInitializedAsync()
        {
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

            var allTimeStrings = JiraWorklogEntries.Select(worklogEntry => worklogEntry.TimeSpent).ToList();
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
                        Console.WriteLine($"Adding {newTimeSpan} to {totalTimeSpan}");
                        totalTimeSpan = totalTimeSpan.Add(newTimeSpan);
                        Console.WriteLine($"Totalling {totalTimeSpan}");
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

        private void DeleteEntry(JiraWorklogEntry jiraWorklogEntry)
        {
            JiraWorklogEntries.Remove(jiraWorklogEntry);
        }
    }
}
