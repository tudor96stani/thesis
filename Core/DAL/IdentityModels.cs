using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace Core.DAL
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public virtual ICollection<UsersBooks> Books { get; set; }
        public virtual ICollection<UsersBooks> BorrowedBooks { get; set; }
        public virtual ICollection<Friendship> MyFriends { get; set; }
        public virtual ICollection<Friendship> FriendsWithMe { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("ThesisDbConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<UsersBooks> UsersBooks { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<BookImage> Images { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UsersBooks>().HasKey(u => new
            {
                u.BookId,
                u.UserId
            });

            modelBuilder.Entity<Friendship>().HasKey(f => new
            {
                f.User1Id,
                f.User2Id
            });

            modelBuilder.Entity<Book>()
                        .HasOptional(b => b.BookImage)
                        .WithRequired(bi => bi.Book);




            modelBuilder.Entity<ApplicationUser>().HasMany(x => x.MyFriends).WithRequired(x => x.User1).HasForeignKey(x => x.User1Id);
            modelBuilder.Entity<Friendship>().HasRequired(x => x.User1).WithMany(x => x.MyFriends).HasForeignKey(x => x.User1Id);

            modelBuilder.Entity<ApplicationUser>().HasMany(x => x.FriendsWithMe).WithRequired(x => x.User2).HasForeignKey(x => x.User2Id);
            modelBuilder.Entity<Friendship>().HasRequired(x => x.User2).WithMany(x => x.FriendsWithMe).HasForeignKey(x => x.User2Id);


            modelBuilder.Entity<ApplicationUser>().HasMany(x => x.BorrowedBooks).WithOptional(x => x.Borrower).HasForeignKey(x => x.BorrowerId);
            modelBuilder.Entity<UsersBooks>().HasOptional(x => x.Borrower).WithMany(x => x.BorrowedBooks).HasForeignKey(x => x.BorrowerId);

            modelBuilder.Entity<ApplicationUser>().HasMany(x => x.Books).WithRequired(x => x.User).HasForeignKey(x => x.UserId);
            modelBuilder.Entity<UsersBooks>().HasRequired(x => x.User).WithMany(x => x.Books).HasForeignKey(x => x.UserId);

            modelBuilder.Entity<Book>().HasMany(x => x.Owners).WithRequired(x => x.Book).HasForeignKey(x => x.BookId);
            modelBuilder.Entity<UsersBooks>().HasRequired(x => x.Book).WithMany(x => x.Owners).HasForeignKey(x => x.BookId);
        }
    }
}