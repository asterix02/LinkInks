using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Web.Hosting;
using System.Web.Security;
using LinkInks.Models.BlobStorage;
using LinkInks.Models.Entities;

namespace LinkInks.Models.CourseViewModels
{
    public class ControllerHelper
    {
        public static Guid CreateBook(UniversityDbContext db, string bookRelativeLocation)
        {
            // Ask the blob store to deserialize the file contents
            string bookFullPath     = Store.GetFullPath(bookRelativeLocation.Trim(new char[] { ' ', '/', '\\' }));
            Book deserializedBook   = Store.Instance.GetBookModules(bookFullPath);

            // Use the deserialized data to store the schematized book information in the database
            Book book               = db.Books.Create();
            book.AuthorUserName     = Membership.GetUser().UserName;
            book.Chapters           = new List<Chapter>();
            book.ContentLocation    = Path.GetFileName(deserializedBook.ContentLocation);
            book.CoverPhoto         = Path.GetFileName(deserializedBook.CoverPhoto);
            book.Title              = deserializedBook.Title;

            db.Entry(book).State    = EntityState.Added;
            db.SaveChanges();

            foreach (var deserializedChapter in deserializedBook.Chapters)
            {
                Chapter chapter     = CreateChapter(db, book, deserializedChapter);

                book.Chapters.Add(chapter);
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
            }

            return book.BookId;
        }

        private static Chapter CreateChapter(UniversityDbContext db, Book book, Chapter deserializedChapter)
        {
            Chapter chapter         = db.Chapters.Create();
            chapter.BookId          = book.BookId;
            chapter.ContentLocation = Path.GetFileName(book.ContentLocation);
            chapter.Index           = deserializedChapter.Index;
            chapter.Modules         = new List<Module>();
            chapter.PageCount       = deserializedChapter.PageCount;
            chapter.Title           = deserializedChapter.Title;

            db.Entry(chapter).State = EntityState.Added;
            db.SaveChanges();

            foreach (var deserializedModule in deserializedChapter.Modules)
            {
                Module module       = CreateModule(db, book, deserializedModule);
                chapter.Modules.Add(module);
                db.Entry(chapter).State = EntityState.Modified;
            }

            return chapter;
        }

        private static Module CreateModule(UniversityDbContext db, Book book, Module deserializedModule)
        {
            Module module           = new Module();
            module.BookId           = book.BookId;
            module.ContentLocation  = book.ContentLocation;
            module.Discussions      = new List<Discussion>();
            module.Index            = deserializedModule.Index;
            module.ModuleId         = deserializedModule.ModuleId;
            module.PageNumber       = deserializedModule.PageNumber;

            module.SetContentType(deserializedModule.GetContentType());
            db.Entry(module).State  = EntityState.Added;

            return module;
        }
    }
}