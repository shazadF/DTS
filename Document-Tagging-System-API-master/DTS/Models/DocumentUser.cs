using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTS.Models
{
    public class DocumentUser
    {
        public int Id { set; get; }
        public int DocumentId { set; get; }
        public string UserId { get; set; }
    }
}