using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTS.Models
{
    public class DocumentACL
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public int DocAclType { get; set; }

        public string IsSequential { get; set; }

    }
}