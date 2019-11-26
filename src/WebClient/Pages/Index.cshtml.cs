using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace WebClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ITracer _tracer;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(TracerFactoryBase tracerFactory, ILogger<IndexModel> logger)
        {
            _tracer = tracerFactory.GetTracer(null); //use default tracer
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public async Task OnGetAsync()
        {
            await DoSomeActivityAsync();
        }
        
        public IActionResult OnPost() => RedirectToPage("hello", new { username = Input.Username });

        private async Task DoSomeActivityAsync()
        {
            using var span = _tracer.StartActiveSpan("SomeActivity", out _);
            _logger.LogInformation("Doing some activity, just for tracing!");
            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }


        public class InputModel
        {
            [Required]
            public string Username { get; set; }

        }
    }
}
