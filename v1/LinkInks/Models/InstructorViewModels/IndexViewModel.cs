using System.Collections.Generic;
using System.Linq;
using LinkInks.Models.Entities;
using System.Data.Entity;

namespace LinkInks.Models.InstructorViewModels
{
    public class IndexViewModel
    {
        public string                   InstructorUserName  { get; set; }
        public List<Course>             Courses             { get; set; }

        public IndexViewModel(UniversityDbContext db, string instructorUserName)
        {
            var myCourses           = db.Courses.Include(c => c.Book).Where(c => c.InstructorUserName == instructorUserName);

            this.InstructorUserName = instructorUserName;
            this.Courses            = new List<Course>();

            this.Courses.AddRange(myCourses.OrderBy(c => c.Title));
        }
    }
}