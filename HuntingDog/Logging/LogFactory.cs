
using System;
using System.Collections.Generic;
using System.Diagnostics;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace HuntingDog
{
    public static class LogFactory
    {
        private static readonly Dictionary<Type, Log> loggers = new Dictionary<Type, Log>();

        static LogFactory()
        {
            //LogManager.ThrowExceptions = true;

            // Step 1. Create configuration object 
            LoggingConfiguration config = new LoggingConfiguration();

            FileTarget fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            // Step 3. Set target properties 
            fileTarget.FileName = "${basedir}/Logs/HuntingDog.log";

            fileTarget.Layout = "[${logger}] ${message} ${exception:format=tostring}";

            LoggingRule rule2 = new LoggingRule("*", LogLevel.Trace, fileTarget);
            config.LoggingRules.Add(rule2);

            // Step 5. Activate the configuration
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
