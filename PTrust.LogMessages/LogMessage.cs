using System;

namespace PTrust.LogMessages
{
    public class LogMessage
    {
        public string ContainerId { get; set; }

        public LogType LogType { get; set; }

        public string Message { get; set; }

        public string ServiceId { get; set; }

        public string ThreadId { get; set; }

        public DateTime Timestamp { get; set; }
    }    
}