using System;
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

        public void GetSettings()
        {
            var pair = new ConsulClient().KV.Get("example/config").GetAwaiter().GetResult().Response;
            JObject connectionStringJson = JObject.Parse(Encoding.Default.GetString(pair.Value));
            ConsulSettings.ConnectionString = (string)connectionStringJson["connectionString"];
        }

        public void RegisterService()
        {
            var httpCheck = new AgentServiceCheck
            {
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                Interval = TimeSpan.FromSeconds(30),
                HTTP = "http://localhost:{56509}/HealthCheck"
            };

            var tcpCheck = new AgentServiceCheck
            {
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                Interval = TimeSpan.FromSeconds(30),
                TCP = "http://localhost:56509"
            };

            var registration = new AgentServiceRegistration
            {
                Checks = new [] {tcpCheck, httpCheck},
                Name = "Third",
                ID = "Third",
                Port = 56509,
                Address = "http://localhost"
            };

            using (var client = new ConsulClient())
            {
                client.Agent.ServiceRegister(registration).GetAwaiter().GetResult();
            }
        }

    }
}
