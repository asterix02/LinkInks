using System.Web.Mvc;
using System.Web.Security;
using LinkInks.Models;

namespace LinkInks.Controllers
{
    public class HomeController : Controller
    {
        [Authorize(Roles = "Administrator, Instructor, Student")]
        public ActionResult Index()
        {
            switch (AccountController.GetUsersDominantRole())
            {
                case MembershipRole.Administrator:
                    return RedirectToAction("Index", "Administrator");

                case MembershipRole.Instructor:
                    return RedirectToAction("Index", "Instructor");

                case MembershipRole.Student:
                    return RedirectToAction("Index", "Student");

                default:
                    this.ViewBag.Message = "Sorry, you need to be an instructor or student to access this site.";
                    return View();
            }
        }

        [Authorize(Roles = "Administrator, Instructor, Student")]
        public ActionResult Details()
        {
            switch (AccountController.GetUsersDominantRole())
            {
                case MembershipRole.Administrator:
                    return RedirectToAction("Details", "Administrator");

                case MembershipRole.Instructor:
                    return RedirectToAction("Details", "Instructor");

                case MembershipRole.Student:
                    return RedirectToAction("Details", "Student");

                default:
                    this.ViewBag.Message = "Sorry, you need to be an instructor or student to access this site.";
                    return View();
            }
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
