using System;
using System.ComponentModel;
using System.Data;
using System.Net.Sockets;
using System.Threading;
using TerminalManager.Entities;
using TerminalManager.Helpers;
using TerminalManager.Repository;

namespace TerminalManager.BackgroundWorkers
{
    public class StatusCheckerBackgroundWorker : BackgroundWorker
    {
        public StatusCheckerBackgroundWorker()
        {
            this.DoWork += StatusCheckerBackgroundWorker_DoWork;
        }

        private void StatusCheckerBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            TerminalLogger.Instance.Write("[CHECKER] Server/POS State Checking Started");
            try
            {
                switch (Settings.Instance.Type)
                {
                    case 1:// Server: Check if MainServer (10) is online & Check all POS state
                        GlobalVariables.ServerStatus = GetServerStatus(10);
                        TerminalLogger.Instance.Write(" Server Status is " + (GlobalVariables.ServerStatus? "Online": "Offline."));
                        LogPosStatus();
                        break;
                    case 2:// POSServer: Check if MainServer (10) is online 
                        GlobalVariables.ServerStatus = GetServerStatus(10);
                        TerminalLogger.Instance.Write(" Server Status is " + (GlobalVariables.ServerStatus ? "Online" : "Offline."));
                        break;
                    case 3:// POS: Check if Server of POS Branch is online
                        GlobalVariables.ServerStatus = GetServerStatus(Settings.Instance.BranchId);
                        TerminalLogger.Instance.Write(" Server Status is " + (GlobalVariables.ServerStatus ? "Online" : "Offline."));
                        break;
                    default:
                        ErrorLogger.Instance.Write(" Invalid Terminal Type on settings file.");
                        break;
                }
                e.Result = true;
            }
            catch
            {
                e.Result = false;
            }
        }

        private bool GetServerStatus(int branchID)
        {
            bool state = false;
            if (Settings.Instance.BranchId == 10)
                return true;

            string query = "SELECT `sqlserver` as `serverip` FROM `branch` " +
                           "WHERE `show` = 1 AND `wid` = " + branchID;
            DataTable dt = new SqlQuery(query).ExecuteReader();
            if (dt.Rows.Count > 0)
            {
                string serverip = dt.Rows[0]["serverip"].ToString();
                state = CheckState(serverip);
            }
            else
            {
                ErrorLogger.Instance.Write(" Branch " + branchID + " does not exist on branch table.");
            }
            return state;
        }

        private void LogPosStatus()
        {
            if (TerminalRepository.PosTerminals.Count <= 0)
            {
                ErrorLogger.Instance.Write(" No registered terminals on Local Database");
                return;
            }
            TerminalRepository.PosTerminals.ForEach(pos =>
            {
                if (!CheckState(pos.ServerIp))
                    TerminalLogger.Instance.Write(" Terminal " + pos.Wid + " State is Offline.");
                else
                    TerminalLogger.Instance.Write(" Terminal " + pos.Wid + " State if Online.");
            });
        }
        
        private bool CheckState(string IPAddress)
        {
            bool result = false;
            IPAddress = IPAddress.ToLower().Equals("localhost") ? "127.0.0.1" : IPAddress;
            TcpClient tcpClient = new TcpClient();
            tcpClient.BeginConnect(IPAddress, 80, asyncResult =>
            {
                try
                {
                    result = tcpClient.Connected;
                    //tcpClient.EndConnect(asyncResult);
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine("Client closed before connected: " + ex);
                }
            }, null);
            Thread.Sleep(1000);
            return tcpClient.Connected;
        }
    }
}
