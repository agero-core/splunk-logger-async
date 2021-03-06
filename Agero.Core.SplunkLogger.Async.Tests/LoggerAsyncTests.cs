﻿using System;
using System.IO;
using System.Threading;
using Agero.Core.Checker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Agero.Core.SplunkLogger.Async.Tests
{
    [TestClass]
    public class LoggerAsyncTests
    {
        private static LoggerAsyncTestsSetup _splunkCollectorInfo;

        [ClassInitialize]
        public static void LoggerTestsInitialize(TestContext context)
        {
            Assert.IsTrue(File.Exists(@"logger-settings.json"), "The configuration file logger-settings.json needs to be setup. Please see https://github.com/agero-core/splunk-logger-async to set it up.");

            _splunkCollectorInfo = JsonConvert.DeserializeObject<LoggerAsyncTestsSetup>(File.ReadAllText(@"logger-settings.json"));
        }

        private static AsyncLogger CreateLogger(string collectorUrl, int processingThreadCount = 2)
        {
            Check.ArgumentIsNullOrWhiteSpace(collectorUrl, nameof(collectorUrl));
            Check.Argument(processingThreadCount > 0, "processingThreadCount > 0");

            return new AsyncLogger(
                collectorUri: new Uri(collectorUrl),
                authorizationToken: _splunkCollectorInfo.AuthenticationToken,
                applicationName: "Test",
                applicationVersion: "1.0.0.0",
                timeout: 10000,
                processingThreadCount: processingThreadCount);
        }

        private static void LogError(IAsyncLogger logger, object thread)
        {
            Check.ArgumentIsNull(logger, nameof(logger));
            Check.ArgumentIsNull(thread, nameof(thread));

            for (var i = 0; i < 10; i++)
            {
                logger.Log("Error", $"Error {i} from thread {thread}", new { thread, iteration = i });
                Console.WriteLine($"Thread - {thread}, iteration - {i}");
                Thread.Sleep(1);
            }
        }

        [TestMethod]
        public void MultiThreading_Test_When_Invalid_Collector_Url()
        {
            using (var logger = CreateLogger("http://localhost/Wrong/"))
            {
                // Act
                for (var i = 1; i <= 5; i++)
                {
                    ThreadPool.QueueUserWorkItem(delegate(object id) { LogError(logger, id); }, i);
                }

                Thread.Sleep(5_000);

                // Assert
                Assert.AreEqual(0, logger.PendingLogCount);
            }

            Thread.Sleep(1500);
        }

        [TestMethod]
        public void MultiThreading_Test_When_Valid_Collector_Url()
        {
            using (var logger = CreateLogger(_splunkCollectorInfo.SplunkCollectorUrl, 2))
            {
                // Act
                for (var i = 1; i <= 5; i++)
                {
                    ThreadPool.QueueUserWorkItem(delegate (object id) { LogError(logger, id); }, i);
                }

                Thread.Sleep(5_000);

                // Assert
                Assert.AreEqual(0, logger.PendingLogCount);
            }

            Thread.Sleep(1500);
        }
    }
}
