using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorkCam
{
    public class Settings
    {
        string m_saveFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\MyWorkCam Screenshots";
        [DisplayName("Save to"), Description("Save destination folder")]
        public string saveFolder
        {
            get
            {
                return m_saveFolder;
            }
            set
            {
                m_saveFolder = value;
            }
        }

        public int m_saveIntervalMinutes = 10;
        [DisplayName("Save interval"), Description("Save interval in minutes")]
        public int saveIntervalMinutes
        {
            get
            {
                return m_saveIntervalMinutes;
            }
            set
            {
                m_saveIntervalMinutes = value;
            }

        }
    }
}
