using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry.Trace.Sampler;
using static GrpcService.Greeter;

namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            System.Diagnostics.Activity.DefaultIdFormat = System.Diagnostics.ActivityIdFormat.W3C;

            services.AddControllers();
            services.AddGrpcClient<GreeterClient>(options =>
            {
                options.Address = new Uri("https://localhost:5004");
            });

            services
                .AddOpenTelemetry(builder =>
                {
                    builder
                    .SetSampler(Samplers.AlwaysSample)
                    .UseZipkin(options =>
                    {
                        options.ServiceName = "WebApi";
                        options.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
                    })
                    .AddRequestCollector()
                    .AddDependencyCollector();
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
