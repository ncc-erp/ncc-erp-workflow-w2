using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace W2.Web.Pages.Designer
{
    public class IndexModel : W2PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string WorkflowDefinitionId { get; set; }

        public void OnGet()
        {
        }
    }
}
