using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StormVue2RTCM
{
    public partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
            this.lbCopyright.Text = "©" + DateTime.Now.Year.ToString() + " Astrogenic Systems";
            this.lbRegistered.Text = (Settings.licenseValid ? "Yes" : "No");
         
        }
    }
}
