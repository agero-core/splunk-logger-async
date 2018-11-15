using System.Web.Http;
using Agero.Core.DIContainer;

namespace Agero.Core.SplunkLogger.Async.Web.Controllers
{
    public abstract class BaseController : ApiController
    {
        protected static IReadOnlyContainer Container => DIContainer.Instance;
    }
}