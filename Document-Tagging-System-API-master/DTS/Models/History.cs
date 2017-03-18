using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTS.Models
{
    public class History
    {
        public int Id { get; set; }
        public DateTime AddedOn { get; set; }
        public DateTime EditedOn { get; set; }
        public int DocumentId { get; set; }
       
    }
}