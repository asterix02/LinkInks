using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Security;
using LinkInks.Models.BlobStorage;
using LinkInks.Models.Entities;

namespace LinkInks.Models.ChapterViewModels
{
    public class ControllerHelper
    {
        internal static void StartNewDiscussion(UniversityDbContext db, Guid newDiscussionModuleId, string newDiscussionText)
        {
            // Ignore empty comments
            if (String.IsNullOrEmpty(newDiscussionText))
            {
                return;
            }

            Module module = db.Modules.SingleOrDefault(m => m.ModuleId == newDiscussionModuleId);
            if (module == null)
            {
                throw new ObjectNotFoundException("Module not found: " + newDiscussionModuleId);
            }

            string currentUserName = Membership.GetUser().UserName;
            UserState userState = db.UserStates.Include(u => u.BookViews).SingleOrDefault(u => u.UserName == currentUserName);
            if (userState == null)
            {
                throw new ObjectNotFoundException("User state not initialized for current user");
            }

            // Add a new discussion
            Discussion discussion               = db.Discussions.Create();
            discussion.DiscussionId             = Guid.NewGuid();
            discussion.CreationTime             = DateTime.Now;
            discussion.LastModifiedTime         = DateTime.Now;
            discussion.Likes                    = 0;
            discussion.OwnerUserName            = currentUserName;
            discussion.Posts                    = new List<DiscussionPost>();
            discussion.Title                    = newDiscussionText;
            discussion.SetDiscussionType(Discussion.DiscussionType.Conversation);
            module.Discussions.Add(discussion);

            // If the user added a new discussion, she likely wants the Discussions pane to be visible after refresh
            userState.ShowDiscussions = true;
            userState.LastUpdatedDiscussionId = discussion.DiscussionId;

            // Flag the changes and save
            db.Entry(discussion).State          = EntityState.Added;
            db.Entry(module).State              = EntityState.Modified;
            db.Entry(userState).State = EntityState.Modified;
            db.SaveChanges();
        }

        internal static void AddNewPost(UniversityDbContext db, Guid selectedDiscussionId, string newPostText)
        {
            // Ignore empty comments
            if (String.IsNullOrEmpty(newPostText))
            {
                return;
            }

            Discussion discussion = db.Discussions.SingleOrDefault(d => d.DiscussionId.Equals(selectedDiscussionId));
            if (discussion == null)
            {
                throw new ObjectNotFoundException("Discussion not found: " + selectedDiscussionId);
            }

            string currentUserName = Membership.GetUser().UserName;
            UserState userState = db.UserStates.Include(u => u.BookViews).SingleOrDefault(u => u.UserName == currentUserName);
            if (userState == null)
            {
                throw new ObjectNotFoundException("User state not initialized for current user");
            }

            // Add post to discussion
            DiscussionPost post                 = db.DiscussionPosts.Create();
            post.DiscussionPostId               = Guid.NewGuid();
            post.Content                        = newPostText;
            post.CreationTime                   = DateTime.Now;
            post.Likes                          = 0;
            post.OwnerUserName                  = currentUserName;
            discussion.Posts.Add(post);

            // If the user added a post, she probably wants the Discussions pane to be visible after refresh
            userState.ShowDiscussions           = true;
            userState.LastUpdatedDiscussionId   = discussion.DiscussionId;

            // Flag the changes and save
            db.Entry(post).State                = EntityState.Added;
            db.Entry(discussion).State          = EntityState.Modified;
            db.Entry(userState).State           = EntityState.Modified; 
            db.SaveChanges();
        }

        internal static void SetNextPage(UniversityDbContext db)
        {
            UserState userState = GetUserState(db);
            BookViewState bookViewState = userState.GetBookViewState(userState.LastViewedBook);
            Guid chapterId = bookViewState.ChapterId;

            Chapter chapter = db.Chapters.SingleOrDefault(c => c.ChapterId == bookViewState.ChapterId);
            if (chapter == null)
            {
                throw new ObjectNotFoundException("Could not locate chapter: " + bookViewState.ChapterId);
            }

            bookViewState.PageNumber = GetNextPageNumber(chapter, bookViewState.PageNumber);
            userState.SetViewState(db, bookViewState);
        }

        internal static void SetPreviousPage(UniversityDbContext db)
        {
            UserState userState = GetUserState(db);
            BookViewState bookViewState = userState.GetBookViewState(userState.LastViewedBook);
            bookViewState.PageNumber = GetPreviousPageNumber(bookViewState.PageNumber);
            userState.SetViewState(db, bookViewState);
        }

