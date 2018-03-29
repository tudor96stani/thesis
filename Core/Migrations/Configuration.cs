namespace Core.Migrations
{
    using Core.DAL;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Core.DAL.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Core.DAL.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            var authors = new List<Author>()
            {
                new Author(){Id=Guid.NewGuid(),FullName="Kazuo Ishiguro"},
                new Author(){Id=Guid.NewGuid(),FullName="Aldous Huxley"},
                new Author(){Id=Guid.NewGuid(),FullName="Fyodor Dostoyevsky"}
            };

            var books = new List<Book>()
            {
                new Book(){Id=Guid.NewGuid(),Title="Never let me go",Year=2005,Publisher="Vintage",Authors = new List<Author>(){ authors[0]} },
                new Book(){Id=Guid.NewGuid(),Title="Brave new world",Year=1932,Publisher="Harper",Authors = new List<Author>(){ authors[1]}},
                new Book(){Id=Guid.NewGuid(),Title="The Idiot",Year=1868,Publisher="Modern Library",Authors = new List<Author>(){ authors[2]}}
            };

            authors[0].Books.Add(books[0]);
            authors[1].Books.Add(books[1]);
            authors[2].Books.Add(books[2]);

            context.Authors.AddOrUpdate(a => a.FullName, authors.ToArray());
            context.Books.AddOrUpdate(b => b.Title, books.ToArray());
        }
    }
}
