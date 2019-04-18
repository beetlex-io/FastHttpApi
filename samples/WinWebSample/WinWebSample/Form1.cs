using BeetleX.FastHttpApi;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinWebSample
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private BeetleX.FastHttpApi.HttpApiServer mHttpApiServer;
        private void Form1_Load(object sender, EventArgs e)
        {
            SetIE();
            mHttpApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mHttpApiServer.Register(typeof(Form1).Assembly);
            mHttpApiServer.Open();
        }

        private void SetIE()
        {
            int BrowserVer, RegVal;
            BrowserVer = webBrowser1.Version.Major;
            if (BrowserVer >= 11)
                RegVal = 11001;
            else if (BrowserVer == 10)
                RegVal = 10001;
            else if (BrowserVer == 9)
                RegVal = 9999;
            else if (BrowserVer == 8)
                RegVal = 8888;
            else
                RegVal = 7000;
            string productName = AppDomain.CurrentDomain.SetupInformation.ApplicationName;//获取程序名称
            RegistryKey key = Registry.CurrentUser;
            RegistryKey software =
                key.CreateSubKey(
                    @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION\" + productName);
            if (software != null)
            {
                software.Close();
                software.Dispose();
            }
            RegistryKey wwui =
                key.OpenSubKey(
                    @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true);
            if (wwui != null) wwui.SetValue(productName, RegVal, RegistryValueKind.DWord);
            webBrowser1.Url = new Uri("http://localhost:12345");
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void webBrowser1_DocumentCompleted_1(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        [Controller]
        public class WebAction
        {
            public object GetStarted(string email)
            {
                return new TextResult($"{email} {DateTime.Now}");
            }
        }


    }
}
