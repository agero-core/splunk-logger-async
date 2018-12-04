using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Agero.Core.Checker;
using Newtonsoft.Json;

namespace Agero.Core.SplunkLogger.Async.Web.Controllers
{
    [RoutePrefix("logs")]
    public class LoggingController : ApiController
    {
        private static readonly AsyncLogger _logger = new Func<AsyncLogger>(() =>
        {
            var path = HttpContext.Current.Server.MapPath("~/logger-settings.json");

            Check.Assert(File.Exists(path), "The configuration file logger-settings.json needs to be setup. Please see https://github.com/agero-core/splunk-logger-async to set it up.");

            var setup = JsonConvert.DeserializeObject<LoggerAsyncTestsSetup>(File.ReadAllText(path));

            return new AsyncLogger(
                collectorUri: new Uri(setup.SplunkCollectorUrl),
                authorizationToken: setup.AuthenticationToken,
                applicationName: "WebTest",
                applicationVersion: Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                timeout: 10000,
                processingThreadCount: 2);
        })();

        /// <remarks>POST /logs</remarks>
        [Route("")]
        [HttpPost]
        public int CreateLogs()
        {
            for (var i = 0; i < 10; i++)
                _logger.Log("Error", $"Message {i}");

            return GetPendingCount();
        }

        /// <remarks>GET /logs/pending</remarks>
        [Route("pending")]
        [HttpGet]
        public int GetPendingCount() => _logger.PendingLogCount;
    }
}