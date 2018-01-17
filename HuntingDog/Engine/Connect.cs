
using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using HuntingDog.Core;
using HuntingDog.DogEngine.Impl;
using HuntingDog.DogFace;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.VisualBasic.Compatibility.VB6;
using Microsoft.VisualStudio.CommandBars;

namespace HuntingDog
{
    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />]
    ///
    // f  {C454B5C8-3004-4893-B72A-A583E1789AD9}
    [Guid("B00DF00D-1234-1234-AAAA-BAADC0DE9991")]
    public partial class Connect : IDTExtensibility2, IDTCommandTarget
    {
        private static readonly Log log = LogFactory.GetLog();

        private AddIn addInInstance;

        private EnvDTE.Window addinWindow;

        static Connect()
        {
            log.Info("Program started");
        }

        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect()
        {
        }

        public string Caption
        {
            get { return string.Format("Hunting Dog (Ctrl+{0})", LaunchingHotKey); }
        }

        //EventWatcher _eventWatcher;

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
            try
            {
                BuildCommandInToolsMenu();
            }
            catch (Exception ex)
            {
                log.Error("On startup - build command failed", ex);
            }

            //CreateToolWindow();

            log.Info("Connect: On Startup complete");
            addinWindow = _addInCreater.CreateAddinWindow(addInInstance, Caption);

            //MSSQLController.Current.CreateAddinWindow(_addInInstance);
        }


        internal static HuntingDog.DogEngine.AddinCreater _addInCreater = new HuntingDog.DogEngine.AddinCreater();

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(Object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            try
            {
                //return;
                //_applicationObject = (DTE2)application;
                addInInstance = (AddIn) addInInst;

                if (connectMode == ext_ConnectMode.ext_cm_AfterStartup) //ext_ConnectMode.ext_cm_UISetup
                {
                    //MSSQLController.Current.CreateAddinWindow(_addInInstance);
                }

                // Added ext_ConnectMode.ext_cm_UISetup here as it seems working this way in SSMS 2012
                if ((connectMode == ext_ConnectMode.ext_cm_Startup) || (connectMode == ext_ConnectMode.ext_cm_UISetup)) //ext_ConnectMode.ext_cm_UISetup
                {
                    BuildCommandInToolsMenu();

                    //_applicationObject.Events.SelectionEvents.OnChange += new _dispSelectionEvents_OnChangeEventHandler(SelectionEvents_OnChange);
                }
            }
            catch (Exception ex)
            {
                log.Error("Connect: building", ex);
            }
        }


        public string LaunchingHotKey = "D";
        private string _caption;

        private void ReadShortcutCommandBinding()
        {
            try
            {
                var userPreference = UserPreferencesStorage.Load();
                Config.ConfigPersistor pers = new Config.ConfigPersistor();
                var cfg = pers.Restore<Config.DogConfig>(userPreference);

                if (cfg.LaunchingHotKey.Length == 1)
                    LaunchingHotKey = cfg.LaunchingHotKey;
            }
            catch (Exception ex)
            {
                log.Error("ReadShortcutCommandBinding: failed", ex);
            }
        }

        private void BuildCommandInToolsMenu()
        {
            ReadShortcutCommandBinding();
            var contextGUIDS = new Object[] { };
            Commands2 commands = (Commands2) ServiceCache.ExtensibilityModel.Commands;

            //Commands2 commands = (Commands2)_applicationObject.Commands;
            String toolsMenuName;

            log.Info("Connect: Building tools menu");

            try
            {
                //If you would like to move the command to a different menu, change the word "Tools" to the
                //  English version of the menu. This code will take the culture, append on the name of the menu
                //  then add the command to that menu. You can find a list of all the top-level menus in the file
                //  CommandBar.resx.
                ResourceManager resourceManager = new ResourceManager("HuntingDog.CommandBar", Assembly.GetExecutingAssembly());

                //DIY: you will need to change this if you change the name of your project
                //CultureInfo cultureInfo = new System.Globalization.CultureInfo(_applicationObject.LocaleID);
                CultureInfo cultureInfo = new CultureInfo(ServiceCache.ExtensibilityModel.LocaleID);
                String resourceName = String.Concat(cultureInfo.TwoLetterISOLanguageName, "Tools");
                toolsMenuName = resourceManager.GetString(resourceName);
            }
            catch
            {
                //We tried to find a localized version of the word Tools, but one was not found.
                //  Default to the en-US word, which may work for the current culture.
                toolsMenuName = "Tools";
            }

            //Place the command on the tools menu.
            //Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
            //Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["MenuBar"];
            CommandBar menuBarCommandBar = ((CommandBars) ServiceCache.ExtensibilityModel.CommandBars) [ "MenuBar" ];
            var cb = (CommandBars) ServiceCache.ExtensibilityModel.CommandBars;

            //Find the Tools command bar on the MenuBar command bar:
            CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
            CommandBarPopup toolsPopup = (CommandBarPopup) toolsControl;

            //DIY: this is what you need to do to change the icon of the add-in - seriously - http://msdn2.microsoft.com/en-us/library/ms228771(VS.80).aspx
            //Add a command to the Commands collection:

            //Command command = null;;
            foreach (Command cmd in commands)
            {
                if (cmd.Name.Contains("Hunting") || cmd.Name.Contains("NavigateInOE"))
                {
                    //toolsControl.Delete(cmd);
                    cmd.Delete(); //
                    //throw new Exception();
                    break;
                }
            }

            log.Info("Connect: Adding new command");
            var command = commands.AddNamedCommand2(addInInstance, "HuntingDog", "Hunting Dog ",
                                                    "Unleash the dog!", true, 1, ref contextGUIDS,
                                                    (Int32) vsCommandStatus.vsCommandStatusSupported +
                                                    (Int32) vsCommandStatus.vsCommandStatusEnabled,
                                                    (Int32) vsCommandStyle.vsCommandStylePictAndText,
                                                    vsCommandControlType.vsCommandControlTypeButton);

            //Add a control for the command to the tools menu:
            if ((command != null) && (toolsPopup != null))
            {
                CommandBarControl control = (CommandBarControl) command.AddControl(toolsPopup.CommandBar, 1);

                var bindings = new System.Object[2];
                bindings[0] = "Global::ctrl+" + LaunchingHotKey.ToLower();
                bindings[1] = "SQL Query Editor::ctrl+"+ LaunchingHotKey.ToLower();;
                command.Bindings = bindings;

                CommandBarButton button = (CommandBarButton) control;

                var ole = (stdole.StdPicture) Support.ImageToIPictureDisp(HuntingDog.Properties.Resources.footprint);
                button.Picture = ole;
            }

            log.Info("Connect: finished adding new command");
        }

        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        /// <param term='commandName'>The name of the command to determine state for.</param>
        /// <param term='neededText'>Text that is needed for the command.</param>
        /// <param term='status'>The state of the command in the user interface.</param>
        /// <param term='commandText'>Text requested by the neededText parameter.</param>
        /// <seealso class='Exec' />
        public void QueryStatus(String commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref Object commandText)
        {
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                if (commandName.ToLower() == "HuntingDog.Connect.HuntingDog".ToLower()) //DIY: if you're changing the name of your add-in you will need to change this
                {
                    status = (vsCommandStatus) vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                    return;
                }
            }
        }

        public void Exec(String commandName, vsCommandExecOption executeOption, ref Object varIn, ref Object varOut, ref Boolean handled)
        {
            handled = false;

            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if (commandName.ToLower() == "HuntingDog.Connect.HuntingDog".ToLower()) //DIY: if you're changing the name of your add-in you will need to change this
                {
                    // we need to iterate through existing windows and

                    if ((addinWindow != null) && addinWindow.Visible)
                    {
                        log.Info("Activate window");
                        addinWindow.Activate();
                        HuntingDog.DogEngine.Impl.DiConstruct.Instance.ForceShowYourself();
                    }
                    else
                    {
                        //BuildCommandInToolsMenu();
                        log.Info("Create Addin Window");


                        addinWindow = _addInCreater.CreateAddinWindow(addInInstance, Caption);
                        //MSSQLController.Current.CreateAddinWindow(_addInInstance);
                    }

                    handled = true;
                    return;
                }
            }
        }

        void SelectionEvents_OnChange()
        {
            //throw new NotImplementedException();
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
            //if (disconnectMode != ext_DisconnectMode.ext_dm_UserClosed &&
            //    disconnectMode != ext_DisconnectMode.ext_dm_HostShutdown)
            //  return;

            //// Remove all Commands
            //Command cmd = null;
            //try
            //{
            //    cmd =
            // this._applicationObject.Commands.Item("AddinTemplate.Connect.FooAddinTemplate",
            // -1);
            //    cmd.Delete();
            //    cmd =
            // this._applicationObject.Commands.Item("AddinTemplate.Connect.BarAddinTemplate",
            // -1);
            //    cmd.Delete();
            //}
            //catch { ; }
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
            if (_addInCreater.SearchWindow != null)
            {
                log.Info("OnBeginShutdown - hiding window");
                _addInCreater.SearchWindow.Visible = false;
            }
        }

        /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
        /// <param term='commandName'>The name of the command to execute.</param>
        /// <param term='executeOption'>Describes how the command should be run.</param>
        /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
        /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
        /// <param term='handled'>Informs the caller if the command was handled or not.</param>
        /// <seealso class='Exec' />

        //private DTE2 _applicationObject;
    }
}
