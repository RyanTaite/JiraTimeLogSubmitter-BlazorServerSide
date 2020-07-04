using System;
using System.ComponentModel.DataAnnotations;

namespace JiraWorklogSubmitter.Data
{
    public class TimeEntry
    {
        private string _ticket;
        private string _comment;

        [Required]
        public string Ticket { get => _ticket; set => _ticket = value?.Trim(); }

        //[Required]
        public DateTime? StartTime { get; set; }

        //[Required]
        public DateTime? EndTime { get; set; }

        //[Required]
        public TimeSpan? TimeSpan { get; set; }

        public string Comment { get => _comment; set => _comment = value?.Trim(); }
    }
}
