using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Security;
using LinkInks.Models.Entities;

namespace LinkInks.Models.CourseViewModels
{
    public class CreateViewModel
    {
        public string   Title               { get; set; }
        public int      Credits             { get; set; }
        public string   InstructorUserName  { get; set; }
        
        public string   BookTitle           { get; set; }
        public string   BookLocation        { get; set; }

        public CreateViewModel()
        {
            this.InstructorUserName     = Membership.GetUser().UserName;
        }

        public void Save(UniversityDbContext db)
        {
            Guid bookId                 = BookViewModels.ControllerHelper.CreateBook(db, this.BookLocation);

            Course course               = db.Courses.Create();
            course.BookId               = bookId;
            course.Credits              = this.Credits;
            course.Enrollments          = new List<Enrollment>();
            course.InstructorUserName   = Membership.GetUser().UserName;
            course.Title                = this.Title;

            db.Entry(course).State      = EntityState.Added;
            db.SaveChanges();
        }
    }
}