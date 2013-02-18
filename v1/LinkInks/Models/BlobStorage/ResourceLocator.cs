using System;
using System.Configuration;

namespace LinkInks.Models.BlobStorage
{
    public class ResourceLocator
    {
        public static String GetAbsoluteUri(String relativeUri)
        {
            // Input 1: http://linkinks.blob.core.windows.net/9fbc056d-fdb2-4073-93ec-dd163dbf172b/book.xml
            // Output:  http://linkinks.blob.core.windows.net/9fbc056d-fdb2-4073-93ec-dd163dbf172b/book.xml

            // Input 2: 9fbc056d-fdb2-4073-93ec-dd163dbf172b/book.xml
            // Output:  http://linkinks.blob.core.windows.net/9fbc056d-fdb2-4073-93ec-dd163dbf172b/book.xml

            if (Uri.IsWellFormedUriString(relativeUri, UriKind.Absolute))
            {
                return relativeUri;
            }
            else
            {
                String baseUri = ConfigurationManager.AppSettings["blobStoreBaseUri"];
                return baseUri + relativeUri;
            }
        }

        public static String GetAbsoluteUri(Guid bookId, String resourceName)
        {
            // Input 1: http://linkinks.blob.core.windows.net/9fbc056d-fdb2-4073-93ec-dd163dbf172b/book.xml
            // Output:  http://linkinks.blob.core.windows.net/9fbc056d-fdb2-4073-93ec-dd163dbf172b/book.xml

            // Input 2:   9fbc056d-fdb2-4073-93ec-dd163dbf172b, jumpstart_cover.jpg
            // Output:  http://linkinks.blob.core.windows.net/9fbc056d-fdb2-4073-93ec-dd163dbf172b/jumpstart_cover.jpg

            if (Uri.IsWellFormedUriString(resourceName, UriKind.Absolute))
            {
                return resourceName;
            }
            else
            {
                String baseUri = ConfigurationManager.AppSettings["blobStoreBaseUri"];
                String relativeUri = bookId.ToString().ToLower() + '/' + resourceName;

                return baseUri + relativeUri;
            }
        }

        public static Guid GetBookIdFromRelativeUri(String bookRelativeUri)
        {
            // Input:   9fbc056d-fdb2-4073-93ec-dd163dbf172b/Book.xml
            // Output:  9fbc056d-fdb2-4073-93ec-dd163dbf172b

            int indexOfSlash = bookRelativeUri.IndexOf('/');
            Guid bookId = Guid.Empty;

            if (Guid.TryParse(bookRelativeUri.Substring(0, indexOfSlash), out bookId) == false)
            {
                throw new BlobStoreException("Could not extract Book ID from relative URI: " + bookRelativeUri);
            }

            return bookId;
        }

        public static String GetRelativeUri(Uri resourceUri)
        {
            // Input 1: http://linkinks.blob.core.windows.net/9fbc056d-fdb2-4073-93ec-dd163dbf172b/book.xml
            // Output:  9fbc056d-fdb2-4073-93ec-dd163dbf172b/book.xml

            // Input 2: 9fbc056d-fdb2-4073-93ec-dd163dbf172b/book.xml
            // Output:  9fbc056d-fdb2-4073-93ec-dd163dbf172b/book.xml

            if (resourceUri.IsAbsoluteUri)
            {
                return resourceUri.AbsolutePath;
            }
            else
            {
                return resourceUri.ToString();
            }
        }
    }
}