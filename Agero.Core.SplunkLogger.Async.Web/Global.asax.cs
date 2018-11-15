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
                WebApiConfig.Configure(config);
            });
        }
    }
}
