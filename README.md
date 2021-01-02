# ASP.NET Core 5 + EF

## Main Goals

- Specification pattern
- MediatR
- Integration tests

## Content

- [ASP.NET Core 5 + EF](#aspnet-core-5--ef)
  - [Main Goals](#main-goals)
  - [Content](#content)
  - [Add EF](#add-ef)
  - [Specification Pattern](#specification-pattern)
    - [ISpecification\<T>](#ispecificationt)
    - [BaseSpecification\<T> : ISpecification\<T>](#basespecificationt--ispecificationt)
    - [Example Specification](#example-specification)
    - [Generic Repository](#generic-repository)
  - [Move ConnectionString to appSettings.json](#move-connectionstring-to-appsettingsjson)
  - [Inject Generic Repository](#inject-generic-repository)
  - [MediatR](#mediatr)
  - [Integration Tests](#integration-tests)
    - [Execute WebApi Controller Endpoint](#execute-webapi-controller-endpoint)
    - [EF Core Unit testing](#ef-core-unit-testing)
    - [Approach: InMemoryDB](#approach-inmemorydb)
  - [Information](#information)

## Add EF

Example: <https://github.com/boeschenstein/angular9-dotnetcore-ef-sql>

Add Debugger to Get of WeatherForecastController

Start UI (Swagger)

## Specification Pattern

Source: <https://deviq.com/specification-pattern/>

### ISpecification\<T>
  
```cs
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
}
```

### BaseSpecification\<T> : ISpecification\<T>

```cs
public abstract class BaseSpecification<T> : ISpecification<T>
{
    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    public Expression<Func<T, bool>> Criteria { get; }
    public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();

    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }
}
```

### Example Specification

```cs
public class BasketWithItemsSpecification : BaseSpecification<Basket>
{
    public BasketWithItemsSpecification(int basketId)
        : base(b => b.Id == basketId)
    {
        AddInclude(b => b.Items);
    }
 
```

### Generic Repository

```cs
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly DbContext _dbContext;

    public GenericRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEnumerable<T> List(ISpecification<T> spec)
    {
        IQueryable<T> query = _dbContext.Set<T>().AsQueryable();

        // fetch a Queryable that includes all expression-based includes
        var queryableResultWithIncludes = spec.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // return the result of the query using the specification's criteria expression
        return queryableResultWithIncludes
                .Where(spec.Criteria)
                .AsEnumerable();
    }
}
```

## Move ConnectionString to appSettings.json

Add ConnectionString to appSettings:

```json
"ConnectionStrings": {
  "BloggingConnection": "Server=(localdb)\\mssqllocaldb;Database=BloggingEFSpecificTest;Trusted_Connection=True;MultipleActiveResultSets=true;"
},
```

Use ConnectionString from appSettings:

```cs
services.AddDbContext<BloggingContext>(c => c.UseSqlServer(Configuration.GetConnectionString("BloggingConnection")));
```

Make use of change: replace OnConfiguring with this:

```cs
public BloggingContext(DbContextOptions<BloggingContext> options) : base(options)
{ }
```

Inject Context where needed (Controller is not a good example, please use a repository):

```cs
public WeatherForecastController(
    BloggingContext bloggingContext
)
{
    _bloggingContext = bloggingContext;
}
```

## Inject Generic Repository

Define Generic Injection:

```cs
services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
```

Inject:

```cs
public WeatherForecastController(
    IGenericRepository<Blog> repository)
{
    _repository = repository;
}
```

## MediatR

Add MediatR to WebApi and assembly, where the requests/responses are implemented:

`Install-Package MediatR`

Add MediatR for ASP.NET Core:

`Install-Package MediatR.Extensions.Microsoft.DependencyInjection`

and activate it like this:

```cs
services.AddMediatR(typeof(BlogWithItemsRequest)); // Assembly
```

Create the first Request/response:

```cs
public class BlogWithItemsRequest : IRequest<IEnumerable<Blog>>
{
    public string Url { get; private set; }

    public BlogWithItemsRequest(string url)
    {
        Url = url;
    }
}

public class BlogWithItemsRequestHandler : IRequestHandler<BlogWithItemsRequest, IEnumerable<Blog>>
{
    private readonly IGenericRepository<Blog> _repository;

    public BlogWithItemsRequestHandler(IGenericRepository<Blog> repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Blog>> Handle(BlogWithItemsRequest request, CancellationToken cancellationToken)
    {
        IEnumerable<Blog> blogs = _repository.List(new BlogWithItemsSpecification(request.Url));
        return Task.FromResult(blogs);
    }
}
```

## Integration Tests

|            | Unit Test                                               | Integration Test                                                             |
| ---------- | ------------------------------------------------------- | ---------------------------------------------------------------------------- |
| technique  | use mocks, fakes                                        | use actual components                                                        |
| complexity | simple test                                             | require more code                                                            |
| speed      | fast                                                    | slow                                                                         |
| goal       | test every aspect/edge case<br>of an algorithm          | text generic access (like crud) for<br>each technology (file, database, ...) |
| scope      | should only test code within<br>the developer's control | often do include<br>infrastructure concerns                                  |

Add new solution for Unit testing:

```cmd
dotnet new xunit -o MySpecificTest.Infrastructure.Tests
dotnet sln add .\MySpecificTest.Infrastructure.Tests
```

Add new solution for Integration testing:

```cmd
dotnet new xunit -o MySpecificTest.Infrastructure.IntegrationTests
dotnet sln add .\MySpecificTest.Infrastructure.IntegrationTests
```

In csproj file, change the first line \
from `<Project Sdk="Microsoft.NET.Sdk">`\
to `<Project Sdk="Microsoft.NET.Sdk.Web">`

and add this package:

```ps
install-package Microsoft.AspNetCore.Mvc.Testing
```

### Execute WebApi Controller Endpoint

This code will run your Endpoint like am manual call in Swagger (services like Repo gets injected) and

> Database statements gets executed !!

```cs
public class BasicTests
    : IClassFixture<WebApplicationFactory<MySpecificTest.WebApi.Startup>>
{
    private readonly WebApplicationFactory<MySpecificTest.WebApi.Startup> _factory;

    public BasicTests(WebApplicationFactory<MySpecificTest.WebApi.Startup> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/WeatherForecast")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
    }
}
```

### EF Core Unit testing

<https://docs.microsoft.com/en-us/ef/core/testing/>

Preparation: Add this to your integration test project:

```ps
install-package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore
install-package Microsoft.AspNetCore.Identity.EntityFrameworkCore
install-package Microsoft.EntityFrameworkCore
install-package Microsoft.EntityFrameworkCore.InMemory
install-package Microsoft.EntityFrameworkCore.Tools
```

### Approach: InMemoryDB

<https://docs.microsoft.com/en-us/ef/core/testing/#approach-3-the-ef-core-in-memory-database>

To avoid execution in the real database, you can use InMemoryDB from CustomWebApplicationFactory instead of WebApplicationFactory:

<details>
  <summary>Show me the code!</summary>

```cs
// https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0#customize-webapplicationfactory
public class CustomWebApplicationFactory<TStartup>
    : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BloggingContext>));

            services.Remove(descriptor);

            services.AddDbContext<BloggingContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            var sp = services.BuildServiceProvider();

            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<BloggingContext>();
                var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

                db.Database.EnsureCreated(); // this will create the database (using your DbContext) if it does not exist

                try
                {
                    //Utilities.InitializeDbForTests(db);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"An error occurred seeding the database with test messages. Error: {ex.Message}");
                }
            }
        });
    }
}

```

</details>

The most important line is this:

```cs
db.Database.EnsureCreated(); // this will create the database (using your DbContext) if it does not exist
```

## Information

- EF Core Basics: <https://github.com/boeschenstein/angular9-dotnetcore-ef-sql>
- MediatR Wiki: <https://github.com/jbogard/MediatR/wiki>
- Full application (ASP.NET Core, EF Core, MeditR, Specification Pattern): <https://github.com/dotnet-architecture/eShopOnWeb>
- Testing
  - Unit testing <https://docs.microsoft.com/en-us/dotnet/core/testing/>
  - Integration Testing <https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests>
  - EF Core Testing: <https://docs.microsoft.com/en-us/ef/core/testing/>
