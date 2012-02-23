using System;
using System.Collections.Generic;
using System.Text;
using HuntingDog;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
//using Microsoft.SqlServer.Management.Smo.RegSvrEnum;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Reflection;
using System.Windows;
//using EnvDTE100;
//using EnvDTE;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections;

namespace DatabaseObjectSearcher
{
    public class ObjectExplorerManager
    {

        //public  List<NavigatorServer> GetServers()
        //{
        //    var r = new List<NavigatorServer>();
        //    foreach (var srvConnectionInfo in GetAllServers())
        //    {
        //        var nvServer = new NavigatorServer(srvConnectionInfo, srvConnectionInfo.ServerName);
        //        r.Add(nvServer);
        //    }
        //    return r;
        //}



        public void Init()
        {
            try
            {
                IObjectExplorerService objectExplorer = ServiceCache.GetObjectExplorer();
                var provider = (IObjectExplorerEventProvider)objectExplorer.GetService(typeof(IObjectExplorerEventProvider));

                provider.SelectionChanged += new NodesChangedEventHandler(provider_SelectionChanged);
            }
            catch (Exception ex)
            {
                // NEED TO LOG
                MyLogger.LogError("Error Initialising object explorer (subscribing selection changed event) " + ex.Message, ex);
            }

            try
            {
              //  System.Threading.Thread.Sleep(80 * 1000);
                var cmdEvents = (EnvDTE.CommandEvents)ServiceCache.ExtensibilityModel.Events.get_CommandEvents("{00000000-0000-0000-0000-000000000000}", 0);
                cmdEvents.AfterExecute += this.AfterExecute;
            }
            catch (Exception ex)
            {

                MyLogger.LogError("Error Initialising object explorer  (subscribing command event)" + ex.Message, ex);
            }
        }

        public event Action<string> OnNewServerConnected;
        public event Action OnServerDisconnected;

        void provider_SelectionChanged(object sender, NodesChangedEventArgs args)
        {
            try
            {
                foreach (var n in args.ChangedNodes)
                {
                    if (n.Parent == null)
                    {
                        MyLogger.LogMessage("New Server Connected " + n.Name + " -  " + n.Connection.ServerName);
                        // this could mean that new server was added
                        var res = " server " + n.Name + n.Connection.ServerName;
                        if (OnNewServerConnected != null)
                            OnNewServerConnected(n.Name);
                    }
                } 

            }
            catch(Exception ex)
            {
                MyLogger.LogError("Error processing OnSelectionChanged event: " + ex.Message, ex);
            }
       
           

           
        }

        public void AfterExecute(string Guid, int ID, object CustomIn, object CustomOut)
        {
            MyLogger.LogMessage("After execute command:" + ID + " guid:" + Guid);

            // this coul mean that server was removed
            if (ID == 516)
            {
                MyLogger.LogMessage("Server disconnected..!");
                if (OnServerDisconnected != null)
                    OnServerDisconnected();
            }
        }

        private IObjectExplorerService _oe;
        // return all existing servers in hierarchy
        public  List<SqlConnectionInfo> GetAllServers()
        {
            List<SqlConnectionInfo> servers = new List<SqlConnectionInfo>();

            if(_oe==null)
            {
                _oe = ServiceCache.GetObjectExplorer();
               
            }

            Type t = _oe.GetType();
            FieldInfo getHierarchyMethod = t.GetField("hierarchies", BindingFlags.Instance | BindingFlags.NonPublic);
            var connHT = (Hashtable)getHierarchyMethod.GetValue(_oe);

            foreach (IExplorerHierarchy srvHerarchy in connHT.Values)
            {
                    IServiceProvider provider = srvHerarchy.Root as IServiceProvider;
                    if (provider != null)
                    {
                        INodeInformation containedItem = provider.GetService(typeof(INodeInformation)) as INodeInformation;
                        servers.Add(containedItem.Connection as SqlConnectionInfo);
                    }                
            }

            return servers;
        }


