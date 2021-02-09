using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JiraWorklogSubmitter.Data.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

        /// <summary>
        /// When did you work on this ticket?
        /// </summary>
        /// <remarks>
        /// The format should be:   "yyyy-MM-dd'T'HH:mm:ss.SSSZ"
        /// Example:                "2001-07-04T12:08:56.235-0700"
        /// Note the lack of a ':'
        /// It's based of Java Datetime formats
        /// </remarks>
        [Required]
        [JsonProperty("started")]
        [JsonConverter(typeof(JiraDateTimeConverter))]
        public DateTime Started { get; set; }

        public JiraWorklogEntry()
        {
            Id = Guid.NewGuid();
            Started = DateTime.Today;
        }
    }
}
