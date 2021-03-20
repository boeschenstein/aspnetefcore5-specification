using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySpecificTest.Infrastructure;
using MySpecificTest.Infrastructure.MediatR;

namespace MySpecificTest.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public partial class BlogController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly BloggingContext db; // not needed when using MediatR
        private readonly IMediator mediator;

        public BlogController(
            ILogger<WeatherForecastController> logger,
            BloggingContext bloggingContext,
            IMediator mediator)
        {
            _logger = logger;
            db = bloggingContext;
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task<IEnumerable<Blog>> Post(UrlRequestDto myUrl)
        {
            _logger.LogInformation("Getting Blogs");

            IEnumerable<Blog> blogs = await mediator.Send(new BlogWithItemsRequest(myUrl.Url));
            return blogs;
        }

        [HttpPut]
        public async Task<IEnumerable<Blog>> Put(UrlRequestDto myUrl)
        {
            _logger.LogInformation("Getting Blogs");

            IEnumerable<Blog> blogs = await mediator.Send(new BlogWithItemsRequest(myUrl.Url));
            var ret = blogs.ToList();
            return ret;
        }

        [HttpGet]
        public async Task<IEnumerable<Blog>> SpecificationAndMediatRTester()
        {
            _logger.LogInformation("SpecificationAndMediatRTester");

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

            IEnumerable<Blog> blogs = await mediator.Send(new BlogWithItemsRequest("https://devblogs.microsoft.com/dotnet"));
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

            return blogs;
        }
    }
}