using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Security;
using LinkInks.Models.Entities;
using LinkInks.Models.BlobStorage;

namespace LinkInks.Models.ChapterViewModels
{
    public class DiscussionsViewModel
    {
        public Module                   Module                  { get; set; }
        public bool                     ShowDiscussions         { get; set; }
        public Guid                     LastUpdatedDiscussionId { get; set; }
        public ICollection<Discussion>  Discussions             { get; set; }

        public DiscussionsViewModel()
        {
            this.Module                     = null;
            this.ShowDiscussions            = false;
            this.LastUpdatedDiscussionId    = Guid.Empty;
        }

        public DiscussionsViewModel(UniversityDbContext db, Guid moduleId)
        {
            var module = db.Modules.Include(m => m.Discussions).SingleOrDefault(m => m.ModuleId == moduleId);
            if (module == null)
            {
                throw new ObjectNotFoundException("Could not locate module " + moduleId);
            }

            string currentUserName = Membership.GetUser().UserName;
            UserState userState = db.UserStates.SingleOrDefault(u => u.UserName == currentUserName);
            if (userState == null)
            {
                throw new ObjectNotFoundException("User state not initialized for current user");
            }

            this.Module                     = module;
            this.ShowDiscussions            = true;
            this.LastUpdatedDiscussionId    = userState.LastUpdatedDiscussionId;

            if (module.GetContentType() == ContentType.Question)
            {
                CreateDiscussionForQuestion(db, module);
                this.Discussions = module.Discussions;
            }
            else
            {
                this.Discussions = module.Discussions.OrderBy(d => (d.OwnerUserName != currentUserName))
                                                                    .ThenByDescending(d => d.LastModifiedTime)
                                                                    .ThenByDescending(d => d.Posts.Count).ToList();
            }
        }

        private void CreateDiscussionForQuestion(UniversityDbContext db, Module module)
        {
            string currentUserName = Membership.GetUser().UserName;

            // The view template depends on whether the student has answered this question
            AnswerState answerState = db.AnswerStates.SingleOrDefault(a => ((a.QuestionModuleId == module.ModuleId) &&
                                                                            (a.UserName == currentUserName)));

            // Start tracking the answerState if necessary
            if (answerState == null)
            {
                answerState                     = db.AnswerStates.Create();
                answerState.AnswerStateId       = Guid.NewGuid();
                answerState.Answered            = false;
                answerState.QuestionModuleId    = module.ModuleId;
                answerState.Score               = 0;
                answerState.UserName            = currentUserName;

                db.Entry(answerState).State     = EntityState.Added;
                db.SaveChanges();
            }

            // Create an in-memory, non-persisted discussion for this question module, showing
            // either the answer-choice template to the user, or the current answered state.
            if (answerState.Answered == true)
            {
                AddAnsweredDiscussion(db, module, currentUserName, answerState);
            }
            else
            {
                AddUnansweredDiscussion(db, module, currentUserName);
            }
        }

        private void AddAnsweredDiscussion(UniversityDbContext db, Module module, string currentUserName, AnswerState answerState)
        {
            QuestionContent questionContent = Store.Instance.GetModuleContentFromCache(db, module) as QuestionContent;
            if (questionContent == null)
            {
                throw new ObjectNotFoundException("Could not locate question module: " + module.ModuleId);
            }

            Discussion discussion           = new Discussion();
            discussion.CreationTime         = DateTime.Now;
            discussion.DiscussionId         = Guid.NewGuid();
            discussion.LastModifiedTime     = DateTime.Now;
            discussion.OwnerUserName        = currentUserName;
            discussion.Posts                = new List<DiscussionPost>();
            discussion.Title                = "Status: Answered";
            discussion.SetDiscussionType(Discussion.DiscussionType.QuizAnswer);

            DiscussionPost message          = new DiscussionPost();
            message.DiscussionPostId        = Guid.NewGuid();
            message.Content                 = (answerState.AnsweredCorrectly ? "Congratulations - you answered correctly!" : "Sorry, your answer was incorrect");
            message.CreationTime            = DateTime.Now;
            message.OwnerUserName           = currentUserName;

            DiscussionPost correctAnswer    = new DiscussionPost();
            correctAnswer.DiscussionPostId  = Guid.NewGuid();
            correctAnswer.Content           = questionContent.CorrectAnswer;
            correctAnswer.CreationTime      = DateTime.Now;
            correctAnswer.OwnerUserName     = currentUserName;

            DiscussionPost myAnswer         = new DiscussionPost();
            myAnswer.DiscussionPostId       = Guid.NewGuid();
            myAnswer.Content                = answerState.GivenAnswer;
            myAnswer.CreationTime           = DateTime.Now;
            myAnswer.OwnerUserName          = currentUserName;

            discussion.Posts.Add(message);
            discussion.Posts.Add(correctAnswer);
            discussion.Posts.Add(myAnswer);
            module.Discussions.Add(discussion);
        }

        private void AddUnansweredDiscussion(UniversityDbContext db, Module module, string currentUserName)
        {
            QuestionContent questionContent     = Store.Instance.GetModuleContentFromCache(db, module) as QuestionContent;
            if (questionContent == null)
            {
                throw new ObjectNotFoundException("Could not locate question module: " + module.ModuleId);
            }

            var answerChoices = questionContent.AnswerChoices;
            if (answerChoices != null)
            {
                Discussion discussion           = new Discussion();
                discussion.CreationTime         = DateTime.Now;
                discussion.DiscussionId         = Guid.NewGuid();
                discussion.LastModifiedTime     = DateTime.Now;
                discussion.OwnerUserName        = currentUserName;
                discussion.Posts                = new List<DiscussionPost>();
                discussion.Title                = "Answer Choices";
                discussion.SetDiscussionType(Discussion.DiscussionType.QuizAnswer);

                for (int i = 0; i < answerChoices.Count; i++)
                {
                    DiscussionPost post         = new DiscussionPost();
                    post.DiscussionPostId       = Guid.NewGuid();
                    post.Content                = answerChoices.ElementAt(i);
                    post.CreationTime           = DateTime.Now;
                    post.OwnerUserName          = currentUserName;

                    discussion.Posts.Add(post);
                }

                module.Discussions.Add(discussion);
            }
        }
    }
}