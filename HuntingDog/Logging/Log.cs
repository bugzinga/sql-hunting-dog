
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

        public void Error(String msg, Exception ex)
        {
            logger.ErrorException(msg, ex);
        }

        public void Error(String msg)
        {
            logger.Error(msg);
        }

        public void Performance(String msg, Stopwatch timer)
        {
            String value;
            String postfix;

            if (timer.ElapsedMilliseconds > 1000)
            {
                value = String.Format("{0:0.00}", (Double) timer.ElapsedMilliseconds / 1000);
                postfix = "sec";
            }
            else
            {
                value = timer.ElapsedMilliseconds.ToString();
                postfix = "ms";
            }

            Message(String.Format("Performance: {0} - {1} {2}", msg, value, postfix));
        }

        public void Message(String msg)
        {
            logger.Info(msg);
        }
    }
}
