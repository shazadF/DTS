using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DTS.Models.DTO
{
    public class DocumentDTO
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string UserId { get; set; }
        
        public int CompanyId { get; set; }

        public int FileId { set; get; }
        [Required]
        public string DocType { get; set; }
        [Required]
        public string FileContent { set; get; }
        public DateTime AddedOn { get; set; }
        public DateTime EditedOn { get; set; }
        public int DocumentACLType { get; set; }
    }
}