using System;
using LinkInks.Models.Entities;

namespace LinkInks.Models.ChapterViewModels
{
    public class ModuleViewModel
    {
        public Guid     ModuleId    { get; set; }
        public Module   Module      { get; set; }
        public Content  Content     { get; set; }

        public ModuleViewModel(Module module, Content content)
        {
            this.ModuleId   = module.ModuleId;
            this.Module     = module;
            this.Content    = content;
        }
    }
}