using System.ComponentModel.DataAnnotations;

namespace W2.Mezon
{
    public class ApproveTasksInput
    {
        public string DynamicActionData { get; set; }
        public string Id { get; set; }
        [Required] public string Email { get; set; }
    }
}