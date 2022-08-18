using Elsa.Models;
using Elsa.Persistence.Specifications;
using System;
using System.Linq.Expressions;

namespace W2.Specifications
{
    public class PublishedWorkflowDefinitionsSpecification : Specification<WorkflowDefinition>
    {
        public override Expression<Func<WorkflowDefinition, bool>> ToExpression()
        {
            Expression<Func<WorkflowDefinition, bool>> predicate = x => x.IsPublished;

            return predicate;
        }
    }
}
