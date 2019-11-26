using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static GrpcService.Greeter;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelloController : ControllerBase
    {
        private readonly GreeterClient _greeterClient;

        public HelloController(GreeterClient greeterClient)
        {
            _greeterClient = greeterClient;
        }

        [HttpGet]
        public async Task<ActionResult<HelloResponse>> GetAsync(string username)
        {
            var response = await _greeterClient.SayHelloAsync(new GrpcService.HelloRequest { Name = username });
            return new HelloResponse { Greeting = response.Message };
        }

        public class HelloResponse
        {
            public string Greeting { get; set; }
        }
    }
}
