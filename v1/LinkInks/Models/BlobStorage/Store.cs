using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Hosting;
using System.Web.Security;
using System.Xml;
using LinkInks.Models.Entities;

namespace LinkInks.Models.BlobStorage
{
    public class Store
    {
        public static Store Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        _instance = new Store();
                    }
                }
                return _instance;
            }
        }

        public Dictionary<int, Page> GetBookPagesFromCache(UniversityDbContext db, string bookRelativeUri, Guid requestedChapterId, IList<int> requestedPageNumbers)
        {
            // If not in cache, fetch from disk, and then cache it for future use
            if (!_cachedBooks.ContainsKey(bookRelativeUri))
            {
                // Fetch the book and parse the metadata; bubble up any XML exceptions
                BookBlob bookBlob = DeserializePagesToCache(db, bookRelativeUri);

                // Update cache
                _cachedBooks.Add(bookRelativeUri, bookBlob);
            }

            // Retrieve from cache; if it's not present in the cache even after previous step, then the 
            // caller is asking for non-existent data
            BookBlob cachedBookBlob = _cachedBooks[bookRelativeUri];
            if (cachedBookBlob == null)
            {
                throw new BlobStoreException("Could not locate blob for book: " + bookRelativeUri);
            }

            ChapterBlob cachedChapterBlob = cachedBookBlob.ChapterBlobs[requestedChapterId];
            if (cachedChapterBlob == null)
            {
                throw new BlobStoreException("Could not locate blob for chapter: " + requestedChapterId);
            }

            Dictionary<int, Page> deserializedPages = new Dictionary<int, Page>();
            foreach (int pageNumber in requestedPageNumbers)
            {
                Page page = cachedChapterBlob.ChapterPages[pageNumber];
                if (page == null)
                {
                    throw new BlobStoreException("Could not locate page" + pageNumber + "  in chapter " + requestedChapterId);
                }
                deserializedPages.Add(pageNumber, page);
            }

            return deserializedPages;
        }

        public void RemoveBookFromCache(string bookUri)
        {
            if (_cachedBooks.ContainsKey(bookUri) == false)
            {
                return;
            }

            // Remove any references to the modules (content) within each chapter
            BookBlob bookBlob = _cachedBooks[bookUri];
            foreach (var chapterBlob in bookBlob.ChapterBlobs.Values)
            {
                foreach (var page in chapterBlob.ChapterPages.Values)
                {
                    foreach (var content in page.Contents)
                    {
                        _cachedContents.Remove(content.ModuleId);
                    }
                }

                // Delete the chapter after clearing the contents
                chapterBlob.ChapterPages.Clear();
            }

            // Delete references to the chapters, and then the book itself
            bookBlob.ChapterBlobs.Clear();
            _cachedBooks.Remove(bookUri);
        }

        public Book DeserializeBook(string bookRelativeUri, string authorUserName)
        {
            string bookAbsoluteUri      = ResourceLocator.GetAbsoluteUri(bookRelativeUri);
            XmlDocument document        = new XmlDocument();
            document.Load(bookAbsoluteUri);

            XmlNodeList bookNodes       = document.GetElementsByTagName(BookTag);
            if ((bookNodes == null) || (bookNodes.Count != 1))
            {
                throw new BlobStoreException("Could not locate 'book' tag, or found more than one, in: " + bookAbsoluteUri);
            }

            XmlNodeList chapterNodes    = bookNodes[0].ChildNodes;
            if ((chapterNodes == null) || (chapterNodes.Count == 0))
            {
                throw new BlobStoreException("Could not locate 'chapter' tags under 'book', in: " + bookAbsoluteUri);
            }

            // Note: We use the absolute URI to load the book contents, but when we store the metadata for the book,
            // we only store the relative location. The reason is, if we decide to change the store location (either
            // move to a different store, or use a local store for debugging), we don't want to keep fixing the metadata 
            // each time, and we don't want it to be stale.
            Book book                   = new Book();
            book.AuthorUserName         = authorUserName;
            book.BookId                 = ResourceLocator.GetBookIdFromRelativeUri(bookRelativeUri);
            book.Chapters               = DeserializeChapters(bookRelativeUri, book.BookId, chapterNodes);
            book.ContentLocation        = bookRelativeUri;
            book.CoverPhoto             = ReadAttributeString(bookNodes[0], CoverPhotoAttributeName);
            book.Title                  = ReadAttributeString(bookNodes[0], BookTitleAttributeName);

            return book;
        }

        public Content GetModuleContentFromCache(UniversityDbContext db, Module module)
        {
            if (!_cachedContents.ContainsKey(module.ModuleId))
            {
                DeserializePagesToCache(db, module.Book.ContentLocation);
                if (!_cachedContents.ContainsKey(module.ModuleId))
                {
                    throw new BlobStoreException("Could not locate moduleId " + module.ModuleId + " in book: " + module.BookId);
                }
            }

            return _cachedContents[module.ModuleId];
        }

        private static BookBlob DeserializePagesToCache(UniversityDbContext db, string bookRelativeUri)
        {
            Guid bookId             = ResourceLocator.GetBookIdFromRelativeUri(bookRelativeUri);
            string bookAbsoluteUri  = ResourceLocator.GetAbsoluteUri(bookRelativeUri);

            XmlDocument document    = new XmlDocument();
            document.Load(bookAbsoluteUri);

            XmlNodeList bookNodes = document.GetElementsByTagName(BookTag);
            if ((bookNodes == null) || (bookNodes.Count != 1))
            {
                throw new BlobStoreException("Could not locate 'book' tag, or found more than one, in: " + bookAbsoluteUri);
            }

            XmlNodeList chapterNodes = bookNodes[0].ChildNodes;
            if ((chapterNodes == null) || (chapterNodes.Count == 0))
            {
                throw new BlobStoreException("Could not locate 'chapter' tags under 'book', in: " + bookAbsoluteUri);
            }

            // Since ChapterIds are not stored in the book's serialized XML, we have to retrieve the IDs from the
            // database. If they are not present in the database, we have a mismatch between the blob store and 
            // the database store.
            var dbChapters = db.Chapters.Where(c => c.BookId == bookId).OrderBy(c => c.Index).ToList();
            if (dbChapters.Count != chapterNodes.Count)
            {
                throw new BlobStoreException("Mismatched chapter count for: " + bookId);
            }

            BookBlob bookBlob = new BookBlob();
            bookBlob.ChapterBlobs = new Dictionary<Guid, ChapterBlob>();
            for (int i = 0; i < chapterNodes.Count; i++)
            {
                XmlAttribute chapterIdAttribute = chapterNodes[i].Attributes[ChapterIdAttributeName];
                if ((chapterIdAttribute == null) || (String.IsNullOrEmpty(chapterIdAttribute.Value)))
                {
                    throw new BlobStoreException("Could not find 'id' for chapter");
                }

                ChapterBlob chapterBlob     = new ChapterBlob();
                chapterBlob.ChapterPages    = new Dictionary<int, Page>();
                chapterBlob.PageCount       = chapterNodes[i].ChildNodes.Count;

                foreach (XmlNode pageNode in chapterNodes[i].ChildNodes)
                {
                    Page page = new Page(bookId, pageNode);
                    chapterBlob.ChapterPages.Add(page.PageNumber, page);
                }

                bookBlob.ChapterBlobs.Add(dbChapters[i].ChapterId, chapterBlob);
            }

            return bookBlob;
        }

        private static List<Chapter> DeserializeChapters(string bookRelativeUri, Guid bookId, XmlNodeList chapterNodes)
        {
            List<Chapter> chapters      = new List<Chapter>();
            int chapterIndex            = 0;

            foreach (XmlNode chapterNode in chapterNodes)
            {
                Chapter chapter         = new Chapter();
                chapter.BookId          = bookId;
                chapter.ChapterId       = Guid.NewGuid();
                chapter.ContentLocation = bookRelativeUri;
                chapter.Index           = ++chapterIndex;
                chapter.Modules         = DeserializeModules(bookRelativeUri, bookId, chapterNode.ChildNodes);
                chapter.PageCount       = ReadAttributeInt32(chapterNode, PageCountAttributeName);
                chapter.Title           = ReadAttributeString(chapterNode, ChapterTitleAttributeName);

                chapters.Add(chapter);
            }

            return chapters;
        }

        private static List<Module> DeserializeModules(string bookRelativeUri, Guid bookId, XmlNodeList pageNodes)
        {
            List<Module> modules            = new List<Module>();
            int moduleIndex                 = 0;

            foreach (XmlNode pageNode in pageNodes)
            {
                XmlNodeList moduleNodes     = pageNode.ChildNodes;
                foreach (XmlNode moduleNode in moduleNodes)
                {
                    Module module           = new Module();
                    module.BookId           = bookId;
                    module.ContentLocation  = bookRelativeUri;
                    module.Index            = ++moduleIndex;
                    module.ModuleId         = ReadAttributeGuid(moduleNode, ModuleIdAttributeName);
                    module.PageNumber       = ReadAttributeInt32(pageNode, PageNumberAttributeName);
                    module.SetContentType(Content.ParseContentType(moduleNode.Name));

                    modules.Add(module);
                }
            }

            return modules;
        }

        internal void AddModuleContent(Guid moduleId, Content moduleContent)
        {
            _cachedContents.Add(moduleId, moduleContent);
        }

        internal static string ReadAttributeString(XmlNode node, string attributeName)
        {
            XmlAttribute attribute = node.Attributes[attributeName];
            if ((attribute == null) || (String.IsNullOrEmpty(attribute.Value)))
            {
                throw new BlobStoreException("Could not find " + attributeName + " in: " + node.Name);
            }

            return attribute.Value;
        }

        internal static Int32 ReadAttributeInt32(XmlNode node, string attributeName)
        {
            XmlAttribute attribute = node.Attributes[attributeName];
            if ((attribute == null) || (String.IsNullOrEmpty(attribute.Value)))
            {
                throw new BlobStoreException("Could not find " + attributeName + " in: " + node.Name);
            }

            int result;
            if (Int32.TryParse(attribute.Value, out result))
            {
                return result;
            }
            else
            {
                throw new BlobStoreException("Could not convert " + attributeName + " in: " + node.Name);
            }
        }

        internal static Guid ReadAttributeGuid(XmlNode node, string attributeName)
        {
            XmlAttribute attribute = node.Attributes[attributeName];
            if ((attribute == null) || (String.IsNullOrEmpty(attribute.Value)))
            {
                throw new BlobStoreException("Could not find " + attributeName + " in: " + node.Name);
            }

            Guid result;
            if (Guid.TryParse(attribute.Value, out result))
            {
                return result;
            }
            else
            {
                throw new BlobStoreException("Could not convert " + attributeName + " in: " + node.Name);
            }
        }

        // Settings
        public static string BookTag                                = "book";
        public static string TextTag                                = "text";
        public static string VideoTag                               = "video";
        public static string LinkTag                                = "a";
        public static string PictureTag                             = "img";
        public static string QuestionTag                            = "question";
        public static string QuestionTextTag                        = "questionText";
        public static string AnswerChoiceTag                        = "answerChoice";
        public static string CorrectAnswerTag                       = "correctAnswer";

        public static string BookTitleAttributeName                 = "title";
        public static string CoverPhotoAttributeName                = "coverPhoto";
        public static string ChapterIdAttributeName                 = "id";
        public static string PageCountAttributeName                 = "pageCount";
        public static string ChapterTitleAttributeName              = "title";
        public static string ModuleIdAttributeName                  = "id";
        public static string PageNumberAttributeName                = "pageNumber";
        public static string QuestionPointsAttributeName            = "points";
        public static string LinkSourceAttributeName                = "href";
        public static string PictureSourceAttributeName             = "src";
        public static string VideoSourceAttributeName               = "src";
        public static string TextTitleAttributeName                 = "title";

        public static string LinkInksUrlPrefix                      = "LinkInks://";
        public static string UserDataRoot                           = "/User_Data";

        public static XmlReaderSettings ReaderSettings              = new XmlReaderSettings
        {
            DtdProcessing   = DtdProcessing.Ignore,
            IgnoreComments  = true
        };

        // Internal state
        private static volatile Store               _instance       = null;
        private static object                       _syncRoot       = new Object();

        private Dictionary<string, BookBlob>        _cachedBooks    = new Dictionary<string,BookBlob>();
        private Dictionary<Guid, Content>           _cachedContents = new Dictionary<Guid,Content>();
    }

    public class BlobStoreException : Exception
    {
        public BlobStoreException(string message)
            : base(message)
        {
        }
    }

    public class BookBlob
    {
        public Dictionary<Guid, ChapterBlob>        ChapterBlobs    { get; set; }
    }

    public class ChapterBlob
    {
        public Dictionary<int, Page>                ChapterPages    { get; set; }
        public int                                  PageCount       { get; set; }
    }
}