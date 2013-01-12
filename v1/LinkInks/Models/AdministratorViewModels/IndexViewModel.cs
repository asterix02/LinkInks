using System.Collections.Generic;
using System.Linq;
using LinkInks.Models.Entities;
using System.Data.Entity;

namespace LinkInks.Models.AdministratorViewModels
{
    public class IndexViewModel
    {
        public string                   AdministratorUserName   { get; set; }
        public List<Course>             Courses                 { get; set; }

        public IndexViewModel(UniversityDbContext db, string administratorUserName)
        {
            var myCourses               = db.Courses.AsEnumerable();

            this.AdministratorUserName  = administratorUserName;
            this.Courses                = new List<Course>();

            this.Courses.AddRange(myCourses.OrderBy(c => c.Title));
        }
    }
}