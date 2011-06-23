using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
//using Microsoft.SqlServer.Management.Smo.RegSvrEnum;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Reflection;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using System.Threading;
using System.Text.RegularExpressions;


namespace DatabaseObjectSearcher
{
    public class NavigatorServer
    {
        public SqlConnectionInfo ConnInfo{get;set;}
        public string Name{get;set;}

        public DbObjectSearcher DbSearcher { get; private set; }

        public NavigatorServer(SqlConnectionInfo ci, string name)
        {
            Name = name;
            ConnInfo = ci;
            DbSearcher = new DbObjectSearcher(ci);

            // prefetch all databases from server
            DbSearcher.BuilDataBaseDictionary();
        }
   
    }


    public class MSSQLController
    {
        static MSSQLController currentInstance = new MSSQLController();

       // private DTE2 _application;
       
        private Dictionary<string, DbObjectSearcher> searcherDictionary = new Dictionary<string, DbObjectSearcher>();

        private EnvDTE.Window toolWindow;
        DatabaseObjectSearcher.ObjectExplorerManager manager = new DatabaseObjectSearcher.ObjectExplorerManager();

        public static MSSQLController Current
        {
            get { return currentInstance; }
        }

        public List<NavigatorServer> ServerList { get; private set; }

        //public void LoadExistingServers()
        //{
        //    ServerList = new List<NavigatorServer>();

        //    Exception lastEx = null;

        //    foreach (var srvConnectionInfo in ObjectExplorerManager.GetAllServers())
        //    {
        //        try
        //        {
        //            var nvServer = new NavigatorServer(srvConnectionInfo, srvConnectionInfo.ServerName);
        //            ServerList.Add(nvServer);
        //        }
        //        catch (Exception ex)
        //        {
        //            lastEx = ex;
        //        }
        //    }

        //    if (lastEx != null)
        //        throw lastEx;
        //} 
    
   

        //private INodeInformation[] GetObjectExplorerSelectedNodes()
        //{
        //    IObjectExplorerService objExplorer = ServiceCache.GetObjectExplorer();
        //    int arraySize;
        //    INodeInformation[] nodes;
        //    objExplorer.GetSelectedNodes(out arraySize, out nodes);
        //    return nodes;
        //}


        public void SelectSMOObject(NamedSmoObject objectToSelect, SqlConnectionInfo connection)
        {
           
            manager.SelectSMOObjectInObjectExplorer(objectToSelect, connection); 
            
        }

        public EnvDTE.Window SearchWindow
        {
            get { return toolWindow; }
        }
        
        public void CreateAddinWindow(AddIn addinInstance)
        {
            Assembly asm = Assembly.Load("HuntingDog");
           // Guid id = new Guid("4c410c93-d66b-495a-9de2-99d5bde4a3b9"); // this guid doesn't seem to matter?
            //toolWindow = CreateToolWindow("DatabaseObjectSearcherUI.ucMainControl", asm.Location, id,  addinInstance);

            Guid id = new Guid("4c410c93-d66b-495a-9de2-99d5bde4a3b8"); // this guid doesn't seem to matter?
            toolWindow = CreateToolWindow("HuntingDog.ucHost", asm.Location, id,  addinInstance);
       

            //if (_application == null)
            //{
            //    _application = application;
            //}
        }

        private EnvDTE.Window CreateToolWindow(string typeName, string assemblyLocation, Guid uiTypeGuid, AddIn addinInstance)
        {
            Windows2 win2 = ServiceCache.ExtensibilityModel.Windows as Windows2;
            //Windows2 win2 = applicationObject.Windows as Windows2;
            if (win2 != null)
            {
                object controlObject = null;
                Assembly asm = Assembly.GetExecutingAssembly();
                EnvDTE.Window toolWindow = win2.CreateToolWindow2(addinInstance, assemblyLocation, typeName, "Hunting Dog", "{" + uiTypeGuid.ToString() + "}", ref controlObject);

              
                EnvDTE.Window oe = null;
                foreach (EnvDTE.Window w1 in addinInstance.DTE.Windows)
                {
                    if (w1.Caption == "Object Explorer")
                    {
                        oe = w1;
                        //if(oe.LinkedWindows!=null)
                        //    oe.LinkedWindows.Add(toolWindow);  
                    }
                }

                
             
               

                
                //toolWindow.Width = oe.Width;
               // toolWindow.SetKind((vsWindowType)oe.Kind);
               // toolWindow.IsFloating = oe.IsFloating;

           
               // oe.LinkedWindows.Add(toolWindow);


                //Window frame = win2.CreateLinkedWindowFrame(toolWindow,oe, vsLinkedWindowType.vsLinkedWindowTypeHorizontal);

                //frame.SetKind(vsWindowType.vsWindowTypeDocumentOutline);

                //addinInstance.DTE.MainWindow.LinkedWindows.Add(frame);

              
                //frame.Activate();
                //HuntingDog.Properties.Resources.spider1.MakeTransparent(System.Drawing.Color.FromArgb(0, 255, 0));

                //stdole.IPicture tabPic = Support.ImageToIPicture(img) as stdole.IPicture;

                toolWindow.SetTabPicture(HuntingDog.Properties.Resources.footprint.GetHbitmap());
                toolWindow.Visible = true;

               // toolWindow.Linkable = true;
              //  toolWindow.IsFloating = false;

            //    addinInstance.DTE.MainWindow.LinkedWindows.Add(toolWindow);
                //if (oe != null)
                //{

                //    if (toolWindow.LinkedWindowFrame == null)
                //    {
                //        if (oe.LinkedWindowFrame != null)
                //        {
                //            oe.LinkedWindowFrame.LinkedWindows.Add(toolWindow);
                //        }
                //        else
                //        {
                //            toolWindow.Left = oe.Left;
                //            toolWindow.Top = oe.Top;
                //            toolWindow.Width = oe.Width;
                //            toolWindow.Height = oe.Height;

                //            Window2 winFrame = (Window2)win2.CreateLinkedWindowFrame(oe, toolWindow, vsLinkedWindowType.vsLinkedWindowTypeHorizontal);
                //        }
                //    }

                //    //winFrame.SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE.VSFM_MdiChild);  
                //    //winFrame.Linkable = true;
                //    //winFrame.IsFloating = false;
                //}

                return toolWindow;
            }
            return null;
        }
    }
}
