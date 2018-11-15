using System.Linq;
using System.Web.Http;

namespace Agero.Core.SplunkLogger.Async.Web
{
    public static class WebApiConfig
    {
        public static void Configure(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            var formatters = GlobalConfiguration.Configuration.Formatters;

            var formattersToRemove = formatters.Where(f => f != formatters.JsonFormatter).ToArray();
            foreach (var formatter in formattersToRemove)
                formatters.Remove(formatter);

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
        }
    }
}
