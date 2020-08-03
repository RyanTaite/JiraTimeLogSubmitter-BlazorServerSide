using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace JiraWorklogSubmitter.Data
{
    /// <summary>
    /// This is to match to the JIRA Api when we serilize to JSON
    /// </summary>
    /// <remarks>
    /// JIRA Api is case sensitve, which is why we have the JsonPropertyNames
    /// </remarks>
    public class JiraWorklogEntry
    {
        private string _ticket;
        private string _timeSpent;
        private string _comment;
        private string _summary;

        [JsonIgnore]    // The JIRA API can't use this
        public Guid Id { get; set; }

        /// <summary>
        /// The ticket you want to add the Worklog entry to, ex: "CTS-302"
        /// We use this to craft the URL. It's ignored from the JSON payload.
        /// </summary>
        [Required]
        [JsonIgnore]    // The JIRA API does not want this
        public string Ticket { get => _ticket; set => _ticket = value?.Trim(); }

        /// <summary>
        /// How much time was spent. Has to be at least a minute.
        /// Expected format is some form of "#d #h #m"
        /// </summary>
        [Required]
        [JsonProperty("timeSpent")]
        [DisplayName("Time Spent")]
        public string TimeSpent { get => _timeSpent; set => _timeSpent = value?.Trim(); }

        /// <summary>
        /// Any comments you want to attach to the worklog entry. Does not have any formatting support (I think)
        /// </summary>
        [JsonProperty("comment")]
        public string Comment { get => _comment; set => _comment = value?.Trim(); }

        [JsonIgnore]    // The JIRA API does not want this
        public string Summary { get => _summary; set => _summary = value?.Trim(); }

        public JiraWorklogEntry()
        {
            Id = Guid.NewGuid();
        }
    }
}
