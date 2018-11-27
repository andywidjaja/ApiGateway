using System;
using System.Text;
using System.Threading;

//using Common.Logging;
using EasyNetQ;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using PTrust.LogMessages;

namespace PTrust.Services.ShapeManagerApiGateway
{
    public class PtLogger : IPtLogger, IDisposable
    {
        private static Lazy<IBus> _lazyBus;

        private readonly string _serviceId;

        private readonly string _logRoutingKey;

        private bool _disposed;

        //private static readonly ILog Log = LogManager.GetLogger<PtLogger>();

        public PtLogger(IOptions<PtLoggerSettings> ptLoggerSettings)
        {
            _lazyBus = new Lazy<IBus>(() => RabbitHutch.CreateBus(ptLoggerSettings.Value.EndPoint));
            _logRoutingKey = ptLoggerSettings.Value.LogRoutingKey;
            _serviceId = ptLoggerSettings.Value.ServiceId;
        }

        ~PtLogger()
        {
            Dispose();
        }

        public void Dispose()
        {
            lock (this)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                Bus.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        public void LogDebug(string logText)
        {
            //Log.Debug(logText);            
            try
            {
                Bus.Publish(new LogMessage {ServiceId = _serviceId, ContainerId = Environment.MachineName, ThreadId = Thread.CurrentThread.ManagedThreadId.ToString(), LogType = LogType.Debug, Timestamp = DateTime.Now, Message = logText}, _logRoutingKey);
            }
            catch (Exception ex)
            {
                //Log.Error($"Log publish failed: {logText}", ex);
            }
        }

        public void LogError(string logText)
        {
            //Log.Error(logText);
            try
            {
                Bus.Publish(new LogMessage { ServiceId = _serviceId, ContainerId = Environment.MachineName, ThreadId = Thread.CurrentThread.ManagedThreadId.ToString(), LogType = LogType.Error, Timestamp = DateTime.Now, Message = logText }, _logRoutingKey);
            }
            catch (Exception ex)
            {
                //Log.Error($"Log publish failed: {logText}", ex);
            }            
        }

        public void LogError(string logText, Exception exception)
        {
            //Log.Error(logText, exception);
            var errorMsg = new StringBuilder(logText);
            if (!string.IsNullOrEmpty(exception.StackTrace))
            {
                errorMsg.Append(Environment.NewLine + exception.StackTrace);
            }

            try
            {
                Bus.Publish(new LogMessage { ServiceId = _serviceId, ContainerId = Environment.MachineName, ThreadId = Thread.CurrentThread.ManagedThreadId.ToString(), LogType = LogType.Error, Timestamp = DateTime.Now, Message = errorMsg.ToString() }, _logRoutingKey);
            }
            catch (Exception ex)
            {
                //Log.Error($"Log publish failed: {logText}", ex);
            }
        }

        public void LogInfo(string logText)
        {
            //Log.Info(logText);
            try
            {
                Bus.Publish(new LogMessage { ServiceId = _serviceId, ContainerId = Environment.MachineName, ThreadId = Thread.CurrentThread.ManagedThreadId.ToString(), LogType = LogType.Info, Timestamp = DateTime.Now, Message = logText }, _logRoutingKey);
            }
            catch (Exception ex)
            {
                //Log.Error($"Log publish failed: {logText}", ex);
                Console.WriteLine($"Log publish failed: {logText}");
            }            
        }

        public void LogWarn(string logText)
        {
            //Log.Debug(logText);
            try
            {
                Bus.Publish(new LogMessage { ServiceId = _serviceId, ContainerId = Environment.MachineName, ThreadId = Thread.CurrentThread.ManagedThreadId.ToString(), LogType = LogType.Warning, Timestamp = DateTime.Now, Message = logText }, _logRoutingKey);
            }
            catch (Exception ex)
            {
                //Log.Error($"Log publish failed: {logText}", ex);
            }
        }

        public static IBus Bus => _lazyBus.Value;
    }
}