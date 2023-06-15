using System.ComponentModel.DataAnnotations;

namespace W2.WorkflowInstances
{
    public enum WorkflowFinalStatus
    {
        [Display(Name = "Approved")]
        Approved,

        [Display(Name = "Rejected")]
        Rejected,

        [Display(Name = "None")]
        None
    }
}
