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
        public const String UserPref_IgnoredVersion = "Ignored Update";
        public const String Url_ToCheckUpdates = "http://www.sql-hunting-dog.com/update.txt";

        const int SecInHour = 3600;
        const int TimerInitialPeriod = 5;
        const int TimerPeriodIfSameVersionDetected = 30 * SecInHour;             // 30 minutes
        const int TimerPeriodNewVersinWasDetected = 30 * SecInHour;        //6 hours

        private static readonly Log log = LogFactory.GetLog();
        private object _door = new object();
        private DogVersion _newDogVersion;

        Version _versionToIgnore;

        public event Action<DogVersion> NewVersionFound;  

        UpdateNotificator UpdateNotificator = new UpdateNotificator();
        HuntingDog.DogEngine.ISavableStorage Storage {  get;  set; }
        public UpdateDetector(HuntingDog.DogEngine.ISavableStorage storage)
        {
            Storage = storage;
            _versionToIgnore = RetreiveIgnoredVersion();
            UpdateNotificator.Start(Url_ToCheckUpdates, TimerInitialPeriod, OnNewVersion);
        }

        void OnNewVersion(DogVersion v)
        {
            try
            {
                lock (_door)
                {
                    _newDogVersion = v;
                }

                if (_versionToIgnore == null || v.Version > _versionToIgnore)
                {
                    UpdateNotificator.ChangePeriod(TimerPeriodNewVersinWasDetected);
                    NotifyNewVersionFound(v);    
                }              
                else
                {
                    UpdateNotificator.ChangePeriod(TimerPeriodIfSameVersionDetected);
                }

            }
            catch(Exception ex)
            {
                log.Error("On new version failed",ex);
            }
        }

        Version RetreiveIgnoredVersion()
        {
            try
            {
                var ignoredVersion = Storage.GetByName(UserPref_IgnoredVersion);
                if (!string.IsNullOrEmpty(ignoredVersion))
                {
                    return new Version(ignoredVersion);
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed to retreive ignored Version");
            }
            return null;
        }


        void StoreIgnoredVersion()
        {
            try
            {
                Storage.StoreByName(UserPref_IgnoredVersion, _versionToIgnore.ToString());
                Storage.Save();
            }
            catch (Exception ex)
            {
                log.Error("Failed to Store ignored Version");
            }        
        }

        public void StopDetection()
        {
            UpdateNotificator.Stop();
        }        

        public void IgnoreVersion()
        {
            try
            {
                lock (_door)
                {
                    if (_newDogVersion != null)
                    {
                        _versionToIgnore = _newDogVersion.Version;
                    }

                }

                StoreIgnoredVersion();
              
            }
            catch (Exception ex)
            {
                log.Error("Ignore Version failed", ex);
            }
        }

        public void Download()
        {
            try
            {
                lock (_door)
                {
                    if (_newDogVersion != null)
                        Process.Start(_newDogVersion.UrlToDownload);
                    else
                        log.Error("Download was invoked but new version is not known");
                }
            }
            catch (Exception ex)
            {
                log.Error("Download failed", ex);
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

     

        public Action<DogVersion> NewVersion { get; set; }
    }
}

