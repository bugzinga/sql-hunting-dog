
using NLog;
using System;
using System.Diagnostics;

namespace HuntingDog
{
    public class Log
    {
        private readonly Logger logger;

        internal Log(Type type)
        {
            logger = LogManager.GetLogger(type.FullName);
        }

        public void Message(String message)
        {
            logger.Info(message);
        }

        public void Error(String message, Exception exception)
        {
            logger.ErrorException(message, exception);
        }

        public void Error(String message)
        {
            logger.Error(message);
        }

        public void Performance(String operation, Stopwatch timer)
        {
            String time;
            String postfix;

            if (timer.ElapsedMilliseconds > 1000)
            {
                time = String.Format("{0:0.00}", (Double) timer.ElapsedMilliseconds / 1000);
                postfix = "sec";
            }
            else
            {
                time = timer.ElapsedMilliseconds.ToString();
                postfix = "ms";
            }

            Message(String.Format("Performance: {0} - {1} {2}", operation, time, postfix));
        }
    }
}
