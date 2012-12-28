
using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using System.Reflection;

namespace DatabaseObjectSearcher
{
    public class MSSQLController
    {
        public static MSSQLController Instance
        {
            get;
            private set;
        }

        public Window SearchWindow
        {
            get;
            private set;
        }

        static MSSQLController()
        {
            Instance = new MSSQLController();
        }

        public void CreateAddinWindow(AddIn addIn)
        {
            var assemblyName = Assembly.GetExecutingAssembly().FullName;
            var className = typeof(HuntingDog.ucHost).FullName;
            var caption = "Hunting Dog";
            var windowId = "Hunting Dog Tool Window";
            Object userControl = null;

            var windows = ServiceCache.ExtensibilityModel.Windows as Windows2;

            if (windows != null)
            {
                SearchWindow = windows.CreateToolWindow2(addIn, assemblyName, className, caption, windowId, ref userControl);
                SearchWindow.SetTabPicture(HuntingDog.Properties.Resources.footprint.GetHbitmap());
                SearchWindow.Visible = true;
            }
        }
    }
}
