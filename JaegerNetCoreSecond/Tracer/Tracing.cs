using Jaeger;
using Jaeger.Samplers;
using Microsoft.Extensions.Logging;

namespace JaegerNetCoreThird.Tracer
{
    public static class Tracing
    {
        public static Jaeger.Tracer Init(string serviceName, ILoggerFactory loggerFactory)
        {
            var samplerConfiguration = new Configuration.SamplerConfiguration(loggerFactory)
                .WithType(ConstSampler.Type)
                .WithParam(1);

            var reporterConfiguration = new Configuration.ReporterConfiguration(loggerFactory)
                .WithLogSpans(true);

            return (Jaeger.Tracer)new Configuration(serviceName, loggerFactory)
                .WithSampler(samplerConfiguration)
                .WithReporter(reporterConfiguration)
                .GetTracer();
        }
    }
}
