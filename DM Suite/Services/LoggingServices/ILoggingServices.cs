using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroLog;

namespace DM_Suite.Services.LoggingServices
{
    public interface ILoggingServices
    {
        void WriteLine<T>(string message, LogLevel logLevel = LogLevel.Trace, Exception exception = null);
    }
}