        // select server on object window
        public void SelectServer(SqlConnectionInfo connection)
        {

            IExplorerHierarchy hierarchy = GetHierarchyForConnection(connection);
            SelectNode(hierarchy.Root);
        }


        internal void OpenTable(NamedSmoObject objectToSelect, SqlConnectionInfo connection)
        {
            try
            {
                 IExplorerHierarchy hierarchy = GetHierarchyForConnection(connection);
                if (hierarchy == null)
                {
                    return; // there is nothing we can really do if we don't have one of these
                }
                HierarchyTreeNode databasesNode = GetUserDatabasesNode(hierarchy.Root);

                var resultNode = SelectSMOObject(databasesNode, objectToSelect);

                //MSSQLController.Current.SearchWindow.Activate();

                if (resultNode != null)
                    OpenTable(resultNode, connection);

                 
            }
            catch(Exception ex)
            {
                MyLogger.LogError("Error opening table: " + objectToSelect.Name ,ex);
            }

        
        }


        internal void SelectSMOObjectInObjectExplorer(NamedSmoObject objectToSelect, SqlConnectionInfo connection)
        {
            IExplorerHierarchy hierarchy = GetHierarchyForConnection(connection);
            if (hierarchy == null)
            {
                return; // there is nothing we can really do if we don't have one of these
            }
            HierarchyTreeNode databasesNode = GetUserDatabasesNode(hierarchy.Root);
            var resultNode = SelectSMOObject(databasesNode, objectToSelect);
            if (resultNode != null)
                SelectNode(resultNode);
        }

        private HierarchyTreeNode GetUserDatabasesNode(HierarchyTreeNode rootNode)
        {
            if (rootNode != null)
            {
                // root should always be expandable
                if (rootNode.Expandable)
                {
                    EnumerateChildrenSynchronously(rootNode);
                    rootNode.Expand();

                    // TODO this is horrible code - it assumes the first node will ALWAYS be the "Databases" node in the object explorer, which may not always be the case
                    // however I couldn't think of a clean way to always find the right node
                    return (HierarchyTreeNode)rootNode.Nodes[0];
                }
            }
            return null;
        }

        private string BuildMatchingPathExpressionForDepth(NamedSmoObject objectToSelect, string parentNodePath, int level, out bool atFinalLevel)
        {
            atFinalLevel = false;
            string expression = string.Empty;

            // Databases node is at level 1
            switch (level)
            {
                case 2: // database level
                    Regex re = new Regex(".*Database\\[@Name='(.*?)']?");
                    Match m = re.Match(objectToSelect.Urn);
                    expression = parentNodePath + "\\" + m.Groups[1].Captures[0];
                    atFinalLevel = (objectToSelect is Database);
                    break;

                case 3:
                    if (objectToSelect is StoredProcedure || objectToSelect is UserDefinedFunction)
                    {
                        expression = parentNodePath + "\\Programmability";
                    }
                    else if (objectToSelect is Table)
                    {
                        expression = parentNodePath + "\\Tables";
                    }
                    else if (objectToSelect is Microsoft.SqlServer.Management.Smo.View)
                    {
                        expression = parentNodePath + "\\Views";
                    }
                    break;

                case 4:
                    if (objectToSelect is StoredProcedure)
                    {
                        expression = parentNodePath + "\\Stored Procedures";
                    }
                    else if (objectToSelect is UserDefinedFunction)
                    {
                        expression = parentNodePath + "\\Functions";
                    }
                    else
                    {
                        expression = parentNodePath + "\\" + GetSchemaQualifiedNameForSmoObject(objectToSelect);
                        atFinalLevel = true;
                    }
                    break;

                case 5:
                    if (objectToSelect is UserDefinedFunction)
                    {
                        switch (((UserDefinedFunction)objectToSelect).FunctionType)
                        {
                            case UserDefinedFunctionType.Scalar:
                                expression = parentNodePath + "\\Scalar-valued Functions";
                                break;
                            case UserDefinedFunctionType.Table:
                                expression = parentNodePath + "\\Table-valued Functions";
                                break;
                        }
                    }
                    else
                    {

                        expression = parentNodePath + "\\" + GetSchemaQualifiedNameForSmoObject(objectToSelect);
                        atFinalLevel = true;
                    }
                    break;
                case 6:
                    expression = parentNodePath + "\\" + GetSchemaQualifiedNameForSmoObject(objectToSelect);
                    atFinalLevel = true;
                    break;
            }

            return expression;
        }

