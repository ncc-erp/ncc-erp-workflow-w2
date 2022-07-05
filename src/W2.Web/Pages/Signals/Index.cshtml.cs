using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using W2.Signals;

namespace W2.Web.Pages.Signal
{
    public class IndexModel : W2PageModel
    {
        private readonly ISignalAppService _signalAppService;

        public IndexModel(ISignalAppService signalAppService)
        {
            _signalAppService = signalAppService;
        }

        [BindProperty(SupportsGet = true)]
        [FromQuery(Name = "token")]
        public string Token { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await _signalAppService.TriggerAsync(Token);

            return Page();
        }
    }
}
