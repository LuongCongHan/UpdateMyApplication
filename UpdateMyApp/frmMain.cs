using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UpdateMyApp
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            _frmUpdateInfo.btnUpdate.Click += BtnUpdate_frmUpdateInfo_Click;
            delayTimer.Tick += DelayTimer_Tick;
        }
        frmDownloadAndExtract _frmDownloadAndExtract = new frmDownloadAndExtract();
        private void DelayTimer_Tick(object sender, EventArgs e)
        {
            delayTimer.Stop();
            _frmDownloadAndExtract._updateVersion = _frmUpdateInfo._updateVersion;
            _frmDownloadAndExtract._toTalLengthUnzip = _frmUpdateInfo._totalDownloadSize;
            //Download here
            _frmDownloadAndExtract.StartPosition = FormStartPosition.CenterParent;
            _frmDownloadAndExtract.ShowDialog(this);
            if (_frmDownloadAndExtract.isStartNewUpdate)
            {
                this.Close();
            }
        }

        Timer delayTimer = new Timer() { Interval=500};
        private void BtnUpdate_frmUpdateInfo_Click(object sender, EventArgs e)
        {
            _frmUpdateInfo.Close();
            delayTimer.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lbCurrentVersion.Text = this.ProductVersion;
        }
        frmUpdateInfo _frmUpdateInfo = new frmUpdateInfo();
        private void btnCheckUpdate_Click(object sender, EventArgs e)
        {
            _frmUpdateInfo.StartPosition = FormStartPosition.CenterParent;
            _frmUpdateInfo.ShowDialog(this);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hi");
        }
    }
}
