using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DTS.Models.DTO
{
    public class TagDTO
    {
        public int Id { get; set; }
        [Required]
        public int DocumentId { get; set; }
        [Required]
        public string TagContent { get; set; }
        
        public string TagColor { get; set; }
        [Required]
        public string UserId { get; set; }

        //From History
        public DateTime EditedOn { get; set; }

        //From History Sequence 
        public string Message { get; set; }
    }
}