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
        }
    }
}