namespace JaegerNetCoreSecond
{
    public class Utils
    {
        public static string GetJson(string response)
        {
            var indexOfOpenBracket = response.IndexOf('{');
            var indexOfCloseBracket = response.LastIndexOf('}');
            return response.Substring(indexOfOpenBracket, indexOfCloseBracket - indexOfOpenBracket + 1);
        }
    }
}