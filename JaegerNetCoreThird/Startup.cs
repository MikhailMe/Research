using System;
using System.Collections.Generic;
using JaegerNetCoreFirst.Tracer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing.Contrib.NetCore.CoreFx;
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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
