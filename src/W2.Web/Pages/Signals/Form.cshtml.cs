using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using W2.Signals;

namespace W2.Web.Pages.Signals
{
    public class FormModel : PageModel
    {
        private readonly ISignalAppService _signalAppService;

        public FormModel(ISignalAppService signalAppService)
        {
            _signalAppService = signalAppService;
        }

        [BindProperty(SupportsGet = true)]
        [FromQuery(Name = "token")]
        public string Token { get; set; }

        [BindProperty]
        public SignalModelDto SignalModel { get; set; }

        [BindProperty]
        public Dictionary<string, string> SignalInputs { get; set; } = new Dictionary<string, string>();

        public async Task OnGetAsync()
        {
            SignalModel = await _signalAppService.GetSignalModelFromTokenAsync(Token);
            foreach (var input in SignalModel.RequiredInputs)
            {
                SignalInputs.TryAdd(input, string.Empty);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var triggerSignalInput = new TriggerSignalWithInputDto
            {
                Signal = SignalModel.Name,
                WorkflowInstanceId = SignalModel.WorkflowInstanceId,
                Inputs = SignalInputs
            };
            await _signalAppService.TriggerAsync(triggerSignalInput);
            return RedirectToPage("/Signals");
        }
    }
}
