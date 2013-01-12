using System.Collections.Generic;
using System.Linq;
using LinkInks.Models.Entities;
using System.Configuration;

namespace LinkInks.Models.StudentViewModels
{
    public class IndexViewModel
    {
        public string                   StudentUserName     { get; set; }
        public List<CourseInfo>         CourseInfos         { get; set; }

        public IndexViewModel(UniversityDbContext db, string studentUserName)
        {
            var myCourses = from course in db.Courses
                            join enrollment in db.Enrollments
                            on course.CourseId equals enrollment.CourseId
                            where (enrollment.StudentUserName == studentUserName)
                            select new CourseInfo
                            {
                                Title               = course.Title,
                                IsEnrollmentPending = enrollment.IsPending,
                                Book                = course.Book
                            };

            this.StudentUserName = studentUserName;
            this.CourseInfos     = new List<CourseInfo>();

            this.CourseInfos.AddRange(myCourses.OrderBy(c => c.Title));
        }

        public class CourseInfo
        {
            public string   Title               { get; set; }
            public bool     IsEnrollmentPending { get; set; }
            public Book     Book                { get; set; }
        }
    }
}