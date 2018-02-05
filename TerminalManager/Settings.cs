using System;
using System.Windows.Forms;
using System.Xml.Linq;
using TerminalManager.Helpers;

namespace TerminalManager
{
    public class Settings
    {
        private static Settings _instance;
        private XElement _application;
        public static readonly string _xmlFilePath = Application.StartupPath + "\\Settings.xml";
        //Properties
        public int Type { get; set; }
        public int ClientId { get; set; }
        public int TerminalNo { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int BranchId { get; set; }
        public string[] CheckTime { get; set; }
        public string ServerFolderPath = "";
        public string POSFolderPath = "";

        public string FtpUsername = "";
        public string FtpPassword = "";
        public string FtpAddress = "";
        public string ApiServerAddress = "";

        public static Settings Instance
        {
            private set { _instance = value; }
            get
            {
                if (_instance == null)
                    _instance = new Settings();
                return _instance;
            }
        }

        public Settings()
        {
           SetValues(); 
        }
        private void SetValues()
        {
            XDocument xmlDocument = XDocument.Load(_xmlFilePath);
            XElement setting = xmlDocument.Element("Setting");
            _application = setting.Element("Application");

            this.Type = Convert.ToInt32(_application.Element("Type").Value);
            this.ClientId = Convert.ToInt32(_application.Element("ClientNetworkId").Value);
            this.TerminalNo = Convert.ToInt32(_application.Element("TerminalNumber").Value);
            this.Server = _application.Element("Server").Value;
            this.Database = _application.Element("Database").Value;
            this.Username = _application.Element("Username").Value;
            this.Password = EncryptionHelper.Decrypt(_application.Element("Password").Value);
            this.BranchId = Convert.ToInt32(_application.Element("BranchId").Value);
            this.CheckTime = (_application.Element("CheckTime").Value).Split(new string[] { "," } , StringSplitOptions.RemoveEmptyEntries);
            this.ServerFolderPath = _application.Element("ServerFolderPath").Value;
            this.POSFolderPath = _application.Element("PosFolderPath").Value;
           
            this.FtpUsername = _application.Element("FtpUsername").Value;
            this.FtpPassword = _application.Element("FtpPassword").Value;
            this.FtpAddress = _application.Element("FtpAddress").Value;
            
            this.ApiServerAddress = _application.Element("ApiServerAddress").Value;
        }
    }
}
