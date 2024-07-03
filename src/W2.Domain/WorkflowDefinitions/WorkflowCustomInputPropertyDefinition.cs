namespace W2.WorkflowDefinitions
{
    public class WorkflowCustomInputPropertyDefinition
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsRequired { get; set; }
        public bool IsTitle { get; set; }
        public string TitleTemplate { get; set; }
        public string Helper { get; set; }
        public string DefaultValue { get; set; }
    }
}
