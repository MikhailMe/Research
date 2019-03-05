using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Consul;
using JaegerNetCoreFirst.App_Data;
using JaegerNetCoreFirst.Tracer;
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
    public class Startup : IDisposable
    {
        private string _appPort;
        private string _appAddress;
        private readonly ConsulClient _consulClient;
        private static readonly ILoggerFactory LoggerFactory;
        private static readonly Jaeger.Tracer Tracer;

        private const string ConsulPort = "8500";
        private const string PathToStorage = "example/config";
        private const string ConnectionString = "";

        static Startup()
        {
            ConsulSettings.ServiceName = "First Service";
            LoggerFactory = new LoggerFactory().AddConsole();
            Tracer = Tracing.Init(ConsulSettings.ServiceName, LoggerFactory);
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _consulClient = new ConsulClient();
        }

        public IConfiguration Configuration { get; }

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
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var url = app.ServerFeatures.Get<IServerAddressesFeature>().Addresses.Single().Split(":");
            _appAddress = $"{url[0]}:{url[1]}";
            _appPort = url[2].Remove(url[2].Length - 1);
            var uri = new Uri($"{_appAddress}:{ConsulPort}");
            _consulClient.Config.Address = uri;
            ConsulSettings.ClientUrl = uri;

            RegisterService();

            PutSettings();

            GetSettings();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        private void PutSettings()
        {
            var pair = new KVPair(PathToStorage)
            {
                Value = Encoding.ASCII.GetBytes(ConnectionString)
            };
            _consulClient.KV.Put(pair, CancellationToken.None);
        }

        private void GetSettings()
        {
            var pair = _consulClient.KV.Get(PathToStorage).GetAwaiter().GetResult().Response;
            var connectionStringJson = JObject.Parse(Encoding.Default.GetString(pair.Value));
            ConsulSettings.ConnectionString = (string)connectionStringJson["connectionString"];
        }

        private async void RegisterService()
        {
            var httpCheck = new AgentServiceCheck
            {
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                Interval = TimeSpan.FromSeconds(3),
                HTTP = $"{_appAddress}:{_appPort}/api/HealthCheck"
            };

            var registration = new AgentServiceRegistration
            {
                Checks = new [] { httpCheck },
                Name = ConsulSettings.ServiceName,
                ID = ConsulSettings.ServiceName,
                Port = int.Parse(_appPort),
                Address = _appAddress
            };

            await _consulClient.Agent.ServiceRegister(registration);
        }

        public void Dispose()
        {
            _consulClient?.Dispose();
        }
    }
}