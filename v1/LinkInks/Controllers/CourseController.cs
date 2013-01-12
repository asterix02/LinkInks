using System.Web;
using System.Web.Mvc;
using LinkInks.Models;
using LinkInks.Models.CourseViewModels;
using System;

namespace LinkInks.Controllers
{
    public class CourseController : Controller
    {
        // GET: /Course/
        public ActionResult Index()
        {
            return View();
        }

        // GET: /Course/Details/5
        public ActionResult Details(int id)
        {
            return View(new DetailsViewModel(_db, id, HttpContext.Profile.UserName));
        }

        // GET: /Course/Waitlist/5
        public ActionResult Waitlist(int id)
        {
            return View(new DetailsViewModel(_db, id, HttpContext.Profile.UserName));
        }

        // GET: /Course/Create
        public ActionResult Create()
        {
            return View(new CreateViewModel());
        }

        // POST: /Course/Create
        [HttpPost]
        public ActionResult Create(CreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.Save(_db);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return View(model);
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("BookLocation", "Could not generate book: " + e.Message);
                return View(model);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            _db = null;

            base.Dispose(disposing);
        }

        private UniversityDbContext _db = new UniversityDbContext();
    }
}
