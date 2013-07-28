using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseObjectSearcher;

namespace HuntingDog.DogEngine.Impl
{
    public class DiConstruct
    {
        static object _lockConcurrentAccess  = new object();
        private static StudioController Build()
        {
            var manager = new ObjectExplorerManager();
            var srvWatcher = new ServerWatcher(manager);
            var sudioCtrl = new StudioController(manager,srvWatcher);
            return sudioCtrl;
        }

        static StudioController _instance = null;
        public static StudioController Instance
        {
            get
            {
                lock(_lockConcurrentAccess)
                {
                    if (_instance == null)
                    {
                        _instance = DiConstruct.Build();
                    }
                }

                return _instance;
            }
          
        }

    }
}
