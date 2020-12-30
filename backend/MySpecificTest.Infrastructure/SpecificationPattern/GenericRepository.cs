using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MySpecificTest.Infrastructure.SpecificationPattern
{
    /// <summary>
    /// https://github.com/dotnet-architecture/eShopOnWeb
    /// </summary>
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
}