using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

// code based on https://alanbondo.wordpress.com/2008/06/22/creating-a-system-tray-app-with-c/ 
namespace MyWorkCam
{
    public class SysTrayApp : Form
    {
        // you set this to true for shortening screen capture period
        public bool fasterDebugMode = false; 

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        SettingsForm f;
        AboutForm aboutForm;
        static public SysTrayApp singleton;

        System.Threading.Timer timer;
        public SysTrayApp()
        {
            singleton = this;

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

            f = new SettingsForm();
            aboutForm = new AboutForm();

            LoadConfig();

            InitTimer();
        }

        // captures screen if time to capture comes now.
        void CaptureIfTimeMatches()
        {
            var now = DateTime.Now;
            if (now > timeToCapture)
            {
                if (fasterDebugMode)
                    timeToCapture = now + new TimeSpan(0, 0, 3);
                else
                    timeToCapture = now + new TimeSpan(0, settings.saveIntervalMinutes, 0);
                CaptureNow();
            }
        }

        DateTime timeToCapture = DateTime.Now;

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
                timer.Dispose();

                // Release the icon resource.
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }

        // capture the screen into a file.
        public void CaptureNow()
        {
            // Don't expect GC to dispose these right now. 
            Bitmap bmpScreenshot = null;
            Graphics gfxScreenshot = null; 
            Bitmap smallShot = null;

            try
            {
                // written based on the code at http://stackoverflow.com/questions/362986/capture-the-screen-into-a-bitmap

                //Create a new bitmap.
                bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                               Screen.PrimaryScreen.Bounds.Height,
                                               PixelFormat.Format32bppArgb);

                // Create a graphics object from the bitmap.
                gfxScreenshot = Graphics.FromImage(bmpScreenshot);

                // Take the screenshot from the upper left corner to the right bottom corner.
                gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                            Screen.PrimaryScreen.Bounds.Y,
                                            0,
                                            0,
                                            Screen.PrimaryScreen.Bounds.Size,
                                            CopyPixelOperation.SourceCopy);

                // check whether this captured image is valid.
                // the screen will be complete black if the desktop is locked. then we don't have to save the screen.
                smallShot = new Bitmap(bmpScreenshot, new Size(100, 100)); // get small screen 
                bool saveIt = false;
                for (int i = 0; i < 100; i++)
                {
                    for (int j = 0; j < 100; j++)
                    {
                        Color c = smallShot.GetPixel(i, j); // does we have non-black pixel?
                        if (c != Color.Black)
                        {
                            saveIt = true;
                            break;
                        }
                    }
                }

                if (saveIt)
                {
                    // Save the screenshot to the specified path that the user has chosen.
                    var rootFolderName = settings.saveFolder;
                    var currTime = DateTime.Now;
                    var folderName = currTime.ToString("yyyy-MM-dd");
                    var fileNamePostfix = currTime.ToString("HH-mm-ss");
                    var fileName = $"Screenshot-{fileNamePostfix}.png";
                    var savePath = Path.Combine(rootFolderName, folderName, fileName);
                    var saveFolder = Path.Combine(rootFolderName, folderName);
                    if (!Directory.Exists(saveFolder))
                    {
                        Directory.CreateDirectory(saveFolder);
                    }
                    bmpScreenshot.Save(savePath, ImageFormat.Png);
                }

            }
            catch (Exception)
            {
            }
            finally
            {
                if (bmpScreenshot != null)
                    bmpScreenshot.Dispose();
                if (gfxScreenshot != null)
                    gfxScreenshot.Dispose();
                if (smallShot != null)
                    smallShot.Dispose();

            }
        }

        public Settings settings = new Settings();


        void InitTimer()
        {
            // start timer for periodic screen capture
            timer = new System.Threading.Timer((o) =>
            {
                // let's do it in main thread.
                BeginInvoke(new Action(() =>
                {
                    CaptureIfTimeMatches();
                }));
            });

            if (fasterDebugMode)
                timer.Change(0, 1000);
            else
                timer.Change(0, 30 * 1000); // 30초에 한번 정도 일어나는 타이머는 시스템에 거의 영향 안 준다.
        }

        private void LoadConfig()
        {
            try
            {

            }
            finally
            {

            }
        }

    }
}