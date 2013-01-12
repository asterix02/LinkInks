using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using LinkInks.Models.Entities;
using System.Web.Hosting;

namespace LinkInks.Models
{
    public class UniversityDbInitializer : DropCreateDatabaseIfModelChanges<UniversityDbContext>
    {
        protected override void Seed(UniversityDbContext context)
        {
            // Initialize the users. We have preconfigured ASP.NET to create five students (and two instructors), but
            // they do not have their corresponding UserState data yet.
            //InitializeUsers(context);

            //// We assume that the users and roles (students and instructors) have already 
            //// been set up using the ASP.NET Configuration tool
            //_defaultBookId = CreateDefaultTextbook(context);

            //CreateDefaultCourses(context);
            //CreateDefaultEnrollments(context);

            //// Add a few conversations to show off
            //CreateDefaultDiscussions("This is awesome!",                _studentUserNames[0], _studentUserNames[1], _defaultModuleId, context);
            //CreateDefaultDiscussions("Anyone working on HW #3?",        _studentUserNames[1], _studentUserNames[3], _defaultModuleId, context);
            //CreateDefaultDiscussions("I have a question on this...",    _studentUserNames[2], _studentUserNames[4], _defaultModuleId, context);
            //CreateDefaultDiscussions("How do I start a new thread?",    _studentUserNames[4], _studentUserNames[2], _defaultModuleId, context);
        }

        private static void InitializeUsers(UniversityDbContext context)
        {
            foreach (var userName in _studentUserNames)
            {
                var userState = context.UserStates.Create();
                userState.Initialize(userName);

                context.Entry(userState).State = EntityState.Added;
            }

            foreach (var userName in _instructorUserNames)
            {
                var userState = context.UserStates.Create();
                userState.Initialize(userName);

                context.Entry(userState).State = EntityState.Added;
            }

            context.SaveChanges();
        }

        private static void CreateDefaultCourses(UniversityDbContext context)
        {
            // Conventions:
            //
            // 1. All courses have the same book, denoted by _defaultBookId.
            // 2. The _coursesToAdd.InstructorID field represents the index into _instructorUserNames. 
            //    The actual instructorID will be pulled from SQL at runtime.
            // 3. InstructorIds have been populated before this function is called.
            // 4. Enrollments for this course will be added separately, when a student is added to each course.
            //
            Book defaultBook = context.Books.Find(_defaultBookId);
            foreach (var courseToAdd in _coursesToAdd)
            {
                Course newCourse                = context.Courses.Create();
                newCourse.BookId                = _defaultBookId;
                newCourse.Book                  = defaultBook;
                newCourse.Credits               = courseToAdd.Credits;
                newCourse.Enrollments           = new List<Enrollment>();
                newCourse.InstructorUserName    = courseToAdd.InstructorUserName;
                newCourse.Title                 = courseToAdd.Title;

                context.Entry(newCourse).State  = EntityState.Added;
            }
            context.SaveChanges();
        }

        private static void CreateDefaultEnrollments(UniversityDbContext context)
        {
            List<Course> courses = context.Courses.ToList();
            foreach (var course in courses)
            {
                foreach (var studentUserName in _studentUserNames)
                {
                    Enrollment enrollment           = context.Enrollments.Create();
                    enrollment.EnrollmentId         = Guid.NewGuid();
                    enrollment.CourseId             = course.CourseId;
                    enrollment.StudentUserName      = studentUserName;
                    enrollment.IsPending            = !course.Title.Equals("Getting Started");

                    course.Enrollments.Add(enrollment);

                    context.Entry(enrollment).State = EntityState.Added;
                    context.Entry(course).State     = EntityState.Modified;
                }
            }
            context.SaveChanges();
        }

        private static Guid CreateDefaultTextbook(UniversityDbContext context)
        {
            return CourseViewModels.ControllerHelper.CreateBook(context, _defaultBookFileName);
        }

