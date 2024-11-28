using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

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
            //Xoá file cũ đi
            //string path = @"C:\Users\Dell Latitude 3540\AppData\Local\Temp\{26E1A46A-AD69-43E4-B3C9-BBD65C5E82D8}";
            //Directory.Delete(path, true);
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface Interface in interfaces)
                {
                    IPv4InterfaceStatistics statistics1 = Interface.GetIPv4Statistics();
                    if (Interface.OperationalStatus == OperationalStatus.Up)
                    {
                        if ((Interface.NetworkInterfaceType == NetworkInterfaceType.Ppp) && (Interface.NetworkInterfaceType != NetworkInterfaceType.Loopback))
                        {
                            IPv4InterfaceStatistics statistics = Interface.GetIPv4Statistics();
                            MessageBox.Show(Interface.Name + " " + Interface.NetworkInterfaceType.ToString() + " " + Interface.Description);
                        }
                        else
                        {
                            MessageBox.Show("VPN Connection is lost!");
                        }
                    }
                }
            }
        }
        public static bool CheckForInternetConnection(int timeoutMs = 10000, string url = null)
        {
            try
            {
                CultureInfo cultureInfo = CultureInfo.InstalledUICulture;
                if (cultureInfo.Name.StartsWith("fa")) // Iran
                    url = "http://www.aparat.com";
                else if (cultureInfo.Name.StartsWith("zh"))  // China
                    url = "http://www.baidu.com";
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                request.Timeout = timeoutMs;
                using (var response = (HttpWebResponse)request.GetResponse())
                    return true;
            }
            catch
            {
                return false;
            }
        }

        private void btnCheckInternet_Click(object sender, EventArgs e)
        {
            //if (!CheckForInternetConnection(1000, "http://www.google.com"))
            //{
            //    MessageBox.Show("Không có internet");
            //}
            //else
            //    MessageBox.Show("Có internet");
            //var host= Dns.GetHostEntry("https://github.com/LuongCongHan/MyAppUpdate/releases/download/File/AllFile.zip");
            Uri uri = new Uri("https://github.com/LuongCongHan/MyAppUpdate/releases/download/File/AllFile.zip");

            var host = Dns.GetHostEntry("www.github.com");
            //MessageBox.Show(DisplayDnsAddresses());
        }
        public static string DisplayDnsAddresses()
        {
            string txt=null;
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {

                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                IPAddressCollection dnsServers = adapterProperties.DnsAddresses;
                if (dnsServers.Count > 0)
                {
                    Console.WriteLine(adapter.Description);
                    foreach (IPAddress dns in dnsServers)
                    {
                        txt += "  DNS Servers ............................. :" + dns.ToString() +Environment.NewLine;
                    }
                }
            }
            return txt;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            MessageBox.Show("sau 5s");
        }

        private void btnBatTimer_Click(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void btnTatTimer_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }
    }
}
