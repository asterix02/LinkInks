using System.Collections.Generic;
using System.Linq;
using LinkInks.Models.Entities;

namespace LinkInks.Models.AdministratorViewModels
{
    public class DetailsViewModel
    {
        public string           AdministratorUserName   { get; set; }
        public List<Course>     Courses                 { get; set; }

        public DetailsViewModel(UniversityDbContext db, string instructorUserName)
        {
            var myCourses = db.Courses.AsEnumerable();

            this.AdministratorUserName  = instructorUserName;
            this.Courses                = new List<Course>();

            this.Courses.AddRange(myCourses.OrderBy(c => c.Title));
        }
    }
}