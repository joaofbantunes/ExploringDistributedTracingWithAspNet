using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// not needed, W3C is now default
// System.Diagnostics.Activity.DefaultIdFormat = System.Diagnostics.ActivityIdFormat.W3C;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddHttpClient();

builder.Services.AddOpenTelemetryTracing(builder =>
{
    builder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WebClient"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddZipkinExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
        });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
});

app.Run();