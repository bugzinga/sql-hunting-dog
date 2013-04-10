
using System;
using System.Diagnostics;

namespace HuntingDog.Core
{
    public class PerformanceAnalyzer
    {
        private readonly Boolean disabled = false;

        private readonly Log log = LogFactory.GetLog();

        private readonly Object synchronizer = new Object();

        private Stopwatch watcher = new Stopwatch();

        public PerformanceAnalyzer()
        {
            if (disabled)
            {
                return;
            }

            Start();
        }

        ~PerformanceAnalyzer()
        {
            if (disabled)
            {
                return;
            }

            lock (synchronizer)
            {
                if (watcher.IsRunning)
                {
                    watcher.Stop();
                }
            }
        }

        public void Start()
        {
            if (disabled)
            {
                return;
            }

            lock (synchronizer)
            {
                if (watcher.IsRunning)
                {
                    log.Error("Cannot start: analyzer is still working");
                    return;
                }

                watcher.Reset();
                watcher.Start();
            }
        }

        public Stopwatch Stop()
        {
            if (disabled)
            {
                return Result;
            }

            lock (synchronizer)
            {
                if (!watcher.IsRunning)
                {
                    log.Error("Cannot stop: analyzer is not working");
                    return null;
                }

                watcher.Stop();

                return Result;
            }
        }

        public Stopwatch Result
        {
            get
            {
                lock (synchronizer)
                {
                    return watcher;
                }
            }
        }
    }
}
