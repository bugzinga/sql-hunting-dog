using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Reflection;

namespace HuntingDog.Core
{
    public enum EVersionOutcome
    {
        AlreadyLates,
        NewVersionAvailable,
        Failed
    }
    public class UpdateResult
    {
        public EVersionOutcome Outcome { get;  set; }
        public string NewVersion { get; set; }
        public string CurrentVersion { get; set; }
        public string UrlToDownload { get; set; }
    }

    public class VersionChecker
    {
        Version _version;
        public Version Current
        {
            get
            {
                if (_version == null)
                {
                    _version = Assembly.GetExecutingAssembly().GetName().Version;
                }

                return _version;
            }
        }

        private static string FormatVersion(Version vr)
        {
            return string.Format("{0}.{1}",vr.Major,vr.Minor);
        }

        public UpdateResult DetectNewVersion()
        {
            var newVersion = RetrieveVersion("http://sql-hunting-dog.com/version.txt");
            if (newVersion == null)
                return new UpdateResult { Outcome = EVersionOutcome.Failed, CurrentVersion = FormatVersion(Current) };

            if (VersionIsGreaterThanCurrent(newVersion))
            {
                return new UpdateResult { Outcome = EVersionOutcome.NewVersionAvailable, CurrentVersion = FormatVersion(Current), NewVersion = newVersion };
            }

            return new UpdateResult { Outcome = EVersionOutcome.AlreadyLates, CurrentVersion = FormatVersion(Current) };
        }

        private bool VersionIsGreaterThanCurrent(string newVersion)
        {
            var latestVersion = ParseVersion(newVersion);
            if(latestVersion!=null)
                return latestVersion.Minor > Current.Minor || latestVersion.Major > Current.Major;

            return false;
        }

        public Version ParseVersion(string version)
        {
            try
            {
                var digits = version.Split(',');
                if (digits.Length >= 2)
                {
                    return new Version(int.Parse(digits[0]), int.Parse(digits[1]));
                }
            }
            catch (Exception ex)
            {
                // TODO: log the fatc that version was corrupted
            }
            return null;
        }
 
        public string RetrieveVersion(string url)
        {
            try
            {

                string content = string.Empty;
                using (WebClient client = new WebClient())
                {
                    using (Stream stream = client.OpenRead(url))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }                    
                    }
                }

            }
            catch(Exception ex)
            {
                //TODO: call is blocked by firewall or web site is down. try later
            }

            return null;
        }

    };
  
}
