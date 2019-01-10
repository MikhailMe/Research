using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using JaegerNetCoreSecond.App_Data;
using JaegerNetCoreSecond.Tracer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OpenTracing.Util;

namespace JaegerNetCoreSecond
{
    public class Startup
    {
        private static readonly ILoggerFactory LoggerFactory = new LoggerFactory().AddConsole();
        private static readonly Jaeger.Tracer Tracer = Tracing.Init("Second Service", LoggerFactory);

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
            var size = getRequest.Length;
            var f = getRequest.IndexOf('{');
            var l = getRequest.LastIndexOf('}');
            var json = getRequest.Substring(f , l - f + 1);
            JObject jObject = JObject.Parse(json);
            string value = (string)jObject["Value"];
            byte[] data = Convert.FromBase64String(value);
            string decodedValue = Encoding.UTF8.GetString(data);

            JObject jsonSettings = JObject.Parse(decodedValue);
            ConsulSettings.Url = (string)jsonSettings["thirdUrl"];
            ConsulSettings.ConnectionString = (string)jsonSettings["connectionString"];
        }
    }
}
