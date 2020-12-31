using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySpecificTest.Infrastructure;
using MySpecificTest.Infrastructure.SpecificationPattern;

namespace MySpecificTest.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly BloggingContext db;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, BloggingContext bloggingContext)
        {
            _logger = logger;
            db = bloggingContext;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            // Create
            Console.WriteLine("Inserting a new blog");
            db.Add(new Blog { Url = "http://blogs.msdn.com/adonet" });
            db.SaveChanges();

            // Read
            Console.WriteLine("Querying for a blog");
            var blog = db.Blogs
                .OrderBy(b => b.BlogId)
                .First();

            // Update
            Console.WriteLine("Updating the blog and adding a post");
            blog.Url = "https://devblogs.microsoft.com/dotnet";
            blog.Posts.Add(
                new Post
                {
                    Title = "Hello World",
                    Content = "I wrote an app using EF Core!"
                });
            db.SaveChanges();

            // Query by Specification Repository
            GenericRepository<Blog> repo = new GenericRepository<Blog>(db); // todo: inject this
            IEnumerable<Blog> blogs = repo.List(new BlogWithItemsSpecification("https://devblogs.microsoft.com/dotnet"));
            foreach (var item in blogs)
            {
                Console.WriteLine($"== BLOG: {item.BlogId}, {item.Url}");
                foreach (var post in item.Posts)
                {
                    Console.WriteLine($"    {post.Title}");
                }
            }

            // Delete
            Console.WriteLine("Delete the blog");
            db.Remove(blog);
            db.SaveChanges();

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}