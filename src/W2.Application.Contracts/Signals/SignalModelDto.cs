using System.Collections.Generic;

namespace W2.Signals
{
    public class SignalModelDto
    {
        public string Name { get; set; }
        public string WorkflowInstanceId { get; set; }
        public List<string> RequiredInputs { get; set; } = new List<string>();
    }
}
