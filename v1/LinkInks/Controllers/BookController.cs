using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using LinkInks.Models;
using LinkInks.Models.Entities;
using LinkInks.Models.BookViewModels;

namespace LinkInks.Controllers
{
    [Authorize(Roles = "Administrator, Instructor, Student")]
    public class BookController : Controller
    {
        // GET: /Book/
        public ActionResult Index()
        {
            return View();
        }

        // GET: /Book/Read/5
        public ActionResult Read(Guid id)
        {
            // If this user has read this book before, try to open the last viewed page
            BookViewState viewState = ControllerHelper.GetViewState(_db, id);
            if (viewState != null)
            {
                return RedirectToAction("Read", "Chapter", new { id = viewState.ChapterId });
            }
            else
            {
                // First time opening the book => go to the Table of Contents
                return RedirectToAction("Contents", new { id });
            }
        }

        // GET: /Book/Contents/5
        public ActionResult Contents(Guid id)
        {
            Book book = _db.Books.Find(id);
            if (book == null)
            {
                throw new ObjectNotFoundException("Could not locate book: " + id);
            }

            return View(book);
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
