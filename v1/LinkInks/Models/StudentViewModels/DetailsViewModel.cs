using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using LinkInks.Models.Entities;

namespace LinkInks.Models.StudentViewModels
{
    public class DetailsViewModel
    {
        public string                   StudentUserName     { get; set; }
        public ICollection<Course>      RegisteredCourses   { get; set; }
        public ICollection<Course>      WaitlistedCourses   { get; set; }
        public ICollection<Course>      AvailableCourses    { get; set; }

        public DetailsViewModel(UniversityDbContext db, string studentUserName)
        {
            var myCourseList = from course in db.Courses
                                join enrollment in db.Enrollments
                                on course.CourseId equals enrollment.CourseId
                                where (enrollment.StudentUserName == studentUserName)
                                select new
                                { 
                                    Course = course,
                                    IsEnrollmentPending = enrollment.IsPending
                                };

            this.StudentUserName    = studentUserName;
            this.RegisteredCourses  = new List<Course>();
            this.WaitlistedCourses  = new List<Course>();
            this.AvailableCourses   = new List<Course>();

            foreach (var courseInfo in myCourseList)
            {
                if (courseInfo.IsEnrollmentPending)
                {
                    this.WaitlistedCourses.Add(courseInfo.Course);
                }
                else
                {
                    this.RegisteredCourses.Add(courseInfo.Course);
                }
            }

            foreach (var course in db.Courses)
            {
                if (!this.RegisteredCourses.Contains(course) && !this.WaitlistedCourses.Contains(course))
                {
                    this.AvailableCourses.Add(course);
                }
            }
        }
    }
}