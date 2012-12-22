
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace HuntingDog
{
    public class Log
    {
        private readonly Logger logger;

        internal Log(Type type)
        {
            logger = LogManager.GetLogger(type.FullName);
        }

        public void LogError(String msg, Exception ex)
        {
            logger.ErrorException(msg, ex);
        }

        public void LogError(String msg)
        {
            logger.Error(msg);
        }

        public void LogPerformace(String msg, Stopwatch timer)
        {
            if (timer.ElapsedMilliseconds > 1000)
            {
                LogMessage("Perf: " + msg
                    + String.Format("{0:0.00}", (Double) timer.ElapsedMilliseconds / 1000)
                    + " sec");
            }
            else
            {
                LogMessage("Perf: " + msg + " - " + timer.ElapsedMilliseconds + " ms");
            }
        }

        public void LogMessage(String msg)
        {
            logger.Info(msg);
        }
    }
}
