using GrpcService;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// not needed, W3C is now default
// System.Diagnostics.Activity.DefaultIdFormat = System.Diagnostics.ActivityIdFormat.W3C;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.AddOpenTelemetryTracing(builder =>
{
    builder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("GrpcService"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource(nameof(GreeterService))
        .AddZipkinExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
        });
});


var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcService<GreeterService>();

    endpoints.MapGet("/", async context =>
    {
        await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
    });
});

app.Run();