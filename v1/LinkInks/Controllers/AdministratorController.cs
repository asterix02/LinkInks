using System.Web.Mvc;
using System.Web.Security;
using LinkInks.Models;
using LinkInks.Models.AdministratorViewModels;

namespace LinkInks.Controllers
{
    public class AdministratorController : Controller
    {
        // GET: /Administrator/
        public ActionResult Index()
        {
            return View(new IndexViewModel(_db, Membership.GetUser().UserName));
        }

        // GET: /Administrator/CourseDetails
        public ActionResult CourseDetails(int id)
        {
            return RedirectToAction("Details", "Course", new { id });
        }

        // GET: /Administrator/AddStudentToCourse?studentUserName = "Alice", courseId = 5
        public ActionResult AddStudentToCourse(string studentUserName, int courseId)
        {
            ControllerHelper.AddStudentToCourse(_db, Membership.GetUser().UserName, studentUserName, courseId);
            return RedirectToAction("Details", "Course", new { id = courseId });
        }

        // GET: /Administrator/DropStudentFromCourse?studentUserName = "Alice", courseId = 5
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
