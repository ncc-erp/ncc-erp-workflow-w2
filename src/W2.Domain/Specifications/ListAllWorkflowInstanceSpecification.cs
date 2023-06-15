using Elsa.Models;
using Elsa.Persistence.Specifications;
using System;
using System.Linq;
using System.Linq.Expressions;
using Volo.Abp.TenantManagement;

namespace W2.Specifications
{
    public class ListAllWorkflowInstanceSpecification : Specification<WorkflowInstance>
    {
        public string[] Ids { get; set; }

        public ListAllWorkflowInstanceSpecification(string[] Ids) 
        { 
            this.Ids = Ids;
        }

        public override Expression<Func<WorkflowInstance, bool>> ToExpression()
        {
            Expression<Func<WorkflowInstance, bool>> predicate = x => Ids.Contains(x.Id);
            return predicate;
        }
    }
}
