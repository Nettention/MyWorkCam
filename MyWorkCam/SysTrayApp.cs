using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

// code based on https://alanbondo.wordpress.com/2008/06/22/creating-a-system-tray-app-with-c/ 
namespace MyWorkCam
{

    public class SysTrayApp : Form
    {
        // you temporarily set this to true for shortening screen capture period.
        public bool fasterDebugMode = false; 

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        SettingsForm settingsForm;
        AboutForm aboutForm;
        static public SysTrayApp singleton;

        // for periodically capturing work
        System.Threading.Timer timer;

        public SysTrayApp()
        {
            singleton = this;

            InitTrayApp();

            settingsForm = new SettingsForm();
            aboutForm = new AboutForm();

            LoadConfig();

            InitTimer();

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }


        private void InitTrayApp()
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
            trayIcon.Text = "My Work Cam";
            Icon appIcon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            trayIcon.Icon = new Icon(appIcon, 40, 40);

            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
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

        // as its name say.
        DateTime timeToCapture = DateTime.Now;

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            Application.Exit();
        }

        void OnSettings(object sender, EventArgs e)
        {
            settingsForm.ShowDialog();
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
                if (sessionLocked)
                    return;

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
                    bool isFirst = false;
                    if (!Directory.Exists(saveFolder))
                    {
                        Directory.CreateDirectory(saveFolder);
                        isFirst = true;
                    }
                                        
                    // For user's convenience, we add "Today's first shot" to the image if it is the first screenshot.
                    if (isFirst)
                    {
                        var g = gfxScreenshot;

                        RectangleF rectf = new RectangleF(0, 0, 600, 70);

                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.DrawString("Today's first shot", new Font("Tahoma", 40), Brushes.Red, rectf);
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
                timer.Change(1000, 1000); // 1초 정도는 기다려 주어야 window handle이 만들어지지...
            else
                timer.Change(1000, 30 * 1000); // 30초에 한번 정도 일어나는 타이머는 시스템에 거의 영향 안 준다.
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

        // code based on https://social.msdn.microsoft.com/Forums/vstudio/en-US/45649a15-f60f-41ea-a51b-49e139c74de9/how-do-i-check-if-the-current-desktop-is-locked?forum=csharpgeneral
        public void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                // Do what you need to here as the system is locked
                sessionLocked = true;
            }
            else
            {
                sessionLocked = false;
            }
        }

        // desktop is locked?
        bool sessionLocked = false;

    }
}