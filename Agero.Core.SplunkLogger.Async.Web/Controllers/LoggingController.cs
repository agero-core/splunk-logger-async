using System.Web.Http;

namespace Agero.Core.SplunkLogger.Async.Web.Controllers
{
    [RoutePrefix("logging")]
    public class LoggingController : BaseController
    {
        /// <remarks>GET /logging</remarks>
        [Route("")]
        [HttpGet]
        public int GetLogging([FromUri]int size = 100)
        {
            var logger = Container.Get<ILoggerAsync>();

            for (var i = 0; i < size; i++)
                logger.Log("Error", $"Error {i}");

            return PendingCount();
        }

        /// <remarks>GET /logging/pendingCount</remarks>
        [Route("pendingCount")]
        [HttpGet]
        public int PendingCount() => ((LoggerAsync) Container.Get<ILoggerAsync>()).PendingLogCount;
    }
}