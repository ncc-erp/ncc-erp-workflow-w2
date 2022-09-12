using System.Collections.Generic;

namespace W2.Signals
{
    public class TriggerSignalWithInputDto
    {
        public string Signal { get; set; }
        public string WorkflowInstanceId { get; set; }
        public Dictionary<string, string> Inputs { get; set; }
    }
}
