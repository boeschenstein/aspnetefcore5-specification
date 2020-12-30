using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MySpecificTest.Infrastructure.SpecificationPattern
{
    /// <summary>
    /// https://deviq.com/specification-pattern/
    /// </summary>
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> Criteria { get; }
        List<Expression<Func<T, object>>> Includes { get; }
    }
}