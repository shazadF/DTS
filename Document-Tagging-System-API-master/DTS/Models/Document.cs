using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTS.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string Name { set; get; }
        public string UserId { get; set; }
        public int CompanyId { get; set; }
        public int FileId { get; set; }
        public int DocumentACLId { get; set; }
        public string DocType { get; set; }

       

        
        
    }
}