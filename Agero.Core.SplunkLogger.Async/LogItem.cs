using Agero.Core.Checker;

namespace Agero.Core.SplunkLogger.Async
{
    internal class LogItem
    {
        public LogItem(string type, string message, object data = null, string correlationId = null)
        {
            Check.ArgumentIsNullOrWhiteSpace(type, nameof(type));
            Check.ArgumentIsNullOrWhiteSpace(message, nameof(message));

            Type = type;
            Message = message;
            Data = data;
            CorrelationId = correlationId;
        }

        public string Type { get; }

        public string Message { get; }

        public object Data { get; }

        public string CorrelationId { get; }
    }
}
