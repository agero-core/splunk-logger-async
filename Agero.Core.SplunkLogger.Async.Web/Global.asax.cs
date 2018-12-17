using System.Net;
using System.Web;
using System.Web.Http;

namespace Agero.Core.SplunkLogger.Async.Web
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(config =>
            {
                config.MapHttpAttributeRoutes();
                config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            });
        }
    }
}
