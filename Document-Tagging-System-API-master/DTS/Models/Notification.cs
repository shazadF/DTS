using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTS.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string ToWhom { get; set; }
        public DateTime Date { get; set; }
        public bool IsViewed { get; set; }
        public int DocumentId { get; set; }
    }
}