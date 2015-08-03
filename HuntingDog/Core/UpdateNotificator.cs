using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace HuntingDog.Core
{
    /// <summary>
    /// Detects new version from specific URL by polling in background
    /// </summary>
    public class UpdateNotificator
    {
        const int TimerInitialPeriod = 5 * 1000;

        Timer timer;
        VersionRetriever Retriever = new VersionRetriever();
        private static readonly Log log = LogFactory.GetLog();

        public Action<DogVersion> _onDetection;

        string _urlToCheckUpdate;
        object _singleExecutionOnly = new object();

        public void Start(string urlToCheckUpdate, int periodInSeconds, Action<DogVersion> onDetection)
        {
            _urlToCheckUpdate = urlToCheckUpdate;
            _onDetection = onDetection;
            timer = new Timer(timeForACheck, null, periodInSeconds * 1000, periodInSeconds * 1000);
        }

        public void ChangePeriod(int seconds)
        {
            if (timer == null)
                throw new Exception("not in Started state.");

            timer.Change(seconds * 1000, seconds * 1000);
        }

        public void Stop()
        {
            if(timer!=null)
                timer.Dispose();
            timer = null;
        }


        private void timeForACheck(object state)
        {
            try
            {
                lock (_singleExecutionOnly)
                {
                    var result = Retriever.RetrieveVersion(_urlToCheckUpdate);
               
                    if (result.IsRetrieved)
                    {
                        log.Info("Retrieved a new version " + result.RetrievedVersion);
                        _onDetection(result.RetrievedVersion);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("UpdateNotificator failure", ex);
            }
        }

    }
}
