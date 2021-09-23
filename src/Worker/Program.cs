using EasyNetQ;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Worker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<MessageHandler>();
        services.AddSingleton<IBus>(_ => RabbitHutch.CreateBus("host=localhost"));
        services.AddOpenTelemetryTracing(builder =>
        {
            builder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Worker"))
                .AddSource(nameof(MessageHandler))
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

    })
    .Build();

await host.RunAsync();

public record HelloMessage(string Greeting);