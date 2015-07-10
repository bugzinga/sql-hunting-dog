using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace HuntingDog.Core
{

    public class UpdateDetector
    {
        enum UpdateState
        {
            NotStarted,
            FailedToDetect,
            DetectedNewVersion,
            DetectedSameVersion
        }

        UpdateState CurrentState = UpdateState.NotStarted;

        VersionRetriever Retriever = new VersionRetriever();
        private static readonly Log log = LogFactory.GetLog();

        Timer timer;
        private DogVersion _newDogVersion;

        const int TimerPeriodIfFailed = 10* 1000;

        const int TimerInitialPeriod = 2*1000;

        const int TimerPeriodIfSameVersionDetected =  30 * 1000;

        const string UrlWithUpdates = "http://localhost:82/some.txt";

        public event Action<DogVersion> NewVersionFound;

        public void StartDetection()
        {
            timer = new Timer(timeForACheck, null, TimerInitialPeriod, Timeout.Infinite);
            
        }

        public void StopDetection()
        {
            timer.Dispose();
        }

        public void Download()
        {
            try
            {
                if(_newDogVersion!=null)
                    Process.Start(_newDogVersion.UrlToDownload);
                else
                    log.Error("Download was invoked but new version is not known");
            }
            catch (Exception ex)
            {
                log.Error("Download failed", ex);
            }
        }

        private void Detect()
        {
            var newVersionresult =  Retriever.RetrieveVersion(UrlWithUpdates);
            if (!newVersionresult.IsRetrieved)
            {
                CurrentState = UpdateState.FailedToDetect;
            }
            else
            {
                _newDogVersion = newVersionresult.RetrievedVersion;
                if (_newDogVersion.IsGreaterThanCurrent())
                {
                    CurrentState = UpdateState.DetectedNewVersion;
                    log.Info("New Version was detected :" + newVersionresult.RetrievedVersion);
                    NotifyNewVersionFound(_newDogVersion);
                }
                else
                {
                    CurrentState = UpdateState.DetectedSameVersion;
                }
            }
        }

        private void NotifyNewVersionFound(DogVersion version)
        {
            try
            {
                if (NewVersionFound != null)
                    NewVersionFound(version);
            }
            catch (Exception ex)
            {
                log.Error("New Version Notification failed", ex);
            }
        }

        private void timeForACheck(object state)
        {
            try
            {
                Detect();

                switch (CurrentState)
                {
                    case UpdateState.NotStarted:
                    case UpdateState.FailedToDetect:
                        timer.Change(TimerPeriodIfFailed, Timeout.Infinite);            
                        break;
                    case UpdateState.DetectedSameVersion:
                        timer.Change(TimerPeriodIfSameVersionDetected, Timeout.Infinite);
                        break;
                    case UpdateState.DetectedNewVersion:
                        StopDetection();
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error("Timer Update detector failure", ex);
            }

        }

    }
}

