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


    public partial class SettingsForm : Form
    {

        public SettingsForm()
        {
            InitializeComponent();
            propertyGrid1.SelectedObject = SysTrayApp.singleton.settings;
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            SysTrayApp.singleton.CaptureNow();

        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
