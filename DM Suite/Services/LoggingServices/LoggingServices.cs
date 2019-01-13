using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroLog;
using MetroLog.Targets;

namespace DM_Suite.Services.LoggingServices
{
    public class LoggingServices
    {
        public static LoggingServices Instance { get; }
        public static int RetainDays { get; } = 3; // 3 days
        public static bool Enabled { get; set; } = true;

        static LoggingServices()
        {
            // Implement singleton pattern
            Instance = Instance ?? new LoggingServices();

            // Set logging default config
            // public enum LogLevel
            /*      Trace = 0,
             *      Debug = 1,
             *      Info = 2,
             *      Warn = 3,
             *      Error = 4,
             *      Fatal = 5
             */

#if DEBUG
            LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new StreamingFileTarget { RetainDays = RetainDays });
#else
            LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Info, LogLevel.Fatal, new StreamingFileTarget { RetainDays = RetainDays });;
#endif
        }

        public void WriteLine<T>(string message, LogLevel logLevel = LogLevel.Trace, Exception exception = null)
        {
            if (Enabled)
            {
                var logger = LogManagerFactory.DefaultLogManager.GetLogger<T>();
                if (logLevel == LogLevel.Trace && logger.IsTraceEnabled)
                {
                    logger.Trace(message);
                }
                if (logLevel == LogLevel.Debug && logger.IsDebugEnabled)
                {
                    Debug.WriteLine($"{DateTime.Now.TimeOfDay.ToString()} {message}");
                    logger.Debug(message);
                }
                if (logLevel == LogLevel.Error && logger.IsErrorEnabled)
                {
                    logger.Error(message);
                }
                if (logLevel == LogLevel.Fatal && logger.IsFatalEnabled)
                {
                    logger.Fatal(message);
                }
                if (logLevel == LogLevel.Info && logger.IsInfoEnabled)
                {
                    logger.Info(message);
                }
                if (logLevel == LogLevel.Warn && logger.IsWarnEnabled)
                {
                    logger.Warn(message);
                }
            }
        }
    }
}
