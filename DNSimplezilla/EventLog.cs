using System;
using log4net;
using log4net.Core;

namespace DNSimple.UpdateService
{
    public static class EventLog
    {
        private static readonly ILog DefaultLogger = LogManager.GetLogger("DefaultLogger");

        public static void Info(string message, EventId eventId = EventId.Info)
        {
            Log(message, eventId, Level.Info, null);
        }

        public static void Warn(string message, EventId eventId = EventId.Warning)
        {
            Log(message, eventId, Level.Warn, null);
        }

        public static void Warn(string message, Exception ex, EventId eventId = EventId.Warning)
        {
            Log(message, eventId, Level.Warn, ex);
        }

        public static void Error(string message, Exception ex, EventId eventId = EventId.Error)
        {
            Log(message, eventId, Level.Error, ex);
        }

        private static void Log(string message, EventId eventId, Level level, Exception ex)
        {
            var loggingEvent = new LoggingEvent(typeof(EventLog), DefaultLogger.Logger.Repository, DefaultLogger.Logger.Name, level, message ?? string.Empty, ex);
            loggingEvent.Properties["EventID"] = (int)eventId;

            DefaultLogger.Logger.Log(loggingEvent);
        }
    }
}
