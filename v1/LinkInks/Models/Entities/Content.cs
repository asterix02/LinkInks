using System;
using System.Collections.Generic;
using LinkInks.Models.BlobStorage;

namespace LinkInks.Models.Entities
{
    public abstract class Content
    {
        public Guid ModuleId { get; set; }
        public abstract ContentType GetContentType();

        public static ContentType ParseContentType(string contentTypeString)
        {
            if (contentTypeString.Equals(Store.TextTag, StringComparison.InvariantCultureIgnoreCase))
            {
                return ContentType.Text;
            }
            else if (contentTypeString.Equals(Store.VideoTag, StringComparison.InvariantCultureIgnoreCase))
            {
                return ContentType.Video;
            }
            else if (contentTypeString.Equals(Store.PictureTag, StringComparison.InvariantCultureIgnoreCase))
            {
                return ContentType.Picture;
            }
            else if (contentTypeString.Equals(Store.LinkTag, StringComparison.InvariantCultureIgnoreCase))
            {
                return ContentType.Link;
            }
            else if (contentTypeString.Equals(Store.QuestionTag, StringComparison.InvariantCultureIgnoreCase))
            {
                return ContentType.Question;
            }
            else
            {
                return ContentType.Unknown;
            }
        }
    }

    public class TextContent : Content
    {
        public string Title { get; set; }
        public string Content { get; set; }

        public override ContentType GetContentType()
        {
            return ContentType.Text;
        }
    }

    public class VideoContent : Content
    {
        public string Location { get; set; }
        public string Caption { get; set; }

        public override ContentType GetContentType()
        {
            return ContentType.Video;
        }
    }

    public class PictureContent : Content
    {
        public string Location { get; set; }
        public string Caption { get; set; }

        public override ContentType GetContentType()
        {
            return ContentType.Picture;
        }
    }

    public class LinkContent : Content
    {
        public string Location { get; set; }
        public string Caption { get; set; }

        public override ContentType GetContentType()
        {
            return ContentType.Link;
        }
    }

    public class QuestionContent : Content
    {
        public string QuestionText { get; set; }
        public ICollection<string> AnswerChoices { get; set; }
        public string CorrectAnswer { get; set; }
        public int Points { get; set; }

        public override ContentType GetContentType()
        {
            return ContentType.Question;
        }
    }
}