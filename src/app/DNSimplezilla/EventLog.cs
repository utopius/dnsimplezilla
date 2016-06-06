using System;
using log4net;
using log4net.Core;

namespace DNSimplezilla
{
    public class EventLog : IEventLog
    {
        private static readonly ILog DefaultLogger = LogManager.GetLogger("DefaultLogger");

        public void Info(string message, EventId eventId = EventId.Info)
        {
            Log(message, eventId, Level.Info, null);
        }

        public void Warn(string message, EventId eventId = EventId.Warning)
        {
            Log(message, eventId, Level.Warn, null);
        }

        public void Warn(string message, Exception ex, EventId eventId = EventId.Warning)
        {
            Log(message, eventId, Level.Warn, ex);
        }

        public void Error(string message, Exception ex, EventId eventId = EventId.Error)
        {
            Log(message, eventId, Level.Error, ex);
        }

        public void Error(string message, EventId eventId = EventId.Error)
        {
            Log(message, eventId, Level.Error, null);
        }

        private void Log(string message, EventId eventId, Level level, Exception ex)
        {
            var loggingEvent = new LoggingEvent(typeof(EventLog), DefaultLogger.Logger.Repository, DefaultLogger.Logger.Name, level, message ?? string.Empty, ex);
            loggingEvent.Properties["EventID"] = (int)eventId;

            DefaultLogger.Logger.Log(loggingEvent);
        }
    }

    public interface IEventLog
    {
        void Info(string message, EventId eventId = EventId.Info);
        void Warn(string message, EventId eventId = EventId.Warning);
        void Warn(string message, Exception ex, EventId eventId = EventId.Warning);
        void Error(string message, Exception ex, EventId eventId = EventId.Error);
        void Error(string message, EventId eventId = EventId.Error);
    }
}
