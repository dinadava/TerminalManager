using System;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Windows.Forms;
using TerminalManager.Helpers;

namespace TerminalManager.Entities
{
    public class GlobalVariables
    {
        public static readonly string ApplicationNameAndVersion = Application.ProductName + " " + Application.ProductVersion;
        public static readonly string BranchName = InitializeBranchName();
        public static string LogsPath = Environment.MachineName + "_" + GetIPAddress() + "_Logs";
        public static string FtpScriptPath = Application.StartupPath + "\\Scripts";
        public static bool ServerStatus { get; set; }
        public static DataTable PosStatusList { get; set; }
        private static string InitializeBranchName()
        {
            string sql = "SELECT `name` FROM `branch` WHERE `wid` = " + Settings.Instance.BranchId;
            DataTable dataTable = new SqlQuery(sql).ExecuteReader();
            if (dataTable != null)
                if (dataTable.Rows.Count > 0)
                    return dataTable.Rows[0]["name"].ToString();
            return "";
        }
        public static string GetIPAddress()
        {
            IPHostEntry hostInfo = Dns.GetHostByName(Dns.GetHostName());
            if (hostInfo.AddressList.Length > 0)
                return hostInfo.AddressList[0].ToString();
            return null;
        }
    }
}

