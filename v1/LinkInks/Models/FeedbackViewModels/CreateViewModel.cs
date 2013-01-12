using System;
using System.ComponentModel.DataAnnotations;

namespace LinkInks.Models.Entities
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public String Title { get; set; }

        [StringLength(1024)]
        public String Description { get; set; }
    }
}
