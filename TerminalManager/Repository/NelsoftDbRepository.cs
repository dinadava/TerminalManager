﻿using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using TerminalManager.Entities;
using TerminalManager.Helpers;

namespace TerminalManager.Repository
{
    public class NelsoftDbRepository
    {
        public bool _clientExist;
        public bool _branchExist;
        public bool _isSuccessful;
        
        public NelsoftDbRepository()
        {
            _clientExist = ClientExist();
            _branchExist = BranchExist();
            _isSuccessful = UpdateTerminals(this.GetTerminalDetails());
           // _isSuccessful = UpdloadTerminalDetails();
        }

        // to be removed 
        private readonly string ConnectionString = "server=localhost" 
                                                  + ";userid=root"
                                                  + ";password=121586"
                                                  + ";database=nelsoft_clients"
                                                  + ";Allow Zero Datetime=true;Connect Timeout=300;Allow User Variables=True;Charset=utf8";
        private MySqlConnection GetConnection()
        {
            MySqlConnection conn = new MySqlConnection(this.ConnectionString);
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
            }
            catch { }
            return conn;
        }
        private DataTable ExecuteReader(string query)
        {
            DataTable dt = new DataTable();
            MySqlCommand command = new MySqlCommand(query, this.GetConnection());
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                dt.Load(reader);
            }
            return dt;
        }

        private int ExecuteNonQuery(string query)
        {
            MySqlCommand command = new MySqlCommand(query, this.GetConnection());
            return command.ExecuteNonQuery();
        }

        public bool ClientExist()
        {
            //// For API
            //string result = APIRequest.GetRequest(Settings.Instance.ApiServerAddress + "/getclient/" + Settings.Instance.ClientId);
            //if (result != "")
            //    return true;
            //return false;

            string query = "SELECT `id` FROM `clienthead` WHERE `id` = " + Settings.Instance.ClientId + " LIMIT 1";
            if (this.ExecuteReader(query).Rows.Count > 0)
                return true;
            return false;
        }

        private bool TerminalExist(int clientdetailsId, int refno)
        {
            string query = "SELECT * FROM `clientterminaldetails` WHERE `clientbranchid` = " + clientdetailsId +
                           " AND `referenceno` = " + refno + " LIMIT 1;";
            return ExecuteReader(query).Rows.Count > 0;
        }
        
        private bool BranchExist()
        {
            //// For API
            //string result = result = APIRequest.GetRequest(Settings.Instance.ApiServerAddress + "/" + "getbranchbyclient?branchId=" + Settings.Instance.ClientId + "&clientId=" + Settings.Instance.BranchId);
            //if (result != "")
            //    return true;
            //return false;

            string query = @"SELECT `branchid` as `clientbranchid` FROM `clientdetails` WHERE `clientid` = " +
                           Settings.Instance.ClientId +
                           " AND `branchid` = " + Settings.Instance.BranchId;
            if (this.ExecuteReader(query).Rows.Count > 0)
                return true;
            return false;
        }

        private Terminal GetTerminalDetails()
        {
            int clientbranchId = 0;
            string query = "SELECT `id` as `clientbranchid`" +
                            " FROM `clientdetails`" +
                            " WHERE `clientid` = " + Settings.Instance.ClientId +
                            " AND `branchid` = " + Settings.Instance.BranchId +
                            " LIMIT 1";
            if(this.ExecuteReader(query).Rows.Count > 0)   
                clientbranchId = Convert.ToInt32(this.ExecuteReader(query).Rows[0]["clientbranchid"]);

            Terminal terminalDetails = new Terminal
            {
                ClientDetailsID = clientbranchId
            };

            return terminalDetails;
        }


        private bool UpdateTerminals(Terminal terminal)
        {
            if (terminal.ClientDetailsID == 0)
            {
                ErrorLogger.Instance.Write("[TERMINALUPDATE] Error when updating TerminalClientDetails on port82. BranchID " +Settings.Instance.BranchId+" for ClientNetworkId "+Settings.Instance.ClientId+ " does not exist!");
                return false;
            }
            bool isUpdate = false;
            string query;
            if (!TerminalExist(terminal.ClientDetailsID, terminal.TerminalNo))
            {
                query =
                    "INSERT INTO `clientterminaldetails`(clientbranchid,type,referenceno,TeamViewer_id,TeamViewer_pass,versionno,autosyncversion,databaseversion,remark,permitno,machineno,serialno,posstatusid,name,company_accre,mall_accre,pos_type,datecreated)" +
                    "VALUES ('" + terminal.ClientDetailsID +
                            "','" + terminal.TerminalType +
                            "','" + terminal.TerminalNo +
                            "','" + terminal.TeamViewerID +
                            "','" + terminal.TeamViewerPass +
                            "','" + terminal.SystemVersion +
                            "','" + terminal.AutoSyncVersion +
                            "','" + terminal.DbVersion +
                            "','" + terminal.Remark +
                            "','" + terminal.PermitNo +
                            "','" + terminal.MachineNo +
                            "','" + terminal.SerialNo +
                            "','" + (int)terminal.PosStatus +
                            "','" + terminal.TerminalName +
                            "','" + (int)terminal.CompanyAccredType +
                            "','" + (int)terminal.MallAccredType +
                            "','" + (int)terminal.PosType +
                            "','" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") +
                            "')";
            }
            else
            {
                query = @"UPDATE `clientterminaldetails`
                        SET `versionno` = '" + terminal.SystemVersion + "'," +
                            "`autosyncversion` = '" + terminal.AutoSyncVersion + "'," +
                            "`databaseversion` = '" + terminal.DbVersion + "'," +
                            "`posstatusid` = '" + (int)terminal.PosStatus + "'," +
                            "`lastmodifieddate` = '" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + 
                       "' WHERE `clientbranchid` = " + terminal.ClientDetailsID + 
                       " AND `referenceno` = " + terminal.TerminalNo;
                isUpdate = true;
            }
            try
            {
                if (this.ExecuteNonQuery(query) > 0)
                {
                    TerminalLogger.Instance.Write("[PORT82] ClientBranchId: "+ terminal.ClientDetailsID+" | RefNo: " + terminal.TerminalNo + (isUpdate? " Terminal Details Update Sucessful!" : " Terminal Details Entry Successful!"));
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                TerminalLogger.Instance.Write("[PORT82] ClientBranchId: " + terminal.ClientDetailsID + " | RefNo: " + terminal.TerminalNo + (isUpdate ? " Terminal Details Update Failed! Exception: " : " Terminal Details Entry Failed! Check Error Logs."));
                ErrorLogger.Instance.Write("[TERMINALUPDATE] Error when updating TerminalClientDetails on port82. Exception: " + e);
                return false;
            }
        }

        // for API
        private bool UpdloadTerminalDetails()
        {
            Dictionary<string, string> terminalDictionary = new Dictionary<string, string>();
            Terminal terminal = new Terminal();

            terminalDictionary.Add("clientid", terminal.ClientId.ToString());
            terminalDictionary.Add("branchid", terminal.BranchId.ToString());
            terminalDictionary.Add("type", terminal.TerminalType.ToString());
            terminalDictionary.Add("referenceno", terminal.TerminalNo.ToString());
            terminalDictionary.Add("teamviewer_id", terminal.TeamViewerID);
            terminalDictionary.Add("teamviewer_pass", terminal.TeamViewerPass);
            terminalDictionary.Add("versionno", terminal.SystemVersion);
            terminalDictionary.Add("autosyncversion", terminal.AutoSyncVersion);
            terminalDictionary.Add("dbversion", terminal.DbVersion);
            terminalDictionary.Add("remark", terminal.Remark);
            terminalDictionary.Add("permitno", terminal.PermitNo);
            terminalDictionary.Add("machineno", terminal.MachineNo);
            terminalDictionary.Add("serialno", terminal.SerialNo);
            terminalDictionary.Add("posstatusid", ((int)terminal.PosType).ToString());
            terminalDictionary.Add("name", terminal.TerminalName);
            terminalDictionary.Add("companyaccre", ((int)terminal.CompanyAccredType).ToString());
            terminalDictionary.Add("mallaccre", ((int)terminal.MallAccredType).ToString());
            terminalDictionary.Add("postype", ((int)terminal.PosType).ToString());
            
            if (APIRequest.PostRequest(Settings.Instance.ApiServerAddress + "/terminaldetails", terminalDictionary))
                return true;
            else
                return false;
        }
    }
}
