# Exploring distributed tracing with ASP.NET Core
## Intro
Simple set of ASP.NET Core applications to explore [W3C Trace Context](https://www.w3.org/TR/trace-context/) support.

1. `WebClient` - basic Razor Pages web application with a single feature - provide a name to be greeted.
2. `WebApi` - basic web API exposing a single endpoint, receiving the user name and responding with a greeting.
3. `GrpcService` - template provided gRPC service with a single operation, the user name and responding with a greeting.

The goal of having these 3 applications is to see the context flowing along with the requests.

ASP.NET Core flows this trace context automatically, for example, including the information in `HttpClient` requests.

The default format is not the W3C recommendation, but it's a line of code of configuration to get it.

```csharp
System.Diagnostics.Activity.DefaultIdFormat = System.Diagnostics.ActivityIdFormat.W3C;
```

## Viewing the trace context
### Logs
The simplest way to see this in action is looking at the logs, as we can then see this information printed out (assuming the required logging configurations are made).

### OpenTelemetry and Zipkin
A more interesting way to view this in action is to use other telemetry tools.

For this sample, we'll use [OpenTelemetry](https://opentelemetry.io/) and [Zipkin](https://zipkin.io/).

OpenTelemetry provides a set of tools to capture the trace information and then make available to observability tools (like Zipkin). OpenTelemetry provides a set of [NuGet packages](https://github.com/open-telemetry/opentelemetry-dotnet) we can use to easily integration with .NET.

Zipkin, like mentioned before, is an observability tool, where we can analyze the behavior of our applications through the collected traces.

The easiest way to get it running is using Docker, with the following line to get the container going.

```sh
docker run -d -p 9411:9411 openzipkin/zipkin
```