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
    public class ReadViewModel
    {
        public ReadViewModel(UniversityDbContext db)
            : this(db, ControllerHelper.GetLastViewState(db))
        {
        }

        public ReadViewModel(UniversityDbContext db, int chapterId)
            : this(db, ControllerHelper.GetLastViewState(db, chapterId))
        {
        }

        public ReadViewModel(UniversityDbContext db, BookViewState bookViewState)
        {
            if (bookViewState == null)
            {
                throw new ObjectNotFoundException("Could not find last viewed book state for user");
            }

            Chapter chapter = db.Chapters.Include(c => c.Modules).SingleOrDefault(c => c.ChapterId == bookViewState.ChapterId);
            if (chapter == null)
            {
                throw new ObjectNotFoundException("Could not locate chapter " + bookViewState.ChapterId);
            }

            this.LeftPageNumber                     = bookViewState.PageNumber;
            this.RightPageNumber                    = this.LeftPageNumber + 1;

            // Get the schematized data from the database...
            this.BookId                             = chapter.BookId;
            this.ChapterId                          = bookViewState.ChapterId;
            this.Index                              = chapter.Index;
            this.LastPageNumber                     = chapter.PageCount;
            this.Title                              = chapter.Title;

            var allModules                          = chapter.Modules.OrderBy(m => m.Index);
            ICollection<Module> leftPageModules     = allModules.Where(m => m.PageNumber == this.LeftPageNumber).ToList();
            ICollection<Module> rightPageModules    = allModules.Where(m => m.PageNumber == this.RightPageNumber).ToList();

            // ... next, fetch the content from the blob store
            Store blobStore                         = Store.Instance;
            Dictionary<int, Page> pages             = blobStore.GetBookPages(chapter.BookId, chapter.GetContentLocationUri(), this.ChapterId, 
                                                       new List<int>() { this.LeftPageNumber, this.RightPageNumber });

            ICollection<Content> leftPageContents   = (pages[this.LeftPageNumber] != null ? pages[this.LeftPageNumber].Contents : new List<Content>());
            ICollection<Content> rightPageContents  = (pages[this.RightPageNumber] != null ? pages[this.RightPageNumber].Contents : new List<Content>());

            // ... and merge that into a form appropriate for the view model
            this.LeftPageModuleViewModels           = new List<ModuleViewModel>();
            this.RightPageModuleViewModels          = new List<ModuleViewModel>();

            BuildModuleViewModels(this.LeftPageModuleViewModels, leftPageModules, leftPageContents);
            BuildModuleViewModels(this.RightPageModuleViewModels, rightPageModules, rightPageContents);
        }

        private void BuildModuleViewModels(ICollection<ModuleViewModel> viewModels, ICollection<Module> modules, ICollection<Content> contents)
        {
            if (modules.Count != contents.Count)
            {
                throw new ArgumentException("Modules and contents are mismatched in size: " + modules.Count + ", " + contents.Count);
            }

            for (int i = 0; i < modules.Count; i++)
            {
                viewModels.Add(new ModuleViewModel(modules.ElementAt(i), contents.ElementAt(i)));
            }
        }

        public void Dispose()
        {
            _db.Dispose();
            _db = null;
        }

        public Guid                     BookId              { get; private set; }
        public int                      ChapterId           { get; private set; }
        public string                   Title               { get; private set; }
        public int                      Index               { get; private set; }
        public string                   ContentLocation     { get; private set; }

        public int                      LeftPageNumber      { get; private set; }
        public int                      RightPageNumber     { get; private set; }
        public int                      LastPageNumber      { get; private set; }

        public ICollection<ModuleViewModel> LeftPageModuleViewModels    { get; private set; }
        public ICollection<ModuleViewModel> RightPageModuleViewModels   { get; private set; }

        public ICollection<Discussion>  Discussions         { get; private set; }
        public bool                     ShowDiscussions     { get; private set; }

        private UniversityDbContext     _db;
    }
}