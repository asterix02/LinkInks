using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinkInks.Models.Entities
{
    public class Course
    {
        [Key, Required]
        public int      CourseId                { get; set; }

        [Required]
        public string   Title                   { get; set; }

        [Required]
        public int      Credits                 { get; set; }

        [Required]
        public string   InstructorUserName      { get; set; }

        public Guid     BookId                  { get; set; }
        public Book     Book                    { get; set; }

        [Required]
        public virtual  ICollection<Enrollment> Enrollments { get; set; }
    }
}