        private static void CreateDefaultDiscussions(string title, string userName1, string userName2, Guid moduleId, UniversityDbContext context)
        {
            // Find the specified module
            Module module = context.Modules.SingleOrDefault(m => m.ModuleId == moduleId);
            if (module == null)
            {
                throw new ObjectNotFoundException();
            }

            // Add a new discussion
            Discussion discussion           = context.Discussions.Create();
            discussion.DiscussionId         = Guid.NewGuid();
            discussion.CreationTime         = DateTime.Now;
            discussion.LastModifiedTime     = DateTime.Now;
            discussion.Likes                = 1;
            discussion.OwnerUserName        = userName1;
            discussion.Posts                = new List<DiscussionPost>();
            discussion.Title                = title;
            discussion.SetDiscussionType(Discussion.DiscussionType.Conversation);

            module.Discussions.Add(discussion);
            context.Entry(discussion).State = EntityState.Added;
            context.Entry(module).State     = EntityState.Modified;
            context.SaveChanges();

            // Add a few posts to that discussion
            DiscussionPost post1 = context.DiscussionPosts.Create();
            post1.DiscussionPostId          = Guid.NewGuid();
            post1.Content                   = "Yeah, I wish I had seen this earlier!";
            post1.CreationTime              = DateTime.Now;
            post1.Likes                     = 0;
            post1.OwnerUserName             = userName2;
            discussion.Posts.Add(post1);
            context.Entry(discussion).State = EntityState.Modified;
            context.Entry(post1).State      = EntityState.Added;

            DiscussionPost post2 = context.DiscussionPosts.Create();
            post2.DiscussionPostId          = Guid.NewGuid();
            post2.Content                   = "We should ping the rest of our classmates to try this out.";
            post2.CreationTime              = DateTime.Now;
            post2.Likes                     = 0;
            post2.OwnerUserName             = userName1;
            discussion.Posts.Add(post2);
            context.Entry(discussion).State = EntityState.Modified;
            context.Entry(post2).State      = EntityState.Added;
            context.SaveChanges();

            DiscussionPost post3 = context.DiscussionPosts.Create();
            post3.DiscussionPostId          = Guid.NewGuid();
            post3.Content                   = "Totally!";
            post3.CreationTime              = DateTime.Now;
            post3.Likes                     = 0;
            post3.OwnerUserName             = userName2;
            discussion.Posts.Add(post3);
            context.Entry(discussion).State = EntityState.Modified;
            context.Entry(post3).State      = EntityState.Added;
            context.SaveChanges();
        }

        private static List<Course> _coursesToAdd = new List<Course>
        {
            new Course { Title = "Getting Started",     Credits = 0,    InstructorUserName = "Ravi" },
            new Course { Title = "Business Networks",   Credits = 6,    InstructorUserName = "Ravi" },
            new Course { Title = "Commercialization",   Credits = 6,    InstructorUserName = "Boni" },
        };

        // We assume that these users and roles (students and instructors) have already 
        // been set up using the ASP.NET Configuration tool
        private static List<string> _studentUserNames = new List<string> 
        { 
            "Alice", 
            "Bob", 
            "Charlie", 
            "Doug", 
            "Erica" 
        };

        // We assume that these users and roles (students and instructors) have already 
        // been set up using the ASP.NET Configuration tool
        private static List<string> _instructorUserNames = new List<string> 
        { 
            "Ravi",
            "Boni", 
        };

        private static Guid     _defaultBookId;
        private static Guid     _defaultModuleId = Guid.Parse("{2F03672D-BFA1-4252-8917-94833D0F5485}"); // or, {0D3A4DA6-A754-48D3-B640-9AAE71FFA86D}
        //private static string   _defaultBookFileName = "/User_Data/ravi/Getting Started/Book.xml";
        private static string _defaultBookFileName = "LinkInks://Getting Started/Book.xml";
    }
}