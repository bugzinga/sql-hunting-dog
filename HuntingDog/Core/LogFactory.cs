
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HuntingDog.Core
{
    public static class LogFactory
    {
        private const String DefaultLogFileName = "${basedir}/Logs/HuntingDog.log";

        private const String DefaultLogLayout = "${longdate} ${level:uppercase=true:padding=-5} [${logger:shortName=true}] ${message} ${exception:format=tostring}";

        private static readonly Dictionary<Type, Log> loggers = new Dictionary<Type, Log>();

        static LogFactory()
        {
            var config = new LoggingConfiguration();

            var target = new FileTarget();
            target.FileName = DefaultLogFileName;
            target.Layout = DefaultLogLayout;
            config.AddTarget("file", target);

            var rule = new LoggingRule("*", LogLevel.Trace, target);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;
        }

        public static Log GetLog(Type type = null)
        {
            if (type == null)
            {
                var stackTrace = new StackTrace(1);
                var callerFrame = stackTrace.GetFrame(0);

                foreach (var frame in stackTrace.GetFrames())
                {
                    if (!frame.GetMethod().IsConstructor)
                    {
                        break;
                    }

                    callerFrame = frame;
                }

                type = callerFrame.GetMethod().ReflectedType;
            }
            
            if (!loggers.ContainsKey(type))
            {
                loggers[type] = new Log(type);
            }

            return loggers[type];
        }
    }
}
