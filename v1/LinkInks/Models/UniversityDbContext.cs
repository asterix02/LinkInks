using System.Data.Entity;
using LinkInks.Models.Entities;

namespace LinkInks.Models
{
    public class UniversityDbContext : DbContext
    {
        public DbSet<UserState>         UserStates          { get; set; }

        public DbSet<Course>            Courses             { get; set; }
        public DbSet<Enrollment>        Enrollments         { get; set; }

        public DbSet<Book>              Books               { get; set; }
        public DbSet<Chapter>           Chapters            { get; set; }
        public DbSet<Module>            Modules             { get; set; }

        public DbSet<BookViewState>     BookViewStates      { get; set; }
        public DbSet<AnswerState>       AnswerStates        { get; set; }
        
        public DbSet<Discussion>        Discussions         { get; set; }
        public DbSet<DiscussionPost>    DiscussionPosts     { get; set; }

        public DbSet<Feedback>          Feedbacks           { get; set; }

        public UniversityDbContext() : base("DataServices")
        {
        }
    }
}