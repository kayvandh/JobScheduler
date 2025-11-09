using System.Linq.Expressions;

namespace Framework.Persistance.Interfaces
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>>? Criteria { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; }
    }
}