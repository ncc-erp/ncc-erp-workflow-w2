using Volo.Abp.TextTemplating;
using Volo.Abp.TextTemplating.Scriban;

namespace W2.Templates
{
    public class CustomTemplateDefinitionProvider : TemplateDefinitionProvider
    {
        public override void Define(ITemplateDefinitionContext context)
        {
            context.Add(
                new TemplateDefinition(
                        CustomTemplateNames.NewInstanceCreatedEmail,
                        typeof(Localization.W2Resource)
                    )
                    .WithScribanEngine()
                    .WithVirtualFilePath("/EmailTemplates/NewInstanceCreatedEmail.tpl", true)
            );
        }
    }
}
