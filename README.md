# ASP.NET Core 5 + EF

## Main Goals

- Specification pattern
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

## Information

- Basics: <https://github.com/boeschenstein/angular9-dotnetcore-ef-sql>
