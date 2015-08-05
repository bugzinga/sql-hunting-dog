using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace HuntingDog.Core
{
    public class DogVersion
    {
        public DogVersion(Version version, string url)
        {
            _version = version;
            UrlToDownload = url;
        }

        Version _version;

        public Version Version
        {
            get
            {
                return _version;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}", _version.Major, _version.Minor);
        }

        public string UrlToDownload { get;set;}

        static Version _currentVersion;
        public static Version Current
        {
            get
            {
                if (_currentVersion == null)
                {
                    var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                    _currentVersion = new Version(currentVersion.Major, currentVersion.Minor);
                }

                return _currentVersion;
            }
        }

     
    }
}
