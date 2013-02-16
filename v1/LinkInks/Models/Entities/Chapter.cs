using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinkInks.Models.Entities
{
    public class Chapter
    {
        [Key, Required]
        public Guid         ChapterId                   { get; set; }

        [ForeignKey("Book")]
        public Guid         BookId                      { get; set; }
        public virtual Book Book                        { get; set; }

        [Required]
        public string       ContentLocation             { get; set; }

        [Required]
        public int          Index                       { get; set; }

        [Required]
        public int          PageCount                   { get; set; }

        [Required]
        public string       Title                       { get; set; }

        [Required]
        public virtual ICollection<Module> Modules      { get; set; }
    }
}