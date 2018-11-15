using System;
using System.IO;
using System.Reflection;
using Agero.Core.DIContainer;
using Agero.Core.Lazy;
using Newtonsoft.Json;

namespace Agero.Core.SplunkLogger.Async.Web
{
    public static class DIContainer
    {
        private static readonly SyncLazy<IReadOnlyContainer> _container =
            new SyncLazy<IReadOnlyContainer>(CreateContainer);

        private static readonly LoggerAsyncTestsSetup _loggerSetup =
            JsonConvert.DeserializeObject<LoggerAsyncTestsSetup>(File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/logger-settings.json")));

        private static IReadOnlyContainer CreateContainer()
        {
            var container = ContainerFactory.Create();

            container.RegisterFactoryMethod<ILoggerAsync>(c =>
                    new LoggerAsync
                    (
                        collectorUri: new Uri(_loggerSetup.SplunkCollectorUrl),
                        authorizationToken: _loggerSetup.AuthenticationToken,
                        applicationName: "WebTest",
                        applicationVersion: Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                        timeout: 10000,
                        processingThreadCount: 2
                    ),
                Lifetime.PerContainer
            );

            return container;
        }

        public static IReadOnlyContainer Instance => _container.Value;
    }
}