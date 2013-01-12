using System;
using System.Collections.Generic;
using System.Xml;
using LinkInks.Models.Entities;
using System.Web.Hosting;
using System.Web.Security;

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

        public static String GetAbsoluteUri(Guid bookId, String fileName)
        {
            try
            {
                Uri u = new Uri(fileName);
                if (u.IsAbsoluteUri)
                {
                    return fileName;
                }
            }
            catch (Exception e)
            {
            }
            String baseUri = System.Configuration.ConfigurationManager.AppSettings["blobStoreBaseUri"];
            String relativeUri = bookId.ToString().ToLower() + '/' + fileName;

            return baseUri + relativeUri;
        }

        public Dictionary<int, Page> GetBookPages(Guid bookId, string bookUri, int requestedChapterId, IList<int> requestedPageNumbers)
        {
            // If not in cache, fetch from disk, and then cache it for future use
            if (!_cachedBooks.ContainsKey(bookUri))
            {
                // Fetch the book and parse the metadata; bubble up any XML exceptions
                BookBlob bookBlob = DeserializePages(bookId, bookUri);

                // Update cache
                _cachedBooks.Add(bookUri, bookBlob);
            }

            // Retrieve from cache; if it's not present in the cache even after previous step, then the 
            // caller is asking for non-existent data
            BookBlob cachedBookBlob = _cachedBooks[bookUri];
            if (cachedBookBlob == null)
            {
                throw new BlobStoreException("Could not locate blob for book: " + bookUri);
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

        public Book GetBookModules(string bookUri)
        {
            XmlDocument document = new XmlDocument();
            document.Load(bookUri);

            XmlNodeList bookNodes = document.GetElementsByTagName(BookTag);
            if ((bookNodes == null) || (bookNodes.Count != 1))
            {
                throw new BlobStoreException("Could not locate 'book' tag, or found more than one, in: " + bookUri);
            }

            XmlNodeList chapterNodes = bookNodes[0].ChildNodes;
            if ((chapterNodes == null) || (chapterNodes.Count == 0))
            {
                throw new BlobStoreException("Could not locate 'chapter' tags under 'book', in: " + bookUri);
            }

            Book book                   = new Book();
            book.Chapters               = DeserializeChapters(bookUri, chapterNodes);
            book.ContentLocation        = bookUri;
            book.CoverPhoto             = ConvertUrl(ReadAttributeString(bookNodes[0], CoverPhotoAttributeName));
            book.Title                  = ReadAttributeString(bookNodes[0], BookTitleAttributeName);

            return book;
        }

        public Content GetModuleContent(Module module)
        {
            if (!_cachedContents.ContainsKey(module.ModuleId))
            {
                DeserializePages(module.BookId, module.Book.ContentLocation);
                if (!_cachedContents.ContainsKey(module.ModuleId))
                {
                    throw new BlobStoreException("Could not locate moduleId " + module.ModuleId + " in book: " + module.BookId);
                }
            }

            return _cachedContents[module.ModuleId];
        }

        public static string ConvertUrl(string customUrl)
        {
            // Ignore HTTP URLs
            if (customUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            {
                return customUrl;
            }

            // All other URLs must start with the LinkInksUrlPrefix
            if (!customUrl.StartsWith(LinkInksUrlPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new NotSupportedException(customUrl + " is not supported; only LinkInks:// URLs are supported");
            }

            string canonizedUrl = customUrl.Remove(0, LinkInksUrlPrefix.Length).Replace('\\', '/');
            return String.Format("~{0}/{1}/{2}", UserDataRoot, "ravi"/*Membership.GetUser().UserName*/, canonizedUrl);
        }

        public static string GetFullPath(string relativePath)
        {
            // Ignore HTTP URLs
            if (relativePath.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            {
                return relativePath;
            }

            // All other URLs must start with the LinkInksUrlPrefix
            if (!relativePath.StartsWith(LinkInksUrlPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new NotSupportedException(relativePath + " is not supported; only LinkInks:// URLs are supported");
            }

            string canonizedUrl = relativePath.Remove(0, LinkInksUrlPrefix.Length).Replace('/', '\\');
            return String.Format("{0}/{1}/{2}", HostingEnvironment.MapPath(UserDataRoot), "ravi"/*Membership.GetUser().UserName*/, canonizedUrl);
        }

        private static BookBlob DeserializePages(Guid bookId, string bookUri)
        {
            XmlDocument document = new XmlDocument();
            document.Load(bookUri);

            XmlNodeList bookNodes = document.GetElementsByTagName(BookTag);
            if ((bookNodes == null) || (bookNodes.Count != 1))
            {
                throw new BlobStoreException("Could not locate 'book' tag, or found more than one, in: " + bookUri);
            }

            XmlNodeList chapterNodes = bookNodes[0].ChildNodes;
            if ((chapterNodes == null) || (chapterNodes.Count == 0))
            {
                throw new BlobStoreException("Could not locate 'chapter' tags under 'book', in: " + bookUri);
            }

            BookBlob bookBlob = new BookBlob();
            bookBlob.ChapterBlobs = new Dictionary<int, ChapterBlob>();
            foreach (XmlNode chapterNode in chapterNodes)
            {
                XmlAttribute chapterIdAttribute = chapterNode.Attributes[ChapterIdAttributeName];
                if ((chapterIdAttribute == null) || (String.IsNullOrEmpty(chapterIdAttribute.Value)))
                {
                    throw new BlobStoreException("Could not find 'id' for chapter");
                }

                ChapterBlob chapterBlob     = new ChapterBlob();
                chapterBlob.ChapterPages    = new Dictionary<int, Page>();
                chapterBlob.PageCount       = chapterNode.ChildNodes.Count;

                foreach (XmlNode pageNode in chapterNode.ChildNodes)
                {
                    Page page = new Page(bookId, pageNode);
                    chapterBlob.ChapterPages.Add(page.PageNumber, page);
                }

                int chapterId = Int32.Parse(chapterIdAttribute.Value);
                bookBlob.ChapterBlobs.Add(chapterId, chapterBlob);
            }

            return bookBlob;
        }

        private static List<Chapter> DeserializeChapters(string bookUri, XmlNodeList chapterNodes)
        {
            List<Chapter> chapters      = new List<Chapter>();
            int chapterIndex            = 0;

            foreach (XmlNode chapterNode in chapterNodes)
            {
                Chapter chapter         = new Chapter();
                chapter.ChapterId       = ReadAttributeInt32(chapterNode, ChapterIdAttributeName);
                chapter.ContentLocation = bookUri;
                chapter.Index           = ++chapterIndex;
                chapter.Modules         = DeserializeModules(bookUri, chapterNode.ChildNodes);
                chapter.PageCount       = ReadAttributeInt32(chapterNode, PageCountAttributeName);
                chapter.Title           = ReadAttributeString(chapterNode, ChapterTitleAttributeName);

                chapters.Add(chapter);
            }

            return chapters;
        }

        private static List<Module> DeserializeModules(string bookUri, XmlNodeList pageNodes)
        {
            List<Module> modules        = new List<Module>();
            int moduleIndex             = 0;

            foreach (XmlNode pageNode in pageNodes)
            {
                XmlNodeList moduleNodes = pageNode.ChildNodes;
                foreach (XmlNode moduleNode in moduleNodes)
                {
                    Module module       = new Module();
                    module.Index        = ++moduleIndex;
                    module.ModuleId     = ReadAttributeGuid(moduleNode, ModuleIdAttributeName);
                    module.PageNumber   = ReadAttributeInt32(pageNode, PageNumberAttributeName);
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
        public Dictionary<int, ChapterBlob>         ChapterBlobs    { get; set; }
    }

    public class ChapterBlob
    {
        public Dictionary<int, Page>                ChapterPages    { get; set; }
        public int                                  PageCount       { get; set; }
    }
}