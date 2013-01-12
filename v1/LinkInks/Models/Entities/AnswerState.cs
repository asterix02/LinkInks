using System;
using System.ComponentModel.DataAnnotations;

namespace LinkInks.Models.Entities
{
    public class AnswerState
    {
        [Key, Required]
        public Guid     AnswerStateId       { get; set; }
        
        [Required]
        public string   UserName            { get; set; }

        [Required]
        public Guid     QuestionModuleId    { get; set; }

        [Required]
        public bool     Answered            { get; set; }

        public string   GivenAnswer         { get; set; }

        public bool     AnsweredCorrectly   { get; set; }
        
        public int      Score               { get; set; }
    }
}