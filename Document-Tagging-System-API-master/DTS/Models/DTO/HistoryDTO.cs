using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTS.Models.DTO
{
    public class HistoryDTO
    {
        public int Id { get; set; }
        public DateTime AddedOn { get; set; }
        public DateTime EditedOn { get; set; }
        public int DocumentId { get; set; }
        public int HistorySequenceId { get; set; }
        //public string Message { get; set; }
        //public DateTime Date { get; set; }
        //public string UserId { get; set; }
        public List<HistorySequence> HistorySequences { get; set; }

        public HistoryDTO()
        {
            HistorySequences = new List<HistorySequence>();
        }
    }
}