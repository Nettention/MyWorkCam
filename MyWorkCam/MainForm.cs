using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyWorkCam
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            try
            {
                // written based on the code at http://stackoverflow.com/questions/362986/capture-the-screen-into-a-bitmap

                //Create a new bitmap.
                var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                               Screen.PrimaryScreen.Bounds.Height,
                                               PixelFormat.Format32bppArgb);

                // Create a graphics object from the bitmap.
                var gfxScreenshot = Graphics.FromImage(bmpScreenshot);

                // Take the screenshot from the upper left corner to the right bottom corner.
                gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                            Screen.PrimaryScreen.Bounds.Y,
                                            0,
                                            0,
                                            Screen.PrimaryScreen.Bounds.Size,
                                            CopyPixelOperation.SourceCopy);

                // check whether this captured image is valid.
                // the screen will be complete black if the desktop is locked. then we dont have to save the screen.
                Bitmap smallShot = new Bitmap(bmpScreenshot, new Size(100, 100)); // get small screen 
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
                    var centerFolderName = Environment.CurrentDirectory;
                    var currTime = DateTime.Now;
                    var folderName = currTime.ToString("yyyy-MM-dd");
                    var fileNamePostfix = currTime.ToString("HH-mm");
                    var fileName = $"Screenshot-{fileNamePostfix}.png";
                    var savePath = Path.Combine(centerFolderName, folderName, fileName);
                    var saveFolder = Path.Combine(centerFolderName, folderName);
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

        }
    }
}
