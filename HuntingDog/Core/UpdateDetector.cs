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
            DetectedNewVersionButWillBeIgnored,
            DetectedSameVersion
        }

        const int MsInSecond = 1000;
        const int MsInHour = (3600*1000);

        const int TimerPeriodIfFailed = 10 * MsInSecond;
        const int TimerInitialPeriod = 5 * MsInSecond;
        const int TimerPeriodIfSameVersionDetected = 30 * MsInSecond ;             // 30 minutes
        const int TimerPeriodNewVersinWasDetected = 30 * MsInSecond;        //6 hours

        UpdateState CurrentState = UpdateState.NotStarted;

        VersionRetriever Retriever = new VersionRetriever();
        private static readonly Log log = LogFactory.GetLog();

        Timer timer;
        private DogVersion _newDogVersion;

        Version _versionToIgnore;

        public Version NewVersion
        {
            get
            {
                if(_newDogVersion!=null)                
                    return _newDogVersion.Version;

                return null;
            }
        }

        const string UrlWithUpdates = "http://localhost:82/some.txt";

        public event Action<DogVersion> NewVersionFound;

        public void StartDetection()
        {
            if(timer==null)
                timer = new Timer(timeForACheck, null, TimerInitialPeriod, Timeout.Infinite);
            else
                timer.Change(TimerInitialPeriod, Timeout.Infinite);
        }

        public void StopDetection()
        {
            timer.Dispose();
            timer = null;
        }        

        public void IgnoreVersion(Version version)
        {
            try
            {
                _versionToIgnore = version;
                StartDetection();
            }
            catch (Exception ex)
            {

            }
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
                if (_newDogVersion.Version == DogVersion.Current)
                {
                    CurrentState = UpdateState.DetectedSameVersion;
                }
                else if (_newDogVersion.Version > DogVersion.Current)
                {
                    if (_versionToIgnore != null &&_newDogVersion.Version <= _versionToIgnore)
                    {
                        CurrentState = UpdateState.DetectedNewVersionButWillBeIgnored;
                        log.Info("New Version was detected but will be ignored (user choice) :" + newVersionresult.RetrievedVersion);
                    }
                    else
                    {
                        CurrentState = UpdateState.DetectedNewVersion;
                        NotifyNewVersionFound(_newDogVersion);
                        log.Info("New Version was detected :" + newVersionresult.RetrievedVersion);
                    }
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
                    case UpdateState.DetectedNewVersionButWillBeIgnored:
                        timer.Change(TimerPeriodNewVersinWasDetected, Timeout.Infinite);
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

