using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// not needed, W3C is now default
// System.Diagnostics.Activity.DefaultIdFormat = System.Diagnostics.ActivityIdFormat.W3C;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(builder => builder.AddSeq());

builder.Services.AddRazorPages();

builder.Services.AddHttpClient();

builder.Services.AddOpenTelemetryTracing(builder =>
{
    builder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WebClient"))
        .AddAspNetCoreInstrumentation(
            // if we wanted to ignore some specific requests, we could use the filter
            options => options.Filter = httpContext => !httpContext.Request.Path.Value?.Contains("/_framework/aspnetcore-browser-refresh.js") ?? true)
        .AddHttpClientInstrumentation(
            // we can hook into existing activities and customize them
            options => options.Enrich = (activity, eventName, rawObject) =>
            {
                if(eventName == "OnStartActivity" && rawObject is System.Net.Http.HttpRequestMessage request && request.Method == HttpMethod.Get)
                {
                    activity.SetTag("RandomDemoTag", "Adding some random demo tag, just to see things working");
                }
            }
        )
        .AddZipkinExporter(options =>
        {
            // not needed, it's the default
            //options.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
        })
        .AddJaegerExporter(options =>
        {
            // not needed, it's the default
            //options.AgentHost = "localhost";
            //options.AgentPort = 6831;
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