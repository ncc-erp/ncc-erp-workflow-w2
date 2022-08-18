using Elsa.Models;
using Elsa.Persistence.Specifications;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace W2.Specifications
{
    public class ListAllWorkflowDefinitionsSpecification : PublishedWorkflowDefinitionsSpecification
    {
        public string TenantId { get; private set; }
        public string[] Ids { get; set; }

        public ListAllWorkflowDefinitionsSpecification(string tenantId, string[] ids)
        {
            TenantId = tenantId;
            Ids = ids;
        }

        public override Expression<Func<WorkflowDefinition, bool>> ToExpression()
        {
            var predicate = base.ToExpression();
            
            predicate = predicate.And(x => x.IsLatest);

            if (Ids != null && Ids.Any())
            {
                predicate = predicate.And(x => Ids.Contains(x.DefinitionId));
            }

            if (!string.IsNullOrWhiteSpace(TenantId))
            {
                predicate = predicate.And(x => x.TenantId == TenantId);
            }

            return predicate;
        }
    }
}
