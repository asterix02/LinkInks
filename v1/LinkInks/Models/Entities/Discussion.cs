using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinkInks.Models.Entities
{
    public class Discussion
    {
        [Key, Required]
        public Guid     DiscussionId                        { get; set;             }

        [Required]
        public string   OwnerUserName                       { get; set;             }

        [Required]
        public int      DiscussionTypeInternal              { get; protected set;   }

        [Required]
        public string   Title                               { get; set;             }
        public int      Likes                               { get; set;             }

        [Required]
        public DateTime CreationTime                        { get; set;             }

        [Required]
        public DateTime LastModifiedTime                    { get; set;             }

        [Required]
        public virtual  ICollection<DiscussionPost> Posts   { get; set;             }

        public enum DiscussionType
        {
            Undefined    = 1,
            Conversation = 2,
            QuizAnswer   = 3
        }

        public DiscussionType GetDiscussionType()
        {
            return ((DiscussionType) this.DiscussionTypeInternal);
        }

        public void SetDiscussionType(DiscussionType type)
        {
            this.DiscussionTypeInternal = (int) type;
        }
    }
}