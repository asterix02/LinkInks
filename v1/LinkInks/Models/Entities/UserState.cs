using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Security;

namespace LinkInks.Models.Entities
{
    public class UserState
    {
        [Key, Required(ErrorMessage="Missing UserStateId")]
        public int UserStateId { get; set; }

        [Required(ErrorMessage = "Missing UserName")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Missing BookViews")]
        public virtual ICollection<BookViewState> BookViews { get; protected set; }
        public Guid LastViewedBook { get; protected set; }

        public bool ShowDiscussions { get; set; }
        public Guid LastUpdatedDiscussionId { get; set; }

        public void Initialize(string userName)
        {
            this.UserName                   = userName;
            this.BookViews                  = new List<BookViewState>();
            this.LastViewedBook             = Guid.Empty;
            this.ShowDiscussions            = false;
            this.LastUpdatedDiscussionId    = Guid.Empty;
        }

        public BookViewState GetBookViewState(Guid bookId)
        {
            if (this.BookViews != null)
            {
                foreach (var view in this.BookViews)
                {
                    if (view.BookId == bookId)
                    {
                        return view;
                    }
                }
            }

            return null;
        }

        public BookViewState GetChapterViewState(Guid chapterId)
        {
            if (this.BookViews != null)
            {
                foreach (var view in this.BookViews)
                {
                    if (view.ChapterId == chapterId)
                    {
                        return view;
                    }
                }
            }

            return null;
        }

        public static void ResetViewStatesForUsers(UniversityDbContext db, Guid bookId)
        {
            var userStates = db.UserStates.Include(u => u.BookViews).Where(u => u.LastViewedBook == bookId);
            foreach (var userState in userStates)
            {
                BookViewState storedViewState = userState.GetBookViewState(bookId);
                if (storedViewState != null)
                {
                    userState.BookViews.Remove(storedViewState);

                    db.Entry(storedViewState).State = EntityState.Deleted;
                    db.Entry(userState).State       = EntityState.Modified;
                }
            }
            db.SaveChanges();
        }

        public void SetViewState(UniversityDbContext db, BookViewState bookViewState)
        {
            this.LastViewedBook = bookViewState.BookId;

            BookViewState storedViewState = GetBookViewState(bookViewState.BookId);
            if (storedViewState != null)
            {
                storedViewState.ChapterId       = bookViewState.ChapterId;
                storedViewState.PageNumber      = bookViewState.PageNumber;

                db.Entry(storedViewState).State = EntityState.Modified;
                db.Entry(this).State            = EntityState.Modified;
            }
            else
            {
                storedViewState                 = db.BookViewStates.Create();
                storedViewState.BookViewStateId = Guid.NewGuid();
                storedViewState.BookId          = bookViewState.BookId;
                storedViewState.ChapterId       = bookViewState.ChapterId;
                storedViewState.PageNumber      = bookViewState.PageNumber;
                storedViewState.ShowDiscussions = false;
                storedViewState.UserName        = Membership.GetUser().UserName;
                
                db.Entry(storedViewState).State = EntityState.Added;
                db.Entry(this).State            = EntityState.Modified;
                this.BookViews.Add(storedViewState);
            }

            db.SaveChanges();
        }
    }
}