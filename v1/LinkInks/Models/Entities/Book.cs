using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinkInks.Models.Entities
{
    public class Book
    {
        [Key, Required]
        public Guid     BookId          { get; set; }
        public string   CoverPhoto      { get; set; }

        [Required]
        public string   AuthorUserName  { get; set; }

        [Required]
        public string   Title           { get; set; }

        [Required]
        public string   ContentLocation { get; set; }

        [Required]
        public virtual ICollection<Chapter> Chapters { get; set; }
    }
}