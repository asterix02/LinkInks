using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinkInks.Models.Entities
{
    /// <summary>
    /// This class represents the smallest unit of content within any textbook.
    /// 
    /// Content can take many forms, such as text, pictures, videos, questions,
    /// links etc. Ideally, this class should be general enough to model any content,
    /// while being specific enough to be schematized and persisted in the SQL database. 
    /// However, the Entity Framework does not support concepts like inheritance to model
    /// such an abstraction. 
    /// 
    /// Our approach is to store just the basic properties of every module in the 
    /// SQL database, while deferring the specific content to be mapped in a way that's
    /// appropriate for the content. For instance, the Module instance will contain the
    /// location of a video, but the video content will be stored as a blob on the local
    /// filesystem. However, for programming ease, we expose helper classes that are
    /// specific to the content type, which can be obtained by typecasting the Module
    /// instance.
    /// 
    /// Specifically, these are the mappings for the different types of content:
    /// 
    /// Text
    ///     ContentLocation     = Xml file location
    ///     
    /// Video 
    ///     ContentLocation     = Video file location
    ///     
    /// Link
    ///     ContentLocation     = Link target location
    ///     
    /// Question
    ///     ContentLocation     = Xml file location
    ///     
    /// Picture
    ///     ContentLocation     = Picture file location
    ///     
    /// </summary>
    public class Module
    {
        [Key, Required]
        public Guid     ModuleId            { get; set; }

        [Required]
        public int      Index               { get; set; }

        [ForeignKey("Book")]
        public Guid     BookId              { get; set; }
        public virtual Book Book            { get; set; }

        /// <summary>
        /// Please use GetContentType() and SetContentType() to access the content type
        /// instead of directly using this field.
        /// </summary>
        [Required]
        public int      ContentTypeInternal { get; set; }

        [Required]
        public string   ContentLocation     { get; set; }

        [Required]
        public int      PageNumber          { get; set; }

        [Required]
        public virtual ICollection<Discussion> Discussions { get; set; }

        public ContentType GetContentType()
        {
            return (Entities.ContentType) this.ContentTypeInternal;
        }

        public void SetContentType(Entities.ContentType contentType)
        {
            this.ContentTypeInternal = (int)contentType;
        }
    }

    public enum ContentType
    {
        Unknown     = 0,
        Text        = 1,
        Video       = 2,
        Link        = 3,
        Question    = 4,
        Picture     = 5
    }
}