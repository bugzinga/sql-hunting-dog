using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;


namespace HuntingDog
{
    internal sealed class HuntingDogCommand
    {
        private readonly Package _package;
        private IVsWindowFrame _windowFrame = null;
        private HuntingDog.ucHost _uglyUsefuleDogFace;

        private HuntingDogCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("HuntingDogCommand is null from an unknown reason.");
            }

            this._package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandId = new CommandID(PackageGuids.HuntingDogCommandSetID, PackageIds.HuntingDogCommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandId);
                commandService.AddCommand(menuItem);
            }
        }

        public static HuntingDogCommand Instance { get; private set; }
        private IServiceProvider ServiceProvider { get { return this._package; } }
        public string Caption { get { return "Hunting Dog"; } }

        public static void Initialize(Package package)
        {
            Instance = new HuntingDogCommand(package);
            Instance.ShowToolWindow();
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            ShowToolWindow();
        }

        //https://www.mztools.com/articles/2015/MZ2015005.aspx
        private void ShowToolWindow()
        {
            const string TOOLWINDOW_GUID = "{7C23E551-2E95-40A8-B783-3753D4E3DEAB}";

            if (_windowFrame == null)
            {

                _uglyUsefuleDogFace = new ucHost();
                _windowFrame = CreateToolWindow(Caption, TOOLWINDOW_GUID, _uglyUsefuleDogFace);

                // additional init could be done after this line for the user control
            }
            _windowFrame.Show();
        }


        private IVsWindowFrame CreateToolWindow(string caption, string guid, System.Windows.Forms.UserControl userControl)
        {
            const int TOOL_WINDOW_INSTANCE_ID = 0; // Single-instance toolwindow

            IVsUIShell uiShell = (IVsUIShell)ServiceProvider.GetService(typeof(SVsUIShell));
            Guid toolWindowPersistenceGuid = new Guid(guid);
            Guid guidNull = Guid.Empty;
            int[] position = new int[1];
            IVsWindowFrame windowFrame = null;

            int result = uiShell.CreateToolWindow((uint)__VSCREATETOOLWIN.CTW_fInitNew, TOOL_WINDOW_INSTANCE_ID, userControl, ref guidNull, ref toolWindowPersistenceGuid, ref guidNull, null, caption, position, out windowFrame);

            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(result);

            return windowFrame;
        }


    }
}
