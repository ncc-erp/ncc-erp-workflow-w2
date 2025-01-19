using System.ComponentModel.DataAnnotations;

namespace W2.Mezon;

public class RejectTasksInput
{
    [Required] public string Id { get; set; }
    [Required] public string Reason { get; set; }
    [Required] public string Email { get; set; }
}