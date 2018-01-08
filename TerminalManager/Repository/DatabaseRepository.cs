using TerminalManager.Helpers;

namespace TerminalManager.Repository
{
    public class DatabaseRepository
    {
        public static decimal GetDbSize()
        {
            decimal size = 0;
            decimal.TryParse(new SqlQuery(@"
                    SELECT 
                        table_schema 'DB Name', 
                    ROUND
                        (SUM(data_length + index_length) / 1024 / 1024, 1) 'Size in MB'
                    FROM
                        information_schema.tables 
                    WHERE 
                        table_schema = '"+ Settings.Instance.Database + "' GROUP  BY table_schema;"
                        ).ExecuteReader().Rows[0][1].ToString(),
                    out size);
            return size;
        }
        public static string GetLocalNetworkIP()
        {
            string returnValue = new SqlQuery(@"
                            SELECT
                                `localnetworkip`
                            FROM
                                `branch`
                            WHERE
                                `wid` = " + Settings.Instance.BranchId + " LIMIT 1"
                                ).ExecuteReader().Rows[0][0].ToString();
            if (returnValue != null)
                return returnValue;
            return null;
        }
        public static long GetDbBranchId()
        {
            long temp = 0;
            long.TryParse(new SqlQuery(@"
                SELECT
                    `value`
                FROM
                    `config`
                WHERE
                    `particular` = 'branchid'
                LIMIT 1").ExecuteReader().Rows[0][0].ToString(),
                out temp);
            return temp;
        }
        public static string GetDbVersion()
        {
            string temp = new SqlQuery(@"
                SELECT
                    `value`
                FROM
                    `config`
                WHERE
                    `particular` = 'version'
                LIMIT 1").ExecuteReader().Rows[0][0].ToString();
            return temp;
        }
        public static int GetDbTerminalNo()
        {
            int temp = 0;
            int.TryParse(new SqlQuery(@"
                SELECT
                    `value`
                FROM
                    `config`
                WHERE
                    `particular` = 'terminalno'
                LIMIT 1").ExecuteReader().Rows[0][0].ToString(),
                out temp);
            return temp;
        }
        
    }
}
