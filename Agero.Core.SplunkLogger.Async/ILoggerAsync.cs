using System;

namespace Agero.Core.SplunkLogger.Async
{
    /// <summary>Logger which supports asynchronous message submit</summary>
    public interface ILoggerAsync: IDisposable
    {
        /// <summary>Submits log to Splunk</summary>
        /// <param name="type">Log type (Error, Info, etc.)</param>
        /// <param name="message">Log text message</param>
        /// <param name="data">Any object which serialized into JSON</param>
        /// <param name="correlationId">Correlation ID for synchronizing different messages</param>
        /// <remarks>If submitting to Splunk fails then log is submitted to Windows Event Log</remarks>
        void Log(string type, string message, object data = null, string correlationId = null);
    }
}
