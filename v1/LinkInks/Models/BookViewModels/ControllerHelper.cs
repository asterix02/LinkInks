using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Security;
using LinkInks.Models.Entities;

namespace LinkInks.Models.BookViewModels
{
    public class ControllerHelper
    {
        public static BookViewState GetViewState(UniversityDbContext db, Guid bookId)
        {
            BookViewState viewState = null;
            string userName = Membership.GetUser().UserName;
            UserState userState = db.UserStates.Include(u => u.BookViews).SingleOrDefault(u => u.UserName == userName);
            if (userState != null)
            {
                viewState = userState.GetBookViewState(bookId);
            }

            return viewState;
        }
    }
}