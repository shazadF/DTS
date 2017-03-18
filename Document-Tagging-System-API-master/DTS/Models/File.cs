using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace DTS.Models
{
    public class File
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string Content { get; set; }
    }
}