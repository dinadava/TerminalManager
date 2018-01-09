using System.Data;
using TerminalManager.Helpers;

namespace TerminalManager.Repository
{
    public class DatabaseRepository
    {
        public static decimal GetDbSize()
        {
            decimal size = 0;
            DataTable dt = new SqlQuery(@"
                    SELECT 
                        table_schema 'DB Name', 
                    ROUND
                        (SUM(data_length + index_length) / 1024 / 1024, 1) 'Size in MB'
                    FROM
                        information_schema.tables 
                    WHERE 
                        table_schema = '" + Settings.Instance.Database + "' GROUP  BY table_schema;"
                ).ExecuteReader();
            if (dt.Rows.Count > 0)
                decimal.TryParse(dt.Rows[0][1].ToString(),out size);
            return size;
        }
        public static string GetLocalNetworkIP()
        {
            DataTable dt = new SqlQuery(@"
                            SELECT
                                `localnetworkip`
                            FROM
                                `branch`
                            WHERE
                                `wid` = " + Settings.Instance.BranchId + " LIMIT 1"
                ).ExecuteReader();
            if (dt.Rows.Count > 0)
                return dt.Rows[0][0].ToString();
            return null;
        }
        public static long GetDbBranchId()
        {
            long temp = 0;
            DataTable dt = new SqlQuery(@"
                SELECT
                    `value`
                FROM
                    `config`
                WHERE
                    `particular` = 'branchid'
                LIMIT 1").ExecuteReader();
            if (dt.Rows.Count > 0)
                long.TryParse(dt.Rows[0][0].ToString(),out temp);
            return temp;
        }
        public static string GetDbVersion()
        {
            DataTable dt = new SqlQuery(@"
                SELECT
                    `value`
                FROM
                    `config`
                WHERE
                    `particular` = 'version'
                LIMIT 1").ExecuteReader();
            if (dt.Rows.Count > 0)
                return dt.Rows[0][0].ToString();
            return "";
        }
        public static int GetDbTerminalNo()
        {
            int temp = 0;
            DataTable dt = new SqlQuery(@"
                SELECT
                    `value`
                FROM
                    `config`
                WHERE
                    `particular` = 'terminalno'
                LIMIT 1").ExecuteReader();
            if (dt.Rows.Count > 0)
                int.TryParse(dt.Rows[0][0].ToString(), out temp);
            return temp;
        }
        
    }
}
