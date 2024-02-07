# ASP.NET Core 5 + EF

## Main Goals

- Specification pattern
- MediatR
- Integration tests
- Json (System.Text.Json vs. NewtonSoft)

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
      - [Option a) Execute controller from outside](#option-a-execute-controller-from-outside)
      - [Option b) Call new Controller](#option-b-call-new-controller)
    - [EF Core Unit testing](#ef-core-unit-testing)
    - [Approach: InMemoryDB](#approach-inmemorydb)
    - [Approach: SQLite](#approach-sqlite)
  - [Unit Test Helpers](#unit-test-helpers)
    - [FluentAssertions](#fluentassertions)
    - [Moq](#moq)
    - [AutoMoqer/AutoMoqCore](#automoqerautomoqcore)
    - [Bogus, AutoBogus](#bogus-autobogus)
    - [AutoBogus / AutoFaker](#autobogus--autofaker)
    - [AutoFixture](#autofixture)
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

Define Open Generic Injection:

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

This code will run your Endpoint like a manual call in Swagger (services like Repo gets injected) and

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

>Use a CustomWebApplicationFactory instead of WebApplicationFactory (inherit form WebApplicationFactory) use a complete new set of config (aka program.cs/startup.cs)

```cs
    public class BasicTests
        : IClassFixture<CustomWebApplicationFactory<MySpecificTest.WebApi.Startup>>
    { // the whole code will follow
```

You can find more examples here: <https://github.com/dotnet-architecture/eShopOnWeb>

Mock a Service: <https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0#inject-mock-services>

#### Option a) Execute controller from outside

To execute the controller from outside - like the example before - is the broadest test scenario. Another way to test the controller, can be this:

#### Option b) Call new Controller

<https://docs.microsoft.com/en-us/ef/core/testing/testing-sample#test-structure>

<details>
  <summary>Show me the code!</summary>

```cs
// from https://github.com/dotnet-architecture/eShopOnWeb
public class SetQuantities
{
    private readonly CatalogContext _catalogContext;
    private readonly IAsyncRepository<Basket> _basketRepository;
    private readonly BasketBuilder BasketBuilder = new BasketBuilder();

    public SetQuantities()
    {
        var dbOptions = new DbContextOptionsBuilder<CatalogContext>()
            .UseInMemoryDatabase(databaseName: "TestCatalog")
            .Options;
        _catalogContext = new CatalogContext(dbOptions);
        _basketRepository = new EfRepository<Basket>(_catalogContext);
    }

    [Fact]
    public async Task RemoveEmptyQuantities()
    {
        var basket = BasketBuilder.WithOneBasketItem();
        var basketService = new BasketService(_basketRepository, null);
        await _basketRepository.AddAsync(basket);
        _catalogContext.SaveChanges();

        await basketService.SetQuantities(BasketBuilder.BasketId, new Dictionary<string, int>() { { BasketBuilder.BasketId.ToString(), 0 } });

        Assert.Equal(0, basket.Items.Count);
    }
}
```

</details>

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

Restrictions compared to SQL Server:

- case-sensitive
- It is not a relational database.
  - no referential integrity
  - no cascade delete
- It doesn't support transactions.
- It cannot run raw SQL queries.
- It is not optimized for performance.

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
                    // todo: add a basic set of entities for testing purposes
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

### Approach: SQLite

<https://docs.microsoft.com/en-us/ef/core/testing/#approach-2-sqlite>

This is a better option than InMemoryDB, but there are still some restrictions compared to SQL Server:

- SQLite inevitability doesn't support everything that your production database system does.
- SQLite will behave differently than your production database system for some queries.
  - case-sensitive
  - no DateTimeOffset: use workaround

```cs
dotnet add package Microsoft.Data.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

Spot the difference: SQLite needs an open connection:

```cs
var connection = new SqliteConnection("Filename=:memory:"); // sqlite needs an open connections
connection.Open();

services.AddDbContext<BloggingContext>(options =>
{
    //options.UseInMemoryDatabase("InMemoryDbForTesting");
    options.UseSqlite(connection);
});
```

## Unit Test Helpers

### FluentAssertions

A very extensive set of extension methods that allow you to more naturally specify the expected outcome of a TDD or BDD-style unit tests.

`install-package FluentAssertions`

Examples:

```cs
blog.BlogId.Should().Be(-1);
blog.Url.Should().StartWith("my").And.EndWith("blog").And.Contain("test").And.HaveLength(12);

blog.Should().BeEquivalentTo(new Blog { BlogId = -1, Url = "my.test.blog" });
```

Custom Assertion: check code. Source: <<https://www.youtube.com/watch?v=WybRJ_LKGb>

### Moq

> Moq: The most popular and friendly mocking framework for .NET

```cs
var mock = new Mock<ILoveThisLibrary>();

// WOW! No record/replay weirdness?! :)
mock.Setup(library => library.DownloadExists("2.0.0.0"))
    .Returns(true);

// Use the Object property on the mock to get a reference to the object
// implementing ILoveThisLibrary, and then exercise it by calling methods on it
ILoveThisLibrary lovable = mock.Object;
bool download = lovable.DownloadExists("2.0.0.0");

// Verify that the given method was indeed called with the expected value at most once
mock.Verify(library => library.DownloadExists("2.0.0.0"), Times.AtMostOnce());
```

Moq ILogger: <https://stackoverflow.com/questions/58283208/how-to-mock-ilogger-logxxx-methods-on-netcore-3-0>

```cs
container.GetMock<ILogger<MyClass>>()
         .Setup(l => l.Log(
             It.IsAny<LogLevel>(),
             It.IsAny<EventId>(),
             It.IsAny<It.IsAnyType>(),
             It.IsAny<Exception>(),
             (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>())));
```

If you get the error `System.ArgumentException : Can not create proxy for type Microsoft.Extensions.Logging.ILogger ...`, because assembly Microsoft.Extensions.Logging.Abstractions is strong-named. (Parameter 'interfaceToProxy')`, READ THE ERROR DESC! and add the following line somewhere to your code: 
```
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
```

Detail: see next: AutoMoq... (AutoMoqer/AutoMoqCore)

### AutoMoqer/AutoMoqCore

> AutoMoqer is an "auto-mocking" container that creates objects for you. Just tell it what class to create and it will create it.

`install-package AutoMoqCore`

> Alternative: see AutoFixture

Examples:

```cs
var mocker = new AutoMoqCore.AutoMoqer();

var myArgs = new MyArgs("myArg1");

mocker.GetMock<IDataDependency>()
   .Setup(x => x.GetDataArgs(myArgs))
   .Returns("TEST DATA");

// not to use new() is an advantage:
// If constructor of ClassToTest gets more arguments, the following line does not need a change:
var classToTest = mocker.Resolve<ClassToTest>(); // either use Create or Resolve

classToTest.DoSomething(myArgs);
// classToTest.DoSomething(new MyArgs("myArg1")); DOES NOT WORK - NEEDS TO BE THE SAME INSTANCE

mocker.GetMock<IDependencyToCheck>()
   .Verify(x => x.CallMe("TEST DATA"), Moq.Times.Once);
```

### Bogus, AutoBogus

>Bogus: A simple and sane fake data generator for C#.

Based on and ported from the famed faker.js.

Details: see next: (optimized version): AutoBogus

### AutoBogus / AutoFaker

>A C# library complementing the Bogus generator by adding auto creation and population capabilities.

`install-package AutoBogus`

>Alternative: see AutoFixture

```cs
Bogus.Faker<Customer> customerFaker = new AutoFaker<Customer>()
  .RuleFor(fake => fake.Id, fake => fake.Random.Int(10, 20))
  .RuleSet("empty", rules =>
  {
      rules.RuleFor(fake => fake.Id, () => 0);
  });

// Use explicit conversion or call Generate()
var customer1 = (Customer)customerFaker;
var customer2 = customerFaker.Generate();

customer1.FirstName.Should().NotBe(customer1.LastName);
customer1.FirstName.Should().NotBe(customer2.LastName);
customer1.FirstName.Should().NotBe(customer2.FirstName);
customer1.Id.Should<int>();
customer1.Id.Should().BeInRange(10, 20, "random id's between 10 and 20 are generated");
customer1.DateOfBirth.Should().NotBe(DateTime.Now);
```

### AutoFixture

>AutoFixture is an open source framework for .NET designed to minimize the 'Arrange' phase of your unit tests

It also fills properties with test data.

```cmd
install-package AutoFixture
install-package AutoFixture.Xunit2
```

Fixture example without AutoFixture:

```cs
[Fact]
public void IntroductoryTest()
{
    // Arrange
    Fixture fixture = new Fixture();

    int expectedNumber = fixture.Create<int>();
    MyClass sut = fixture.Create<MyClass>();
    // Act
    int result = sut.Echo(expectedNumber);
    // Assert
    Assert.Equal(expectedNumber, result);
}
```

Same example using AutoFixture:

```cs
[Theory, AutoData]
public void IntroductoryTest(int expectedNumber, MyClass sut) {
    // Act
    int result = sut.Echo(expectedNumber);
    // Assert
    Assert.Equal(expectedNumber, result);
}
```

AutoFaker can be more flexible:

```cs
var pipeline = fixture.Create<PropertyImportData>(); // AutoFixture was unable to create an instance from Serilog.ILogger because it's an interface.
// no issues with AutoFaker
var importFaker = new AutoFaker<PropertyImportData>();
var importData = (PropertyImportData)importFaker;
```

### Mock FileSystem

<https://github.com/System-IO-Abstractions/System.IO.Abstractions>

## NewtonSoft vs. System.Text.Json   
    
|            | NewtonSoft                | System.Text.Json               |
| ---------- | ------------------------- | ------------------------------ |
| parse      | `JArray.Parse(content)`   | `JsonDocument.Parse(content);` |

  
## Static Functions
  
Error:
  
```
System.NotSupportedException : Unsupported expression: f => f.CustomStaticAsync(id, additionalInfo)
Extension methods (here: CustomRepository.CustomStaticAsync) may not be used in setup / verification expressions.  
```
  
>It is not possible to mock static functions
  
Solution:

- There is no need to mock the static function
- Instead, mock the functions, which are called by the static function.  

## Testing 

- Testing, Unit testing Tools: <https://github.com/boeschenstein/testing>

## Information

- EF Core Basics: <https://github.com/boeschenstein/angular9-dotnetcore-ef-sql>
  - Unit Of Work Pattern <https://www.devleader.ca/2024/02/05/unit-of-work-pattern-in-c-for-clean-architecture-what-you-need-to-know/>
- Best practice (Naming): <https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices>
- Specification Pattern:
  - Microsoft Docs: <https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-implementation-entity-framework-core>
  - simple example: https://enterprisecraftsmanship.com/posts/specification-pattern-always-valid-domain-model/
  - Full fledged library https://specification.ardalis.com/
  - Seems state of the art: https://stackoverflow.com/questions/63975708/how-to-add-a-theninclude-for-a-nested-object-within-a-generic-specification-pat
- MediatR Wiki: <https://github.com/jbogard/MediatR/wiki>
- Full application (ASP.NET Core, EF Core, MeditR, Specification Pattern): <https://github.com/dotnet-architecture/eShopOnWeb>
- SQLite: <https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite>
- System.Text.Json vs. NewtonSoft
  - Serialization in .NET: <https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview>
  - Intro: <https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-apis/>
  - Migrate: <https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to>
  - New in .NET 5: <https://devblogs.microsoft.com/dotnet/whats-next-for-system-text-json/>
  - [JsonDocument and JsonElement compared to JToken (like JObject, JArray)](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to?pivots=dotnet-5-0#jsondocument-and-jsonelement-compared-to-jtoken-like-jobject-jarray)
  - <https://devblogs.microsoft.com/dotnet/the-convenience-of-system-text-json/>
