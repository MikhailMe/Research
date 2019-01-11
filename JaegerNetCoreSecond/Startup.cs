using System;
using System.Net.Http;
using System.Text;
using Consul;
using JaegerNetCoreSecond.App_Data;
using JaegerNetCoreThird.Tracer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OpenTracing.Util;

namespace JaegerNetCoreThird
{
    public class Startup
    {
        private static readonly ILoggerFactory LoggerFactory = new LoggerFactory().AddConsole();
        private static readonly Jaeger.Tracer Tracer = Tracing.Init("Third Service", LoggerFactory);

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            GlobalTracer.Register(Tracer);
            services.AddOpenTracing();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            RegisterService();
            GetSettings();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        public async void GetSettings()
        {
            var client = new HttpClient();
            var getRequest = await client.GetStringAsync("http://localhost:8500/v1/kv/example/config");

            var jObject = JObject.Parse(Utils.GetJson(getRequest));
            var value = (string)jObject["Value"];
            var decodedValue = Encoding.UTF8.GetString(Convert.FromBase64String(value));

            JObject jsonSettings = JObject.Parse(decodedValue);
            ConsulSettings.ConnectionString = (string)jsonSettings["connectionString"];
        }

        public async void RegisterService()
        {
            var registration = new AgentServiceRegistration
            {
                Name = "Third",
                Port = 56509,
                Address = "http://localhost"
            };

            using (var client = new ConsulClient())
            {
                await client.Agent.ServiceRegister(registration);
            }
        }

    }
}
