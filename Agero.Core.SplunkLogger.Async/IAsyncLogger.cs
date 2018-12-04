using System;

namespace Agero.Core.SplunkLogger.Async
{
    /// <summary>Asynchronous Splunk Logger</summary>
    public interface IAsyncLogger: IDisposable
    {
        /// <summary>Submits log to Splunk asynchronously</summary>
        /// <param name="type">Log type (Error, Info, etc.)</param>
        /// <param name="message">Log text message</param>
        /// <param name="data">Any object which will be serialized into JSON</param>
        /// <param name="correlationId">Any optional string which can correlate different logs</param>
        void Log(string type, string message, object data = null, string correlationId = null);
    }
}
