using System.Collections.Generic;

namespace W2.Mezon;

public class MezonAppRequestTemplateDto
{
    public string WorkflowDefinitionId { get; set; }
    public List<EmbedDto> Embed { get; set; }
}

public class EmbedDto
{
    public string Name { get; set; }
    public string Value { get; set; }
    public InputDto Inputs { get; set; }
}

public class InputDto
{
    public string Id { get; set; }
    public int Type { get; set; }
    public ComponentDto Component { get; set; }
}

public class ComponentDto
{
    public string Id { get; set; }
    public int Type { get; set; }
    public string Placeholder { get; set; }
    public bool Required { get; set; }
    public bool Textarea { get; set; }
    public List<OptionDto> Options { get; set; }
}

public class OptionDto
{
    public string Label { get; set; }
    public string Value { get; set; }
}