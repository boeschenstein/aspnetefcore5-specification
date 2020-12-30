using System.Collections.Generic;

namespace MySpecificTest.Infrastructure.SpecificationPattern
{
    public interface IGenericRepository<T> where T : class
    {
        IEnumerable<T> List(ISpecification<T> spec);
    }
}