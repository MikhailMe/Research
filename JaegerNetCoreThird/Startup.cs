using System;
using System.Collections.Generic;
using System.Text;
using Consul;
using JaegerNetCoreFirst.Tracer;
using JaegerNetCoreSecond.App_Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
        private static readonly ILoggerFactory LoggerFactory = new LoggerFactory().AddConsole();
        private static readonly Jaeger.Tracer Tracer = Tracing.Init("First Service", LoggerFactory);

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

        public async void RegisterService()
        {
            var registration = new AgentServiceRegistration
            {
                Name = "First",
                Port = 56510,
                Address = "http://localhost"
            };

            using (var client = new ConsulClient())
            {
                await client.Agent.ServiceRegister(registration);
            }
        }

    }
}
