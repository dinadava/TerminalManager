using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TerminalManager.Helpers;
using TerminalManager.Repository;

namespace TerminalManager.Entities
{
    public class Terminal
    {
        public int ClientDetailsID { get; set; }
        public int TerminalType;
        public int TerminalNo;
        public string TeamViewerID = "";
        public string TeamViewerPass = "";
        public string SystemVersion = "";
        public string AutoSyncVersion = "";
        public string DbVersion = "";
        public string Remark { get; set; }
        public string PermitNo = "";
        public string MachineNo = "";
        public string SerialNo = "";
        public Enums.PosStatus PosStatus;
        public string TerminalName = "";
        public Enums.CompanyAccredType CompanyAccredType { get; set; }
        public Enums.MallAccredType MallAccredType { get; set; }
        public Enums.PosType PosType;

        public Terminal()
        {
            SetTerminalDetails();
            SetPosInfo();
        }

        private void SetTerminalDetails()
        {
            this.TerminalType = Settings.Instance.Type;
            this.TerminalNo = Settings.Instance.Type;
            #region TeamViewer
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c reg query HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\TeamViewer /v ClientID",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false
                }
            };
            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {
                try
                {
                    string[] output = process.StandardOutput.ReadToEnd()
                        .Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    output = output[1].Split(new string[] { "    " }, StringSplitOptions.RemoveEmptyEntries);
                    this.TeamViewerID = HexConverterHelper.HexToDecimal(output[2].Replace("0x", ""));
                    this.TeamViewerPass = "nelsoft121586";
                }
                catch (Exception e)
                {
                    ErrorLogger.Instance.Write(" Error occurred when trying to retrieve TeamViewer Credentials. Exception: " + e);
                    return;
                }
            }
            #endregion
            #region System version
            switch (TerminalType)
            {
                case 1:
                    this.SystemVersion = getServerVersion();
                    break;
                case 2:
                    this.SystemVersion = getServerVersion() + "/" + getPosVersion();
                    if (SystemVersion.Trim() == "/") SystemVersion = "";
                    break;
                case 3:
                    this.SystemVersion = getPosVersion();
                    break;
                default:
                    break;
            }
            #endregion
            #region Autosync Version
            #endregion
            this.DbVersion = DatabaseRepository.GetDbVersion();
            SetPosInfo();
        }

        private string getServerVersion()
        {
            string version = "";
            string serverPath = Settings.Instance.ServerFolderPath;
            string configfilePath = serverPath + "/backstage/config.xml";
            if (File.Exists(configfilePath))
            {
                try
                {
                    XDocument xmlDocument = XDocument.Load(configfilePath);
                    XElement _config = xmlDocument.Element("configuration-file");
                    version = _config.Element("version").Value;
                }
                catch
                {
                    ErrorLogger.Instance.Write("Failed to retrieve Server version! config.xml on server path " +
                                               configfilePath + " is either missing or corrupted.");
                }
            }
            else
            {
                ErrorLogger.Instance.Write("config.xml not found on "+configfilePath);
            }
            return version;
        }
        private string getPosVersion()
        {
            string version = "";
            string posexePath = Settings.Instance.POSFolderPath;
            string[] exeFiles = Directory.GetFiles(posexePath, "*.exe");
            if (exeFiles.Length == 0)
            {
                ErrorLogger.Instance.Write("POS exe file not found on "+Settings.Instance.POSFolderPath);
                return "";
            }
            else if (exeFiles.Length > 1)
            {
                string exeFile = exeFiles.Where(f => !f.Contains("vshost")).Select(f => f).Single();
                if (exeFile.Length > 0)
                    version = FileVersionInfo.GetVersionInfo(exeFile).ProductVersion;
            }
            return version;
        }
        private void SetPosInfo()
        {
            if (this.TerminalType == 1)
            {
                this.PosStatus = Enums.PosStatus.None;
                this.PosType = Enums.PosType.None;
                return;
            }
            #region PosType
            string settingsXml = Settings.Instance.POSFolderPath + "\\Settings.xml";
            if (File.Exists(settingsXml))
            {
                XDocument xmlDocument = XDocument.Load(settingsXml);
                XElement _application = xmlDocument.Element("Settings").Element("Application");
                this.PosType = Convert.ToInt32(_application.Element("TakeOrderOnly").Value) == 1
                    ? Enums.PosType.TakeOrder
                    : Enums.PosType.Restaurant;
            }
            else
            {
                this.PosType = Enums.PosType.Retail;
            }
            #endregion

            #region PosStatus
            this.PosStatus = Enums.PosStatus.HasPTU;
            DataTable ptuInfo = new SqlQuery(@"SELECT
                                                `tin`, `acc`,`permitno`,`sn`,`min`
                                              FROM
                                                 `terminal`
                                              WHERE
                                                  `terminalno` = " + Settings.Instance.TerminalNo + " AND `branchid` = " + Settings.Instance.BranchId +
                                            " LIMIT 1;").ExecuteReader();

            if (ptuInfo.Rows.Count <= 0)
            {
                ErrorLogger.Instance.Write(" TerminalNo " + Settings.Instance.TerminalNo +
                                           " does not exist on terminal table");
                PosStatus = Enums.PosStatus.None;
            }
            else
            {
                for (int i = 0; i < ptuInfo.Columns.Count; i++)
                {
                    if (i == 0)
                    {
                        if (ptuInfo.Rows[0][i].ToString() == "0000-0000-000")
                            this.PosStatus = Enums.PosStatus.None;
                    }
                    else if((int)ptuInfo.Rows[0][i] == 0)
                    {
                        this.PosStatus = Enums.PosStatus.None;
                        return;
                    }
                }
            }
            #endregion

            if (this.PosStatus == Enums.PosStatus.HasPTU)
            {
                this.PermitNo = ptuInfo.Rows[0]["permitno"].ToString();
                this.SerialNo = ptuInfo.Rows[0]["sn"].ToString();
                this.MachineNo = ptuInfo.Rows[0]["min"].ToString();
            }

            #region Company Accred, Mall Accred 
            DataTable posSettings = new SqlQuery(@"SELECT
                                                    `posprovidername`, `accreditation`
                                                  FROM
                                                     `possettings`
                                                  WHERE
                                                     `branchid` = " + Settings.Instance.BranchId +
                                                  " LIMIT 1;").ExecuteReader();

            if (posSettings.Rows.Count <= 0)
            {
                ErrorLogger.Instance.Write(" Pos Settings is empty!");
                return;
            }

            switch (posSettings.Rows[0]["posprovidername"].ToString().ToUpper())
            {
                case "NELSOFT TECHNOLOGY SERVICE":
                    this.CompanyAccredType = Enums.CompanyAccredType.Services;
                    break;
                case "NELSOFT TECHNOLOGY INC.":
                    this.CompanyAccredType = Enums.CompanyAccredType.Technology;
                    break;
                case "NELSOFT SYSTEMS INC.":
                    this.CompanyAccredType = Enums.CompanyAccredType.Systems;
                    break;
                default: this.CompanyAccredType = Enums.CompanyAccredType.Others;
                    break;
            }

            switch (posSettings.Rows[0]["accreditation"].ToString())
            {
                case "SM":
                    this.MallAccredType = Enums.MallAccredType.SM;
                    break;
                case "Fishermall":
                    this.MallAccredType = Enums.MallAccredType.Fishermall;
                    break;
                case "RLC":
                    this.MallAccredType = Enums.MallAccredType.RLC;
                    break;
                case "Starmall": this.MallAccredType = Enums.MallAccredType.Starmall;
                    break;
                case "Greenfield": this.MallAccredType = Enums.MallAccredType.Greenfield;
                    break;
                default: this.MallAccredType = Enums.MallAccredType.None;
                    break;
            }

            #endregion
        }
    }
}
