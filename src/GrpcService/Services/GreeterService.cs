using Grpc.Core;
using System.Diagnostics;

namespace GrpcService;

public class GreeterService : Greeter.GreeterBase
{
    private static readonly ActivitySource ActivitySource = new(nameof(GreeterService));

    public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        using (var x = ActivitySource.StartActivity(nameof(SayHello)))
        {
            // something that takes a bit
            await Task.Delay(TimeSpan.FromMilliseconds(250));
        }

        return new HelloReply { Message = "Hello " + request.Name };
    }
}
