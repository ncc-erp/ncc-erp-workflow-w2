namespace W2.WorkflowInstances
{
    public class ListAllWorkflowInstanceInput
    {
        public int MaxResultCount { get; set; }
        public int SkipCount { get; set; }
        public string Sorting { get; set; }
        public string WorkflowDefinitionId { get; set; }
        public string Status { get; set; }
        public string RequestUser { get; set; }

    }
}
