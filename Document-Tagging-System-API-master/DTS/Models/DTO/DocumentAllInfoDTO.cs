using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTS.Models.DTO
{
    public class DocumentAllInfoDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public int CompanyId { get; set; }
        public int FileId { set; get; }
        public string Content { set; get; }
        public string DocType { get; set; }
        public int DocumentACLType { get; set; }
        public History History { get; set; }
        public List<HistorySequence> HistorySequences { set; get; }
        public List<Tag> Tags { get; set; }
            

        public DocumentAllInfoDTO()
        {
            Tags = new List<Tag>();
            HistorySequences = new List<HistorySequence>();
        }
    }
}