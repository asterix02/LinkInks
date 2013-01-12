using System;
using System.ComponentModel.DataAnnotations;

namespace LinkInks.Models.Entities
{
    public class DiscussionPost
    {
        [Key, Required]
        public Guid     DiscussionPostId    { get; set; }

        [Required]
        public string   OwnerUserName       { get; set; }

        [Required]
        public string   Content             { get; set; }

        [Required]
        public int      Likes               { get; set; }

        [Required]
        public DateTime CreationTime        { get; set; }
    }
}