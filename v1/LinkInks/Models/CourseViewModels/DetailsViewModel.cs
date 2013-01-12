using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using LinkInks.Controllers;
using LinkInks.Models.Entities;

namespace LinkInks.Models.CourseViewModels
{
    public class DetailsViewModel
    {
        public Course                   Course                  { get; set; }
        public ICollection<string>      EnrolledStudents        { get; set; }
        public ICollection<string>      WaitlistedStudents      { get; set; }

        public bool                     UseAdministratorView    { get; set; }
        public bool                     UseInstructorView       { get; set; }

        public int                      DiscussionsCount        { get; set; }

        // Code Performance Metrics
        public TimeSpan                 EnrollmentQueryTime     { get; set; }
        public TimeSpan                 DiscussionsQueryTime    { get; set; }

        public DetailsViewModel(UniversityDbContext db, int courseId, string userName)
        {
            Course course = db.Courses.Include(c => c.Book).SingleOrDefault(c => c.CourseId == courseId);
            if (course == null)
            {
                throw new ObjectNotFoundException ("Could not locate course: " + courseId);
            }

            this.Course                 = course;
            this.EnrolledStudents       = new List<string>();
            this.WaitlistedStudents     = new List<string>();
            this.UseInstructorView      = (AccountController.GetUsersDominantRole() == MembershipRole.Instructor);
            this.UseAdministratorView   = (AccountController.GetUsersDominantRole() == MembershipRole.Administrator);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (this.UseInstructorView)
            {
                var enrolledStudents = from enrollment in db.Enrollments
                                       where enrollment.CourseId == courseId
                                       select new
                                       {
                                           StudentUserName      = enrollment.StudentUserName,
                                           IsEnrollmentPending  = enrollment.IsPending
                                       };

                foreach (var enrollment in enrolledStudents)
                {
                    if (enrollment.IsEnrollmentPending)
                    {
                        this.WaitlistedStudents.Add(enrollment.StudentUserName);
                    }
                    else
                    {
                        this.EnrolledStudents.Add(enrollment.StudentUserName);
                    }
                }
            }
            stopwatch.Stop();
            this.EnrollmentQueryTime = stopwatch.Elapsed;

            // Basic Analytics: Discussions
            stopwatch.Restart();

            List<Guid> discussionIds = new List<Guid>();
            this.DiscussionsCount = db.Modules.Include(m => m.Discussions)
                                              .Where(m => m.BookId == course.BookId)
                                              .Sum(m => m.Discussions.Count);

            stopwatch.Stop();
            this.DiscussionsQueryTime = stopwatch.Elapsed;
        }
    }
}