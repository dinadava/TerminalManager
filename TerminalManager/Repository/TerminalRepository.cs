using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerminalManager.Helpers;

namespace TerminalManager.Repository
{
    public class TerminalRepository
    {
        public class PosTerminal
        {
            public int Wid { get; set; }
            public string ServerIp { get; set; }
            public string State { get; set; }
        }

        public static List<PosTerminal> PosTerminals = GetAllPosTerminals();
        private static List<PosTerminal> GetAllPosTerminals()
        {
            List<PosTerminal> temp = new List<PosTerminal>();
            DataTable dt = new SqlQuery(@"SELECT 
                                            `terminalno`, `sqlserver`
                                        FROM 
                                            `terminal` 
                                        WHERE
                                            `ACTIVE` = 1 AND `branchid` = " + Settings.Instance.BranchId
                                        ).ExecuteReader();
            foreach (DataRow row in dt.Rows)
            {
                temp.Add(new PosTerminal
                {
                    Wid = Convert.ToInt32(row["terminalno"]),
                    ServerIp = row["sqlserver"].ToString()
                });
            }
            return temp;
        }
    }
}
