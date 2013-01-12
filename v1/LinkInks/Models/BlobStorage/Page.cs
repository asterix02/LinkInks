using System;
using System.Collections.Generic;
using System.Xml;
using LinkInks.Models.Entities;

namespace LinkInks.Models.BlobStorage
{
    public class Page
    {
        public int                  PageNumber  { get; set; }
        public ICollection<Content> Contents    { get; set; }

        public Page(Guid bookId, XmlNode pageNode)
        {
            this.PageNumber = Store.ReadAttributeInt32(pageNode, Store.PageNumberAttributeName);
            this.Contents   = new List<Content>();

            foreach (XmlNode childNode in pageNode.ChildNodes)
            {
                Content content = null;
                switch (Content.ParseContentType(childNode.Name))
                {
                    case ContentType.Link:
                        content = CreateLinkContent(bookId, childNode);
                        break;

                    case ContentType.Picture:
                        content = CreatePictureContent(bookId, childNode);
                        break;

                    case ContentType.Question:
                        content = CreateQuestionContent(childNode);
                        break;

                    case ContentType.Text:
                        content = CreateTextContent(childNode);
                        break;

                    case ContentType.Video:
                        content = CreateVideoContent(bookId, childNode);
                        break;

                    case ContentType.Unknown:
                        throw new BlobStoreException("Unknown content type: " + childNode.Name);
                }

                if (content != null)
                {
                    content.ModuleId = Store.ReadAttributeGuid(childNode, Store.ModuleIdAttributeName);
                    this.Contents.Add(content);
                    Store.Instance.AddModuleContent(content.ModuleId, content);
                }
            }
        }

        private Content CreateQuestionContent(XmlNode node)
        {
            QuestionContent content = new QuestionContent();
            content.AnswerChoices   = new List<string>();

            if (node.Attributes[Store.QuestionPointsAttributeName] != null)
            {
                content.Points = Store.ReadAttributeInt32(node, Store.QuestionPointsAttributeName);
            }

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name.Equals(Store.QuestionTextTag, StringComparison.InvariantCultureIgnoreCase))
                {
                    content.QuestionText = childNode.InnerXml;
                }
                else if (childNode.Name.Equals(Store.AnswerChoiceTag, StringComparison.InvariantCultureIgnoreCase))
                {
                    content.AnswerChoices.Add(childNode.InnerText.Trim());
                }
                else if (childNode.Name.Equals(Store.CorrectAnswerTag, StringComparison.InvariantCultureIgnoreCase))
                {
                    content.CorrectAnswer = childNode.InnerText.Trim();
                }
            }

            return content;
        }

        private Content CreateLinkContent(Guid bookId, XmlNode node)
        {
            LinkContent content     = new LinkContent();
            content.Location        = Store.GetAbsoluteUri(bookId, Store.ReadAttributeString(node, Store.LinkSourceAttributeName));
            content.Caption         = node.InnerText;

            return content;
        }

        private Content CreatePictureContent(Guid bookId, XmlNode node)
        {
            PictureContent content  = new PictureContent();
            content.Location        = Store.GetAbsoluteUri(bookId, Store.ReadAttributeString(node, Store.PictureSourceAttributeName));
            content.Caption         = node.InnerText;

            return content;
        }

        private Content CreateVideoContent(Guid bookId, XmlNode node)
        {
            VideoContent content    = new VideoContent();
            content.Location        = Store.GetAbsoluteUri(bookId, Store.ReadAttributeString(node, Store.VideoSourceAttributeName));
            content.Caption         = node.InnerText;

            return content;
        }

        private Content CreateTextContent(XmlNode node)
        {
            TextContent content     = new TextContent();
            content.Title           = Store.ReadAttributeString(node, Store.TextTitleAttributeName);
            content.Content         = node.InnerXml;

            return content;
        }
    }
}