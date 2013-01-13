using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using LinkInks.Models;
using LinkInks.Models.ChapterViewModels;
using LinkInks.Models.Entities;

namespace LinkInks.Controllers
{
    [Authorize(Roles = "Administrator, Instructor, Student")]
    public class ChapterController : Controller
    {
        // GET: /Chapter/
        public ActionResult Index()
        {
            return View(new ReadViewModel(_db));
        }

        // GET: /Chapter/Read/5
        public ActionResult Read(int id)
        {
            this.ViewBag.HideFeedbackLink = true;
            return View(new ReadViewModel(_db, id));
        }

        // GET: /Chapter/Create
        public ActionResult Create()
        {
            return View();
        } 

        // POST: /Chapter/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        
        // GET: /Chapter/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: /Chapter/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: /Chapter/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: /Chapter/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult BookAsyncRequest(string submitAction)
        {
            switch (submitAction)
            {
                case "viewNextPage":
                    ControllerHelper.SetNextPage(_db);
                    break;

                case "viewPreviousPage":
                    ControllerHelper.SetPreviousPage(_db);
                    break;

                default:
                    break;
            }

            // The async request expects to render just a partial view, specifically, modules filtered to that page
            return PartialView("_Book", new ReadViewModel(_db));
        }

        [HttpPost] 
        public ActionResult DiscussionsAsyncRequest(string discussionsSubmitAction, Guid selectedModuleId, string newDiscussionText,
            Guid selectedDiscussionId, string newPostText, string answerChoice)
        {
            switch (discussionsSubmitAction)
            {
                case "answerQuiz":
                    ControllerHelper.AnswerQuiz(_db, selectedModuleId, answerChoice);
                    break;

                case "addNewDiscussion":
                    ControllerHelper.StartNewDiscussion(_db, selectedModuleId, newDiscussionText);
                    break;

                case "addNewPost":
                    ControllerHelper.AddNewPost(_db, selectedDiscussionId, newPostText);
                    break;

                case "refreshDiscussions":
                default:
                    break;
            }

            // The async request expects to render just a partial view, specifically, discussions filtered to the selected module
            return PartialView("_Discussions", new DiscussionsViewModel(_db, selectedModuleId));
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }

        private UniversityDbContext _db = new UniversityDbContext();
    }
}
