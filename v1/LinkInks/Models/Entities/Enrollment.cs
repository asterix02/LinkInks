using System;
using System.ComponentModel.DataAnnotations;

namespace LinkInks.Models.Entities
{
    public class Enrollment
    {
        [Key, Required]
        public Guid     EnrollmentId        { get; set; }

        [Required]
        public string   StudentUserName     { get; set; }

        [Required]
        public int      CourseId            { get; set; }

        [Required]
        public bool     IsPending           { get; set; }

        public Decimal? Grade               { get; set; }
    }
}
