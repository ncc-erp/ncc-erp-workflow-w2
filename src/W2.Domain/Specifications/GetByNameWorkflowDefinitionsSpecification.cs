using Elsa.Models;
using Elsa.Persistence.Specifications;
using System;
using System.Linq.Expressions;

namespace W2.Specifications;

public class GetByNameWorkflowDefinitionsSpecification : Specification<WorkflowDefinition>
{
    public string CommandKeyword { get; set; }

    public GetByNameWorkflowDefinitionsSpecification(string command)
    {
        CommandKeyword = getNameByKeyword(command);
    }

    public override Expression<Func<WorkflowDefinition, bool>> ToExpression()
    {
        Expression<Func<WorkflowDefinition, bool>> predicate = x => x.Name == CommandKeyword;

        return predicate;
    }

    private string getNameByKeyword(string command)
    {
        switch (command)
        {
            case "changeofficerequest":
                return "Change Office Request";
            case "devicerequest":
                return "Device Request";
            case "officeequipmentrequest":
                return "Office Equipment Request";
            case "probationaryconfirmationrequest":
                return "Probationary Confirmation Request";
            case "wfhrequest":
                return "WFH Request";
            default:
                return command;
        }
    }
}