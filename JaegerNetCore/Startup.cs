using System;
using System.Linq;
using System.Text;
using Consul;
using JaegerNetCoreSecond.App_Data;
using JaegerNetCoreSecond.Tracer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OpenTracing.Util;

namespace JaegerNetCoreSecond
{
    public class Startup
    {
        private string _appPort;
        private string _appAddress;
        private static readonly ILoggerFactory LoggerFactory;
        private static readonly Jaeger.Tracer Tracer;

        static Startup()
        {
            ConsulSettings.ServiceName = "Second Service";
            LoggerFactory = new LoggerFactory().AddConsole();
            Tracer = Tracing.Init(ConsulSettings.ServiceName, LoggerFactory);
        }

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
            // get address and port from settings
            var url = app.ServerFeatures.Get<IServerAddressesFeature>().Addresses.Single().Split(":");
            _appAddress = $"{url[0]}:{url[1]}";
            _appPort = url[2].Remove(url[2].Length - 1);

            RegisterService();
            GetSettings();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        private void GetSettings()
        {
            var pair = new ConsulClient().KV.Get("example/config").GetAwaiter().GetResult().Response;
            JObject connectionStringJson = JObject.Parse(Encoding.Default.GetString(pair.Value));
            ConsulSettings.ConnectionString = (string)connectionStringJson["connectionString"];
        }

        private async void RegisterService()
        {
            var httpCheck = new AgentServiceCheck
            {
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(20),
                Interval = TimeSpan.FromSeconds(10),
                HTTP = $"{_appAddress}:{_appPort}/api/HealthCheck"
            };

            var registration = new AgentServiceRegistration
            {
                Checks = new [] { httpCheck },
                ID = ConsulSettings.ServiceName,
                Name = ConsulSettings.ServiceName,
                Port = int.Parse(_appPort),
                Address = _appAddress
            };

            using (var client = new ConsulClient())
            {
                 await client.Agent.ServiceRegister(registration);
            }
        }
    }
}