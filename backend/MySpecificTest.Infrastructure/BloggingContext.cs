using Microsoft.EntityFrameworkCore;

namespace MySpecificTest.Infrastructure
{
    public class BloggingContext : DbContext
    {
        public BloggingContext(DbContextOptions<BloggingContext> options) : base(options)
        {
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        // replace BloggingEFTest, if you want a different name for your database
        //protected override void OnConfiguring(DbContextOptionsBuilder options)
        //    => options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=BloggingEFSpecificTest;Trusted_Connection=True;MultipleActiveResultSets=true;");
    }
}