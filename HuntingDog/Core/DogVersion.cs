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
        static Version _currentVersion;

        public override string ToString()
        {
            return string.Format("{0}.{1}", _version.Major, _version.Minor);
        }

        public string UrlToDownload { get;set;}


        public Version Current
        {
            get
            {
                if (_currentVersion == null)
                {
                    _currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                }

                return _currentVersion;
            }
        }


        public bool IsGreaterThanCurrent()
        {
            return _version.Minor > Current.Minor || _version.Major > Current.Major;
        }
    }
}
