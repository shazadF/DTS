using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTS.Models
{
    public class HistorySequence
    {
        public int Id { get; set; }
        public int HistoryId { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; }
    }
}