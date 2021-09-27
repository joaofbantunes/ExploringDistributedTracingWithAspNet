# Exploring distributed tracing with ASP.NET Core
## Intro
Simple set of ASP.NET Core applications to explore distributed tracing support, as well as the standard [W3C Trace Context](https://www.w3.org/TR/trace-context/) usage.

1. `WebClient` - basic Razor Pages web application with a single feature - provide a name to be greeted.
2. `WebApi` - basic web API exposing a single endpoint, receiving the user name and responding with a greeting.
3. `GrpcService` - template provided gRPC service with a single operation, the user name and responding with a greeting.
4. `Worker` - background worker application, handling RabbitMQ messages.

The goal of having these 4 applications is to see the context flowing along with the requests/messages.

ASP.NET Core flows this trace context automatically, for example, including the information in `HttpClient` requests.

~~The default format is not the W3C recommendation, but it's a line of code of configuration to get it.~~ (wasn't when I first did this with ASP.NET Core 3.1, as I'm updating to 6.0, it's now default)

## Viewing the trace context

### Logs

The simplest way to see this in action is looking at the logs, as we can then see this information printed out (assuming the required logging configurations are made).

### OpenTelemetry, Zipkin and Jaeger

A more interesting way to view this in action is to use other telemetry tools.

For this sample, we'll use [OpenTelemetry](https://opentelemetry.io/), [Zipkin](https://zipkin.io/) and [Jaeger](https://www.jaegertracing.io/).

OpenTelemetry provides a set of tools to capture the trace information and then make available to observability tools (like Zipkin and Jaeger). OpenTelemetry provides a set of [NuGet packages](https://github.com/open-telemetry/opentelemetry-dotnet) we can use to easily integration with .NET.

Zipkin and Jaeger, as mentioned before, are observability tools, where we can analyze the behavior of our applications through the collected traces. They are equivalent (though surely there might exist pros and cons to them), so normally we'd use only one, but I wanted to take the opportunity to see both in action.

### Getting things running

Before running the .NET applications, we need to have our dependencies up, which in this case are RabbitMQ, PostgreSQL, Zipkin and Jaeger. To get them all running, there's a Docker Compose file in the repository root, so we just need to execute:

```
docker compose -f docker-compose.dependencies.yml up
```

## Other resources

To do this exploration in general, but particularly the RabbitMQ bits, relied heavily on the docs and examples provided in the [OpenTelemetry .NET repository](https://github.com/open-telemetry/opentelemetry-dotnet), like the [OpenTelemetry Example Application](https://github.com/open-telemetry/opentelemetry-dotnet/tree/core-1.1.0/examples/MicroserviceExample).