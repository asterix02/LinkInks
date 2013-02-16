using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Security;
using LinkInks.Models.BlobStorage;
using LinkInks.Models.Entities;

namespace LinkInks.Models.BookViewModels
{
    public class ControllerHelper
    {
        public static Guid CreateBook(UniversityDbContext db, string bookRelativeUri)
        {
            // Ask the blob store to deserialize the file contents
            bookRelativeUri         = bookRelativeUri.Trim(new char[] { ' ', '/', '\\' });
            Book deserializedBook   = Store.Instance.DeserializeBook(bookRelativeUri, Membership.GetUser().UserName);

            // Use the deserialized data to store the schematized book information in the database
            Book book               = db.Books.Create();
            book.AuthorUserName     = deserializedBook.AuthorUserName;
            book.BookId             = deserializedBook.BookId;
            book.Chapters           = new List<Chapter>();
            book.ContentLocation    = deserializedBook.ContentLocation;
            book.CoverPhoto         = deserializedBook.CoverPhoto;
            book.Title              = deserializedBook.Title;

            db.Entry(book).State    = EntityState.Added;
            db.SaveChanges();

            foreach (var deserializedChapter in deserializedBook.Chapters)
            {
                Chapter chapter = CreateChapter(db, deserializedChapter);

                book.Chapters.Add(chapter);
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
            }

            return book.BookId;
        }

        private static Chapter CreateChapter(UniversityDbContext db, Chapter deserializedChapter)
        {
            Chapter chapter         = db.Chapters.Create();
            chapter.BookId          = deserializedChapter.BookId;
            chapter.ChapterId       = deserializedChapter.ChapterId;
            chapter.ContentLocation = deserializedChapter.ContentLocation;
            chapter.Index           = deserializedChapter.Index;
            chapter.Modules         = new List<Module>();
            chapter.PageCount       = deserializedChapter.PageCount;
            chapter.Title           = deserializedChapter.Title;

            db.Entry(chapter).State = EntityState.Added;
            db.SaveChanges();

            foreach (var deserializedModule in deserializedChapter.Modules)
            {
                Module module       = CreateModule(db, deserializedModule);
                chapter.Modules.Add(module);
                db.Entry(chapter).State = EntityState.Modified;
            }

            return chapter;
        }

        private static Module CreateModule(UniversityDbContext db, Module deserializedModule)
        {
            Module module           = new Module();
            module.BookId           = deserializedModule.BookId;
            module.ContentLocation  = deserializedModule.ContentLocation;
            module.Discussions      = new List<Discussion>();
            module.Index            = deserializedModule.Index;
            module.ModuleId         = deserializedModule.ModuleId;
            module.PageNumber       = deserializedModule.PageNumber;

            module.SetContentType(deserializedModule.GetContentType());
            db.Entry(module).State  = EntityState.Added;

            return module;
        }

        public static void DeleteBook(UniversityDbContext db, Guid bookId)
        {
            Book book = db.Books.Include(b => b.Chapters).SingleOrDefault(b => b.BookId == bookId);
            if (book == null)
            {
                throw new ObjectNotFoundException("Could not find boook to delete: " + bookId);
            }

            // Delete the book from the blob store cache
            Store.Instance.RemoveBookFromCache(book.ContentLocation);

            // Delete the book from the database

            // Step 1: Delete each chapter first (recursively deleting its contents)
            List<Chapter> chaptersToDelete = new List<Chapter>();
            chaptersToDelete.AddRange(book.Chapters);

            foreach (Chapter chapterToDelete in chaptersToDelete)
            {
                DeleteChapterModules(db, chapterToDelete.ChapterId);

                book.Chapters.Remove(chapterToDelete);
                db.Entry(chapterToDelete).State = EntityState.Deleted;
                db.Entry(book).State            = EntityState.Modified;
            }

            if (db.Entry(book).State == EntityState.Modified)
            {
                db.SaveChanges();
            }

            // Step 2: Delete the book and its associated state from the database
            DeleteBookViewState(db, bookId);
            db.Entry(book).State = EntityState.Deleted;
            db.SaveChanges();
        }

        private static void DeleteChapterModules(UniversityDbContext db, Guid chapterId)
        {
            Chapter chapter = db.Chapters.Include(c => c.Modules).SingleOrDefault(c => c.ChapterId == chapterId);
            if (chapter == null)
            {
                throw new ObjectNotFoundException("Could not find module to delete: " + chapterId);
            }

            // Find each module to delete before deleting chapter
            List<Module> modulesToDelete = new List<Module>();
            modulesToDelete.AddRange(chapter.Modules);

            foreach (Module moduleToDelete in modulesToDelete)
            {
                DeleteModuleDiscussions(db, moduleToDelete.ModuleId);

                chapter.Modules.Remove(moduleToDelete);
                db.Entry(moduleToDelete).State  = EntityState.Deleted;
                db.Entry(chapter).State         = EntityState.Modified;
            }

            if (db.Entry(chapter).State == EntityState.Modified)
            {
                db.SaveChanges();
            }
        }

        private static void DeleteModuleDiscussions(UniversityDbContext db, Guid moduleId)
        {
            Module module = db.Modules.Include(m => m.Discussions).SingleOrDefault(m => m.ModuleId == moduleId);
            if (module == null)
            {
                throw new ObjectNotFoundException("Could not find module to delete: " + moduleId);
            }

            // Find each discussion to delete
            List<Discussion> discussionsToDelete = new List<Discussion>();
            discussionsToDelete.AddRange(module.Discussions);

            foreach (Discussion discussionToDelete in discussionsToDelete)
            {
                DeleteDiscussionPosts(db, discussionToDelete.DiscussionId);

                module.Discussions.Remove(discussionToDelete);
                db.Entry(discussionToDelete).State  = EntityState.Deleted;
                db.Entry(module).State              = EntityState.Modified;
            }

            if (db.Entry(module).State == EntityState.Modified)
            {
                db.SaveChanges();
            }
        }

        private static void DeleteDiscussionPosts(UniversityDbContext db, Guid discussionId)
        {
            Discussion discussion = db.Discussions.Include(d => d.Posts).SingleOrDefault(d => d.DiscussionId == discussionId);
            if (discussion == null)
            {
                throw new ObjectNotFoundException("Could not find discussion to delete: " + discussionId);
            }

            // Find each discussion post to delete before deleting discussion
            List<DiscussionPost> postsToDelete = new List<DiscussionPost>();
            postsToDelete.AddRange(discussion.Posts);

            foreach (DiscussionPost post in postsToDelete)
            {
                discussion.Posts.Remove(post);
                db.Entry(post).State        = EntityState.Deleted;
                db.Entry(discussion).State  = EntityState.Modified;
            }

            if (db.Entry(discussion).State == EntityState.Modified)
            {
                db.SaveChanges();
            }
        }

        private static void DeleteBookViewState(UniversityDbContext db, Guid bookId)
        {
            var bookViewStates = db.BookViewStates.Where(b => b.BookId == bookId);
            foreach (var bookViewState in bookViewStates)
            {
                db.Entry(bookViewState).State = EntityState.Deleted;
            }
        }

        public static BookViewState GetViewState(UniversityDbContext db, Guid bookId)
        {
            BookViewState viewState = null;
            string userName         = Membership.GetUser().UserName;
            UserState userState     = db.UserStates.Include(u => u.BookViews).SingleOrDefault(u => u.UserName == userName);
            if (userState != null)
            {
                viewState = userState.GetBookViewState(bookId);
            }

            return viewState;
        }
    }
}