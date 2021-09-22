using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using static GrpcService.Greeter;

// not needed, W3C is now default
// System.Diagnostics.Activity.DefaultIdFormat = System.Diagnostics.ActivityIdFormat.W3C;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddGrpcClient<GreeterClient>(options =>
{
    options.Address = new Uri("https://localhost:5004");
});

builder.Services.AddOpenTelemetryTracing(builder =>
{
    builder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WebApi"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddGrpcClientInstrumentation()
        .AddZipkinExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
        });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapGet("/hello", async (string username, GreeterClient greeterClient) => 
{
    var response = await greeterClient.SayHelloAsync(new GrpcService.HelloRequest { Name = username });
    return new HelloResponse(response.Message);
});

app.Run();


public record HelloResponse(string Greeting);