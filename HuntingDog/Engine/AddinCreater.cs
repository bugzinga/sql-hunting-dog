
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using DatabaseObjectSearcher;
using EnvDTE;
using EnvDTE80;
using HuntingDog.Core;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.UI.VSIntegration;

namespace HuntingDog.DogEngine
{
    public class AddinCreater
    {
        private AddIn addIn;

        private static readonly String windowId = "{7146B360-D37D-44A1-8D4C-5E7E36EA81D4}";

        private readonly Log log = LogFactory.GetLog();

        private EnvDTE.Window SearchWindow
        {
            get;
            set;
        }

        public EnvDTE.Window CreateAddinWindow(AddIn addIn, string caption)
        {
            try
            {
                this.addIn = addIn;

                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var className = typeof(HuntingDog.ucHost).FullName;             
                Object userControl = null;

                var windows = ServiceCache.ExtensibilityModel.Windows as Windows2;

                if (windows != null)
                {
                    if ((SearchWindow == null) || (windows.Item(windowId) == null))
                    {
                        SearchWindow = windows.CreateToolWindow2(addIn, assemblyLocation, className, caption, windowId, ref userControl);
                        SearchWindow.SetTabPicture(HuntingDog.Properties.Resources.footprint.GetHbitmap());
                        HuntingDog.DogEngine.Impl.DiConstruct.Instance.HideYourself += Instance_HideYourself; ;
                    }

                    SearchWindow.Visible = true;
                }

                return SearchWindow;
            }
            catch (Exception ex)
            {
                log.Error("AddIn window could not be created", ex);
                throw;
            }
        }

        private void Instance_HideYourself()
        {
            if (SearchWindow != null)
                SearchWindow.Visible = false;
        }

    }
}
