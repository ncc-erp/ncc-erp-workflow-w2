using System.ComponentModel.DataAnnotations;

namespace W2.Mezon;

public class ListPropertyDefinitionsByMezonCommandDto
{
    [Required]
    public string Keyword { get; set; }
    public string Email { get; set; }
}