using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

builder.Services.AddDbContext<HelloDbContext>(options => options.UseNpgsql("server=localhost;port=5432;user id=user;password=pass;database=Hello"));

builder.Services.AddOpenTelemetryTracing(builder =>
{
    builder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WebApi"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        // to avoid double activity, one for HttpClient, another for the gRPC client
        // -> https://github.com/open-telemetry/opentelemetry-dotnet/blob/core-1.1.0/src/OpenTelemetry.Instrumentation.GrpcNetClient/README.md#suppressdownstreaminstrumentation
        .AddGrpcClientInstrumentation(options => options.SuppressDownstreamInstrumentation = true)
        // besides instrumenting EF, we also want the queries to be part of the telemetry (hence SetDbStatementForText = true)
        .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
        .AddSource(nameof(MessagePublisher)) // when we manually create activities, we need to setup the sources here
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

app.MapGet("/hello", async (string username, GreeterClient greeterClient, MessagePublisher messagePublisher, HelloDbContext db) =>
{
    var response = await greeterClient.SayHelloAsync(new GrpcService.HelloRequest { Name = username });
    
    db.HelloEntries.Add(new HelloEntry(Guid.NewGuid(), username, DateTime.UtcNow));
    await db.SaveChangesAsync();
    
    await messagePublisher.PublishAsync(new HelloMessage(response.Message));
    
    return new HelloResponse(response.Message);
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HelloDbContext>();
    db.Database.EnsureCreated(); // good only for demos 😉
}

app.Run();

public record HelloResponse(string Greeting);

public record HelloMessage(string Greeting);

public record HelloEntry(Guid Id, string Username, DateTime CreatedAt);

public class HelloDbContext : DbContext
{

#pragma warning disable CS8618 // DbSets populated by EF
    public HelloDbContext(DbContextOptions<HelloDbContext> options) : base(options)
    {
    }


    public DbSet<HelloEntry> HelloEntries { get; set; }
#pragma warning restore CS8618

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}

public class HelloEntryConfiguration : IEntityTypeConfiguration<HelloEntry>
{
    public void Configure(EntityTypeBuilder<HelloEntry> builder)
    {
        builder.HasKey(e => e.Id);
    }
}