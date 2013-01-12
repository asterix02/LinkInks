using System.Web.Mvc;
using System.Web.Security;
using LinkInks.Models;
using LinkInks.Models.InstructorViewModels;

namespace LinkInks.Controllers
{
    [Authorize(Roles = "Administrator, Instructor")]
    public class InstructorController : Controller
    {
        // GET: /Instructor/
        public ActionResult Index()
        {
            return View(new IndexViewModel(_db, Membership.GetUser().UserName));
        }

        // GET: /Instructor/Details
        public ActionResult Details()
        {
            return View(new DetailsViewModel(_db, Membership.GetUser().UserName));
        }

        // GET: /Instructor/AddStudentToCourse?studentUserName = "Alice", courseId = 5
        public ActionResult AddStudentToCourse(string studentUserName, int courseId)
        {
            ControllerHelper.AddStudentToCourse(_db, Membership.GetUser().UserName, studentUserName, courseId);
            return RedirectToAction("Details", "Course", new { id = courseId });
        }

        // GET: /Instructor/DropStudentFromCourse?studentUserName = "Alice", courseId = 5
        public ActionResult DropStudentFromCourse(string studentUserName, int courseId)
        {
            ControllerHelper.RemoveStudentFromCourse(_db, Membership.GetUser().UserName, studentUserName, courseId);
            return RedirectToAction("Details", "Course", new { id = courseId });
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }

        private UniversityDbContext _db = new UniversityDbContext();
    }
}
