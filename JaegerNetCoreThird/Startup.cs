using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Consul;
using JaegerNetCoreFirst.Tracer;
using JaegerNetCoreSecond.App_Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OpenTracing.Util;

namespace JaegerNetCoreFirst
{
    public class Startup
    {
        private string _appPort;
        private string _appAddress;
        private const string ServiceName = "First Service";
        private static readonly ILoggerFactory LoggerFactory = new LoggerFactory().AddConsole();
        private static readonly Jaeger.Tracer Tracer = Tracing.Init(ServiceName, LoggerFactory);

        public const string DiagnosticListenerName = "Microsoft.AspNetCore";

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
            var ignorePatterns = new List<Func<HttpContext, bool>>
            {
                context => !context.Request.Headers.ContainsKey("TracingEnable")
            };

            services.AddOpenTracing(c => c.AddAspNetCore()
                .ConfigureAspNetCore(options => options.Hosting.IgnorePatterns.AddRange(ignorePatterns)
            ));
            //services.Configure<HttpHandlerDiagnosticOptions>(options => options.IgnorePatterns.Add(x => !x.Headers.Contains("TracingEnable")));
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
            using (var consulClient = new ConsulClient())
            {
                var pair = consulClient.KV.Get("example/config").GetAwaiter().GetResult().Response;
                JObject connectionStringJson = JObject.Parse(Encoding.Default.GetString(pair.Value));
                ConsulSettings.ConnectionString = (string)connectionStringJson["connectionString"];
            }
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
                Name = ServiceName,
                ID = ServiceName,
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