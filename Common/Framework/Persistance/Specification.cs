using Framework.Persistance.Interfaces;
using System.Linq.Expressions;

namespace Framework.Persistance
{
    public abstract class Specification<T> : ISpecification<T>
    {
        public Expression<Func<T, bool>>? Criteria { get; protected set; }
        public List<Expression<Func<T, object>>> Includes { get; } = [];
        public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; protected set; }

        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        protected void ApplyOrderBy(Func<IQueryable<T>, IOrderedQueryable<T>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        protected void ApplyCriteria(Expression<Func<T, bool>> criteriaExpression)
        {
            Criteria = criteriaExpression;
        }
    }
}