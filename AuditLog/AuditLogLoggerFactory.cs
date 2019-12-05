using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AuditLog
{
    public static class AuditLogLoggerFactory
    {
        public static ILoggerFactory LoggerFactory = new NullLoggerFactory();
        public static ILogger<T> CreateInstance<T>() => LoggerFactory.CreateLogger<T>();
        public static ILogger CreateLogger(string categoryName) => LoggerFactory.CreateLogger(categoryName);
    }
}