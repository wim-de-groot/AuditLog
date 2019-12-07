using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AuditLog
{
    [ExcludeFromCodeCoverage]
    public static class AuditLogLoggerFactory
    {
        private static ILoggerFactory _loggerFactory = new NullLoggerFactory();
        public static ILoggerFactory LoggerFactory
        {
            internal get => _loggerFactory;
            set
            {
                if (_loggerFactory is NullLoggerFactory)
                {
                    _loggerFactory = value;
                }
                else
                {
                    throw new InvalidOperationException("Logger factory has already been set");
                }
            }
        }
        public static ILogger<T> CreateInstance<T>() => LoggerFactory.CreateLogger<T>();
        public static ILogger CreateLogger(string categoryName) => LoggerFactory.CreateLogger(categoryName);
    }
}