using System;

namespace JaegerNetCoreSecond.App_Data
{
    public class ConsulSettings
    {
        public static string Url { get; set; }
        public static string ServiceName { get; set; }
        public static string ConnectionString { get; set; }
        public static Uri ClientUrl { get; set; }
    }
}