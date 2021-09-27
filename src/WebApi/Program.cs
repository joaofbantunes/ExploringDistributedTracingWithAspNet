using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WebApi;
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
        // to avoid double activity, one for HttpClient, another for the gRPC client
        // -> https://github.com/open-telemetry/opentelemetry-dotnet/blob/core-1.1.0/src/OpenTelemetry.Instrumentation.GrpcNetClient/README.md#suppressdownstreaminstrumentation
        .AddGrpcClientInstrumentation(options => options.SuppressDownstreamInstrumentation = true)
        .AddSource(nameof(MessagePublisher))
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

builder.Services.AddSingleton<MessagePublisher>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapGet("/hello", async (string username, GreeterClient greeterClient, MessagePublisher messagePublisher) =>
{
    var response = await greeterClient.SayHelloAsync(new GrpcService.HelloRequest { Name = username });
    await messagePublisher.PublishAsync(new HelloMessage(response.Message));
    return new HelloResponse(response.Message);
});

app.Run();

public record HelloResponse(string Greeting);

public record HelloMessage(string Greeting);