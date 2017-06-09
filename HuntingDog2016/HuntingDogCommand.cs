//------------------------------------------------------------------------------
// <copyright file="HuntingDogCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using System.Resources;
using System.Reflection;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using HuntingDog.Core;
using HuntingDog.DogFace;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.VisualBasic.Compatibility.VB6;
using Microsoft.VisualStudio.CommandBars;

namespace HuntingDog
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class HuntingDogCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("c2de79ab-b0ac-40bf-b340-34c6ab2bff53");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="HuntingDogCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private HuntingDogCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static HuntingDogCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new HuntingDogCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            //string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            //string title = "HuntingDogCommand";

            //// Show a message box to prove we were here
            //VsShellUtilities.ShowMessageBox(
            //    this.ServiceProvider,
            //    message,
            //    title,
            //    OLEMSGICON.OLEMSGICON_INFO,
            //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

            try
            {
                //BuildCommandInToolsMenu();
            }
            catch (Exception ex)
            {
                log.Error("On startup - build command failed", ex);
            }

            log.Info("About to create Hunting Dog dialog");
            _addInCreater.CreateAddinWindow(addInInstance, Caption);

        }

        private void BuildCommandInToolsMenu()
        {
            ReadShortcutCommandBinding();
            var contextGUIDS = new Object[] { };
            Commands2 commands = (Commands2)ServiceCache.ExtensibilityModel.Commands;

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
            CommandBar menuBarCommandBar = ((CommandBars)ServiceCache.ExtensibilityModel.CommandBars)["MenuBar"];
            var cb = (CommandBars)ServiceCache.ExtensibilityModel.CommandBars;

            //Find the Tools command bar on the MenuBar command bar:
            CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
            CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;

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
                                                    (Int32)vsCommandStatus.vsCommandStatusSupported +
                                                    (Int32)vsCommandStatus.vsCommandStatusEnabled,
                                                    (Int32)vsCommandStyle.vsCommandStylePictAndText,
                                                    vsCommandControlType.vsCommandControlTypeButton);

            //Add a control for the command to the tools menu:
            if ((command != null) && (toolsPopup != null))
            {
                CommandBarControl control = (CommandBarControl)command.AddControl(toolsPopup.CommandBar, 1);

                var bindings = new System.Object[2];
                bindings[0] = "Global::ctrl+" + LaunchingHotKey.ToLower();
                bindings[1] = "SQL Query Editor::ctrl+" + LaunchingHotKey.ToLower(); ;
                command.Bindings = bindings;

                CommandBarButton button = (CommandBarButton)control;

                var ole = (stdole.StdPicture)Support.ImageToIPictureDisp(HuntingDog.Properties.Resources.footprint);
                button.Picture = ole;
            }

            log.Info("Connect: finished adding new command");
        }

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

        private AddIn addInInstance;
        static HuntingDog.DogEngine.AddinCreater _addInCreater = new HuntingDog.DogEngine.AddinCreater();
        private static readonly Log log = LogFactory.GetLog();

        public string LaunchingHotKey = "D";
        public string Caption
        {
            get { return string.Format("Hunting Dog (Ctrl+{0})", LaunchingHotKey); }
        }
    }
}
