using Agero.Core.Checker;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Agero.Core.SplunkLogger.Async
{
    /// <summary>Asynchronous Splunk Logger</summary>
    public class AsyncLogger : IAsyncLogger
    {
        private const int QUEUE_READ_TIMEOUT = 1000;

        private readonly BlockingCollection<LogItem> _queue;
        private readonly Logger _logger;

        private bool _isDisposed;

        private int _actualProcessingThreadCount;

        /// <summary>Constructor</summary>
        /// <param name="collectorUri">Splunk HTTP collector URL</param>
        /// <param name="authorizationToken">Splunk authorization token</param>
        /// <param name="applicationName">Unique application name</param>
        /// <param name="applicationVersion">Application version</param>
        /// <param name="timeout">Splunk HTTP collector timeout (milliseconds)</param>
        /// <param name="processingThreadCount">Number of threads for processing logs (1-5)</param>
        public AsyncLogger(Uri collectorUri, string authorizationToken, string applicationName, string applicationVersion, int timeout = 10000, int processingThreadCount = 2)
        {
            Check.ArgumentIsNull(collectorUri, nameof(collectorUri));
            Check.ArgumentIsNullOrWhiteSpace(authorizationToken, nameof(authorizationToken));
            Check.ArgumentIsNullOrWhiteSpace(applicationName, nameof(applicationName));
            Check.ArgumentIsNullOrWhiteSpace(applicationVersion, nameof(applicationVersion));
            Check.Argument(timeout > 0, "timeout > 0");
            Check.Argument(0 < processingThreadCount && processingThreadCount <= 5, "0 < processingThreadCount && processingThreadCount <= 5");

            _logger = new Logger(collectorUri, authorizationToken, applicationName, applicationVersion, timeout);

            _queue = new BlockingCollection<LogItem>();
            
            StartProcessing(processingThreadCount);
        }

        /// <summary>Number of items to be processed</summary>
        public int PendingLogCount => _queue.Count;

        private void StartProcessing(int processingThreadCount)
        {
            Check.Argument(processingThreadCount > 0, "processingThreadCount > 0");
            
            if (HostingEnvironment.IsHosted)
            {
                for (var i = 0; i < processingThreadCount; i++)
                {
                    HostingEnvironment.QueueBackgroundWorkItem(t => UploadLogs(isTerminated: () => _isDisposed || t.IsCancellationRequested));
                }
                
                return;
            }

            bool isTerminated() => _isDisposed || Environment.HasShutdownStarted;

            for (var i = 0; i < processingThreadCount; i++)
            {
                Task.Factory.StartNew(action: () => UploadLogs(isTerminated), creationOptions: TaskCreationOptions.LongRunning);
            }
        }

        private void UploadLogs(Func<bool> isTerminated)
        {
            Check.ArgumentIsNull(isTerminated, nameof(isTerminated));

            Interlocked.Increment(ref _actualProcessingThreadCount);

            while (!isTerminated())
            {
                try
                {
                    if (!_queue.TryTake(out var logItem, millisecondsTimeout: QUEUE_READ_TIMEOUT))
                        continue;

                    Check.Assert(_logger.Log(logItem.Type, logItem.Message, logItem.Data, logItem.CorrelationId), "Log submit failed.");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString(), "ERROR");
                }
            }

            if (Interlocked.Decrement(ref _actualProcessingThreadCount) == 0)
                _queue.Dispose();
        }

        /// <summary>Submits log to Splunk</summary>
        /// <param name="type">Log type (Error, Info, etc.)</param>
        /// <param name="message">Log text message</param>
        /// <param name="data">Any object which serialized into JSON</param>
        /// <param name="correlationId">Correlation ID for synchronizing different messages</param>
        /// <remarks>If submitting to Splunk fails then log is submitted to Windows Event Log</remarks>
        public void Log(string type, string message, object data = null, string correlationId = null) => _queue.Add(new LogItem(type, message, data, correlationId));

        /// <summary>Dispaoses current object</summary>
        public void Dispose()
        {
            _isDisposed = true;
        }
    }
}
