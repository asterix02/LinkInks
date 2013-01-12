using System;
using System.Data;
using System.Linq;
using LinkInks.Models.Entities;

namespace LinkInks.Models.StudentViewModels
{
    public class ControllerHelper
    {
        public static void RegisterCourse(UniversityDbContext db, string studentUserName, int courseId)
        {
            Course course = db.Courses.SingleOrDefault(c => c.CourseId == courseId);
            if (course == null)
            {
                throw new ObjectNotFoundException("Course not found");
            }

            Enrollment enrollment       = db.Enrollments.Create();
            enrollment.EnrollmentId     = Guid.NewGuid();
            enrollment.CourseId         = course.CourseId;
            enrollment.StudentUserName  = studentUserName;
            enrollment.IsPending        = true;

            course.Enrollments.Add(enrollment);

            db.Entry(enrollment).State  = EntityState.Added;
            db.Entry(course).State      = EntityState.Modified;

            db.SaveChanges();
        }

        public static void DropCourse(UniversityDbContext db, string studentUserName, int courseId)
        {
            Course course = db.Courses.SingleOrDefault(c => c.CourseId == courseId);
            if (course == null)
            {
                throw new ObjectNotFoundException("Course not found");
            }

            Enrollment enrollment = db.Enrollments.SingleOrDefault(e => (e.CourseId == courseId && e.StudentUserName == studentUserName));
            if (enrollment == null)
            {
                throw new ObjectNotFoundException("User not enrolled for course: " + courseId);
            }

            course.Enrollments.Remove(enrollment);
            db.Enrollments.Remove(enrollment);

            db.Entry(enrollment).State  = EntityState.Deleted;
            db.Entry(course).State      = EntityState.Modified;

            db.SaveChanges();
        }
    }
}