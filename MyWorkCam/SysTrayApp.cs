using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

// code based on https://alanbondo.wordpress.com/2008/06/22/creating-a-system-tray-app-with-c/ 
namespace MyWorkCam
{
    public class SysTrayApp : Form
    {
      
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        MainForm f = new MainForm();
        AboutForm aboutForm = new AboutForm();


        public SysTrayApp()
        {
            // Create a simple tray menu with only one item.
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Settings", OnSettings);
            trayMenu.MenuItems.Add("About", OnAbout);
            trayMenu.MenuItems.Add("Exit", OnExit);

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "MyTrayApp";
            Icon appIcon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            trayIcon.Icon = new Icon(appIcon, 40, 40);

            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        void OnSettings(object sender, EventArgs e)
        {
            f.ShowDialog();
        }
        void OnAbout(object sender, EventArgs e)
        {
            aboutForm.ShowDialog();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}