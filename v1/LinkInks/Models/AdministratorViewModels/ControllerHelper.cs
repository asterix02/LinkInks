using System.Data;
using System.Linq;
using LinkInks.Models.Entities;

namespace LinkInks.Models.AdministratorViewModels
{
    public class ControllerHelper
    {
        public static void AddStudentToCourse(UniversityDbContext db, string instructorUserName, string studentUserName, int courseId)
        {
            Course course = db.Courses.SingleOrDefault(c => c.CourseId == courseId);
            if ((course == null) || (course.InstructorUserName != instructorUserName))
            {
                throw new ObjectNotFoundException("Could not locate course " + courseId + " for the currently logged in instructor");
            }

            Enrollment enrollment = db.Enrollments.SingleOrDefault(e => (e.CourseId == courseId && e.StudentUserName == studentUserName));
            if (enrollment == null)
            {
                throw new ObjectNotFoundException("Could not locate the enrollment for the student and course");
            }

            enrollment.IsPending = false;
            db.Entry(enrollment).State = EntityState.Modified;

            db.SaveChanges();
        }

        public static void RemoveStudentFromCourse(UniversityDbContext db, string instructorUserName, string studentUserName, int courseId)
        {
            Course course = db.Courses.SingleOrDefault(c => c.CourseId == courseId);
            if ((course == null) || (course.InstructorUserName != instructorUserName))
            {
                throw new ObjectNotFoundException("Could not locate course " + courseId + " for the currently logged in instructor");
            }

            Enrollment enrollment = db.Enrollments.SingleOrDefault(e => (e.CourseId == courseId && e.StudentUserName == studentUserName));
            if (enrollment == null)
            {
                throw new ObjectNotFoundException("Could not locate the enrollment for the student and course");
            }

            enrollment.IsPending = true;
            db.Entry(enrollment).State = EntityState.Modified;

            db.SaveChanges();
        }
    }
}