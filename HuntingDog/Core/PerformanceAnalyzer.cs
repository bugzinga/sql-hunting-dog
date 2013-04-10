
using System;
using System.Diagnostics;

namespace HuntingDog.Core
{
    public class PerformanceAnalyzer
    {
        private readonly Log log = LogFactory.GetLog();

        private readonly Object synchronizer = new Object();

        private Stopwatch watcher = new Stopwatch();

        public PerformanceAnalyzer()
        {
            Start();
        }

        ~PerformanceAnalyzer()
        {
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
            lock (synchronizer)
            {
                if (!watcher.IsRunning)
                {
                    log.Error("Cannot start: analyzer is still working");
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
