using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace W2.Web.Pages.ViewDesigner
{
    public class IndexModel : W2PageModel
    {
        [BindProperty(SupportsGet = true)]
        [FromQuery(Name = "id")]
        public string WorkflowInstanceId { get; set; }

        public void OnGet()
        {
        }
    }
}
