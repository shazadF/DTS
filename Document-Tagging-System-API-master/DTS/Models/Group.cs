using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DTS.Models
{
    public class Group
    {
        public int Id { set; get; }
        [Required]
        public string Name { set; get; }
        [Required]
        public int CompanyId { set; get; }
    }
}