using LinkInks.Models;
using LinkInks.Models.Entities;
using System;
using System.Web;
using System.Web.Mvc;

namespace LinkInks.Controllers
{
    public class FeedbackController : Controller
    {
        public ActionResult Create()
        {
            ViewBag.HideFeedbackLink = true;
            return View();
        }

        //
        // POST: /Feedback/Create
        [HttpPost]
        public ActionResult Create(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                _db.Feedbacks.Add(feedback);
                _db.SaveChanges();
            }

            return RedirectToAction("FeedbackComplete");
        }

        //
        // GET: /Feedback/FeedbackComplete
        public ActionResult FeedbackComplete()
        {
            ViewBag.HideFeedbackLink = true;
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }

        private UniversityDbContext _db = new UniversityDbContext();
    }
}
