using System.Collections.Generic;
using System.Linq;
using LinkInks.Models.Entities;

namespace LinkInks.Models.InstructorViewModels
{
    public class DetailsViewModel
    {
        public string               InstructorUserName  { get; set; }
        public ICollection<Course>  Courses             { get; set; }

        public DetailsViewModel(UniversityDbContext db, string instructorUserName)
        {
            var myCourses = from course in db.Courses
                            where course.InstructorUserName == instructorUserName
                            select course;

            this.InstructorUserName = instructorUserName;
            this.Courses            = new List<Course>();

            foreach (var myCourse in myCourses)
            {
                this.Courses.Add(myCourse);
            }
        }
    }
}