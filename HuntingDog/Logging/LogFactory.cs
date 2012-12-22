
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;

namespace HuntingDog
{
    public static class LogFactory
    {
        private const String DefaultLogFileName = "${basedir}/Logs/HuntingDog.log";

        private const String DefaultLogLayout = "[${logger}] ${message} ${exception:format=tostring}";

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

        public static Log GetLog(Type type)
        {
            if (!loggers.ContainsKey(type))
            {
                loggers[type] = new Log(type);
            }

            return loggers[type];
        }
    }
}
