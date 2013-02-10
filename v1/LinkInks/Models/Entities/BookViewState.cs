using System;
using System.ComponentModel.DataAnnotations;

namespace LinkInks.Models.Entities
{
    public class BookViewState
    {
        [Key, Required]
        public Guid     BookViewStateId { get; set; }

        [Required]
        public string   UserName        { get; set; }

        public Guid     BookId          { get; set; }
        public Guid     ChapterId       { get; set; }
        public int      PageNumber      { get; set; }

        public bool     ShowDiscussions { get; set; }
    }
}