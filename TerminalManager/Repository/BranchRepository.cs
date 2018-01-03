using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerminalManager.Entities;
using TerminalManager.Helpers;

namespace TerminalManager.Repository
{
    public class BranchRepository
    {
        public  static List<Branch> BranchList = new List<Branch>();

        public BranchRepository()
        {
            GetAllBranches();
        }

        private void GetAllBranches()
        {
            DataTable resultDt = new SqlQuery(@"
                        SELECT
                            `wid`, `name`, `sqlserver`, `localnetworkip`
                        FROM
                            `branch`").ExecuteReader();
            foreach (DataRow row in resultDt.Rows)
            {
                BranchList.Add(new Branch
                {
                    BranchID = Convert.ToInt32(row["wid"]),
                    BranchName = row["name"].ToString(),
                    BranchServerIP = row["sqlserver"].ToString(),
                    LocalNetworkIP = row["localnetworkip"].ToString()
                });
            }
        }
    }
}
