using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MySpecificTest.Infrastructure.IntegrationTests
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0#customize-webapplicationfactory
    /// </summary>
    /// <typeparam name="TStartup"></typeparam>
    public class CustomWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BloggingContext>));

                services.Remove(descriptor);

                var connection = new SqliteConnection("Filename=:memory:"); // sqlite needs an open connections
                connection.Open();

                services.AddDbContext<BloggingContext>(options =>
                {
                    //options.UseInMemoryDatabase("InMemoryDbForTesting");
                    options.UseSqlite(connection);
                });

                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<BloggingContext>();
                    var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

                    db.Database.EnsureCreated(); // this will create the database if it does not exist

                    try
                    {
                        //Utilities.InitializeDbForTests(db);

                        db.Add(new Blog { Url="my.test.blog",BlogId=-1 });
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"An error occurred seeding the database with test messages. Error: {ex.Message}");
                    }
                }
            });
        }
    }
}