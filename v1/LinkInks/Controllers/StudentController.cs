using System.Web.Mvc;
using System.Web.Security;
using LinkInks.Models;
using LinkInks.Models.StudentViewModels;

namespace LinkInks.Controllers
{
    [Authorize(Roles = "Administrator, Student")]
    public class StudentController : Controller
    {
        // GET: /Student/
        public ActionResult Index()
        {
            return View(new IndexViewModel(_db, Membership.GetUser().UserName));
        }

        // GET: /Student/Details
        public ActionResult Details()
        {
            return View(new DetailsViewModel(_db, Membership.GetUser().UserName));
        }

        // GET: /Student/CourseEnroll/5
        public ActionResult CourseEnroll(int id)
        {
            ControllerHelper.RegisterCourse(_db, Membership.GetUser().UserName, id);
            return RedirectToAction("Details");
        }

        // GET: /Student/CourseDrop/5
        public ActionResult CourseDrop(int id)
        {
            ControllerHelper.DropCourse(_db, Membership.GetUser().UserName, id);
            return RedirectToAction("Details");
        }

        // GET: /Student/CourseDetails/5
        public ActionResult CourseDetails(int id)
        {
            return RedirectToAction("Details", "Course", new { id });
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }

        private UniversityDbContext _db = new UniversityDbContext();
    }
}