        internal static void AnswerQuiz(UniversityDbContext db, Guid selectedModuleId, string answerChoice)
        {
            // Ignore empty comments
            answerChoice = answerChoice.Trim();
            if (String.IsNullOrEmpty(answerChoice))
            {
                return;
            }

            Module module = db.Modules.Include(m => m.Book).SingleOrDefault(m => m.ModuleId == selectedModuleId);
            if (module == null)
            {
                throw new ObjectNotFoundException("Module not found: " + selectedModuleId);
            }

            QuestionContent questionContent = Store.Instance.GetModuleContentFromCache(db, module) as QuestionContent;
            if (questionContent == null)
            {
                throw new ObjectNotFoundException("Question not found in module: " + selectedModuleId);
            }

            string currentUserName = Membership.GetUser().UserName;
            AnswerState answerState = db.AnswerStates.SingleOrDefault(a => ((a.UserName == currentUserName) &&
                                                                            (a.QuestionModuleId == selectedModuleId)));
            if (answerState == null)
            {
                answerState                     = db.AnswerStates.Create();
                answerState.AnswerStateId       = Guid.NewGuid();
                answerState.QuestionModuleId    = module.ModuleId;
                answerState.UserName            = currentUserName;

                db.Entry(answerState).State     = EntityState.Added;
                db.SaveChanges();
            }

            // Process the answer and save the state
            answerState.Answered            = true;
            answerState.AnsweredCorrectly   = questionContent.CorrectAnswer.Equals(
                                                answerChoice, StringComparison.InvariantCultureIgnoreCase);
            answerState.GivenAnswer         = answerChoice;
            answerState.Score               = (answerState.AnsweredCorrectly ? questionContent.Points : 0);

            db.Entry(answerState).State     = EntityState.Modified;
            db.SaveChanges();
        }

        internal static BookViewState GetLastViewState(UniversityDbContext db)
        {
            UserState userState = GetUserState(db);
            return userState.GetBookViewState(userState.LastViewedBook);
        }

        internal static BookViewState GetLastViewState(UniversityDbContext db, Guid chapterId)
        {
            UserState userState = GetUserState(db);
            BookViewState viewState = userState.GetChapterViewState(chapterId);

            // If this is the first time opening this chapter, we won't have a view state. So, initialize the view state
            // in this particular case. Unfortunately, we may have to requery the chapter details in this special case.
            if (viewState == null)
            {
                Chapter chapter = db.Chapters.Include(c => c.Modules).SingleOrDefault(c => c.ChapterId == chapterId);
                if (chapter == null)
                {
                    throw new ObjectNotFoundException("Could not locate chapter " + chapterId);
                }

                viewState = SetLastViewState(db, chapter.BookId, chapterId, 1);
            }

            return viewState;
        }

        internal static BookViewState SetLastViewState(UniversityDbContext db, Guid bookId, Guid chapterId, int pageNumber)
        {
            UserState userState             = GetUserState(db);
            BookViewState bookViewState     = userState.GetBookViewState(bookId);
            if (bookViewState == null)
            {
                // Use new() instead of db.Create(), as UserState.SetViewState will create a database object
                bookViewState               = new BookViewState(); 
            }

            bookViewState.BookId            = bookId;
            bookViewState.ChapterId         = chapterId;
            bookViewState.PageNumber        = pageNumber;
            bookViewState.ShowDiscussions   = false;

            userState.SetViewState(db, bookViewState);
            return bookViewState;
        }

        private static UserState GetUserState(UniversityDbContext db)
        {
            string userName = Membership.GetUser().UserName;
            UserState userState = db.UserStates.Include(u => u.BookViews).SingleOrDefault(u => u.UserName == userName);
            if (userState == null)
            {
                throw new ObjectNotFoundException("User state not initialized for current user");
            }

            return userState;
        }

        private static int GetNextPageNumber(Chapter chapter, int currentLeftPage)
        {
            int newLeftPage = currentLeftPage + 2;
            return (newLeftPage < chapter.PageCount) ? (newLeftPage) : (currentLeftPage);
        }

        private static int GetPreviousPageNumber(int currentLeftPage)
        {
            int newLeftPage = currentLeftPage - 2;
            return (newLeftPage < 1) ? (currentLeftPage) : (newLeftPage);
        }
    }
}