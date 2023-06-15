using Elsa.Models;
using Elsa.Persistence.Specifications;
using System;
using System.Linq.Expressions;

namespace W2.Specifications
{
    public class FindWorkflowDefinitionByIdSpecification : Specification<WorkflowDefinition>
    {
        public string Id { get; set; }

        public FindWorkflowDefinitionByIdSpecification(string id)
        {
            Id = id;
        }

        public override Expression<Func<WorkflowDefinition, bool>> ToExpression()
        {
            Expression<Func<WorkflowDefinition, bool>> predicate = x => x.DefinitionId == Id && x.IsLatest;

            return predicate;
        }
    }
}