        private string GetSchemaQualifiedNameForSmoObject(NamedSmoObject namedObject)
        {
            Regex re = new Regex(".*\\[@Name='(.*?)' and @Schema='(.*?)']");
            Match m = re.Match(namedObject.Urn);
            string schemaQualifiedName = m.Groups[2].Captures[0] + "." + m.Groups[1].Captures[0];
            return schemaQualifiedName; // Named SMO object has a FullQualifiedName property but it is internal
        }

        private HierarchyTreeNode SelectSMOObject(HierarchyTreeNode node, NamedSmoObject objectToSelect)
        {
            if (node != null)
            {
                if (node.Expandable)
                {
                    EnumerateChildrenSynchronously(node);
                    node.Expand();

                    bool atFinalLevel;
                    string pattern = BuildMatchingPathExpressionForDepth(objectToSelect, node.FullPath, node.Level + 1, out atFinalLevel);

  

                    foreach (HierarchyTreeNode child in node.Nodes)
                    {
                        if (string.Compare(child.FullPath, pattern, true) == 0)
                        {
                            if (atFinalLevel)
                            {
                                return child;// SelectNode(child);
                            }
                            else
                            {
                                return SelectSMOObject(child, objectToSelect);
                            }
                        }
                    }

                }
            }

              return null;
        }

        private void OpenTable(HierarchyTreeNode node,SqlConnectionInfo connection)
        {

            var t = Type.GetType("Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer.OpenTableHelperClass,ObjectExplorer", true, true);
            var mi = t.GetMethod("EditTopNRows", BindingFlags.Static | BindingFlags.Public);


            var ncT = Type.GetType("Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer.NodeContext,ObjectExplorer",true,true);
            IServiceProvider provider = node as IServiceProvider;
            INodeInformation containedItem = provider.GetService(typeof(INodeInformation)) as INodeInformation;
         
            var inst = Activator.CreateInstance(ncT, containedItem);
            if (inst == null)
                throw new Exception("Cannot create type" + ncT.ToString());
            mi.Invoke(null, new object[] { containedItem, 200 });
         
        }

        private void SelectNode(HierarchyTreeNode node)
        {
            IServiceProvider provider = node as IServiceProvider;
            if (provider != null)
            {
                INodeInformation containedItem = provider.GetService(typeof(INodeInformation)) as INodeInformation;
                if (containedItem != null)
                {
                    IObjectExplorerService objExplorer = ServiceCache.GetObjectExplorer();
                    objExplorer.SynchronizeTree(containedItem);
                }
            }
        }

        // ugly reflection hack
        private IExplorerHierarchy GetHierarchyForConnection(SqlConnectionInfo connection)
        {
            IObjectExplorerService objExplorer = ServiceCache.GetObjectExplorer();
           
            Type t = objExplorer.GetType();
            MethodInfo getHierarchyMethod = t.GetMethod("GetHierarchy", BindingFlags.Instance | BindingFlags.NonPublic);
            if (getHierarchyMethod != null)
            {
                // VS2008 here we have additional param string.Empty - need Dependecy Injection in order to make it work?
                IExplorerHierarchy hierarchy = getHierarchyMethod.Invoke(objExplorer, new object[] { connection, string.Empty }) as IExplorerHierarchy;
                return hierarchy;
            }

            return null;
        }

        // another exciting opportunity to use reflection
        private void EnumerateChildrenSynchronously(HierarchyTreeNode node)
        {
            Type t = node.GetType();
            MethodInfo method = t.GetMethod("EnumerateChildren", new Type[] { typeof(Boolean) });
            if (method != null)
            {
                method.Invoke(node, new object[] { false });
            }
            else
            {
                // fail
                node.EnumerateChildren();
            }
        }
    }
}
