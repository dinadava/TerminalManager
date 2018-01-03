using System;
using System.Data;
using MySql.Data.MySqlClient;
using TerminalManager.Entities;

namespace TerminalManager.Repository
{
    public class NelsoftDbRepository
    {
        public bool _clientExist;
        public bool _isSuccessful;
        //public string jsonString;
        
        public NelsoftDbRepository()
        {
            _clientExist = ClientExist();
            _isSuccessful = UpdateTerminals(this.GetTerminalDetails());
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
        
        private Terminal GetTerminalDetails()
        {
            //string jsonString = "";
            string query = "SELECT `id` as `clientbranchid`" +
                            " FROM `clientdetails`" +
                            " WHERE `clientid` = " + Settings.Instance.ClientId +
                            " AND `branchid` = " + Settings.Instance.BranchId +
                            " LIMIT 1";
            int clientbranchId = Convert.ToInt32(this.ExecuteReader(query).Rows[0]["clientbranchid"]);

            Terminal terminalDetails = new Terminal
            {
                ClientDetailsID = clientbranchId
            };

            return terminalDetails;
            //return jsonString;
        }


        private bool UpdateTerminals(Terminal terminal)
        {
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
    }
}
