using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using TerminalManager.Entities;

namespace TerminalManager.Helpers
{
    public class SqlQuery
    {
        public const string DateTimeStringFormat = "yyyy-MM-dd HH:mm:ss";
        public const string DateStringFormat = "yyyy-MM-dd";
        public const string TimeStringFormat = "HH:mm:ss";

        public static Type CommandType
        {
            get
            {
                return _commandType;
            }
            protected set
            {
                _commandType = value;
            }
        }
        public static Type ConnectionType
        {
            get
            {
                return _connectionType;
            }
            protected set
            {
                _connectionType = value;
            }
        }
        public static Type ParameterType
        {
            get
            {
                return _parameterType;
            }
            protected set
            {
                _parameterType = value;
            }
        }

        public bool LogError { get; set; }
        public string ConnectionString { get; set; }
        public string Sql { get; set; }
        public Dictionary<string, object> Parameters { get; private set; }

        public SqlQuery(string sql)
            : this(sql, new Dictionary<string, object>())
        { }
        public SqlQuery(string sql, params KeyValuePair<string, object>[] parameters)
            : this(sql, parameters.ToDictionary(x => x.Key, x => x.Value))
        {
        }
        public SqlQuery(string sql, Dictionary<string, object> parameters)
        {
            this.ConnectionString = GetConnectionString();
            this.Sql = sql;
            this.Parameters = parameters;
            this.LogError = true;
        }
        
        public static string GetConnectionString()
        {
            return "server=" + Settings.Instance.Server
                   + ";userid=" + Settings.Instance.Username
                   + ";password=" + Settings.Instance.Password
                   + ";database=" + Settings.Instance.Database
                   + ";Allow Zero Datetime=true;Connect Timeout=300;Allow User Variables=True;Charset=utf8";
        }

        public static bool ExecuteScript(string script)
        {
            MySqlConnection connection = new MySqlConnection(GetConnectionString());
            try
            {
                connection.Open();
                MySqlScript mySqlScript = new MySqlScript(connection, script);
                mySqlScript.Execute();
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                ErrorLogger.Instance.Write("[RUNSCRIPT] Error when running MySql script. Exception:" + ex);
                return false;
            }
        }

        public int ExecuteNonQuery()
        {
            return ExecuteNonQuery(this.Parameters);
        }
        /// <summary>
        /// Executes non-query with given parameters instead of Parameters property.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(Dictionary<string, object> parameters)
        {
            int returnValue = 0;
            DbConnection connection = (DbConnection)Activator.CreateInstance(ConnectionType);
            connection.ConnectionString = this.ConnectionString;
            DbCommand command = (DbCommand)Activator.CreateInstance(CommandType);
            try
            {
                connection.Open();
                command.CommandText = this.Sql;
                command.Connection = connection;
                if (parameters != null)
                    foreach (KeyValuePair<string, object> paramArg in parameters)
                    {
                        DbParameter parameter = (DbParameter)Activator.CreateInstance(ParameterType);
                        parameter.ParameterName = paramArg.Key;
                        if (paramArg.Value is DateTime)
                        {
                            parameter.Value = ((DateTime)paramArg.Value).ToString(DateTimeStringFormat);
                            parameter.DbType = DbType.DateTime;
                        }
                        else if (paramArg.Value is Enum)
                        {
                            parameter.Value = paramArg.Value;
                            parameter.DbType = DbType.Int32;
                        }
                        else
                        {
                            parameter.Value = paramArg.Value;
                            parameter.DbType = DbType.String;
                            parameter.Size = paramArg.ToString().Length;
                        }
                        command.Parameters.Add(parameter);
                    }
                command.Prepare();
                returnValue = command.ExecuteNonQuery();
                command.Dispose();
                connection.Close();
            }
            catch (Exception e)
            {
                string parameterValues = "";
                foreach (DbParameter parameter in command.Parameters)
                    parameterValues += ", " + parameter.ParameterName + " = " + parameter.Value.ToString();
                string logText =
                    e.Message
                    + "\n" + this.ConnectionString
                    + "\n" + this.Sql
                    + "\n" + (parameterValues.Length > 2 ? parameterValues.Substring(2) : "");
                if (LogError)
                {
                    Console.WriteLine(logText);
                    SqlErrorLogger.Instance.Write(logText);
                }
            }
            return returnValue;
        }
        public DataTable ExecuteReader()
        {
            return ExecuteReader(this.Parameters);
        }
        /// <summary>
        /// Executes query with given parameters instead of Parameters property.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable ExecuteReader(Dictionary<string, object> parameters)
        {
            DataTable returnValue = new DataTable();
            DbConnection connection = (DbConnection)Activator.CreateInstance(ConnectionType);
            connection.ConnectionString = this.ConnectionString;
            DbCommand command = (DbCommand)Activator.CreateInstance(CommandType);
            try
            {
                connection.Open();
                command.CommandText = this.Sql;
                command.Connection = connection;
                if (parameters != null)
                    foreach (KeyValuePair<string, object> paramArg in parameters)
                    {
                        DbParameter parameter = (DbParameter)Activator.CreateInstance(ParameterType);
                        parameter.ParameterName = paramArg.Key;
                        if (paramArg.Value is DateTime)
                        {
                            parameter.Value = ((DateTime)paramArg.Value).ToString(DateTimeStringFormat);
                            parameter.DbType = DbType.DateTime;
                        }
                        else if (paramArg.Value is Enum)
                        {
                            parameter.Value = paramArg.Value;
                            parameter.DbType = DbType.Int32;
                        }
                        else
                        {
                            parameter.Value = paramArg.Value;
                            parameter.DbType = DbType.String;
                            parameter.Size = paramArg.ToString().Length;
                        }
                        command.Parameters.Add(parameter);
                    }
                command.Prepare();
                returnValue.Load(command.ExecuteReader());
                command.Dispose();
                connection.Close();
            }
            catch (Exception e)
            {
                string parameterValues = "";
                foreach (DbParameter parameter in command.Parameters)
                    parameterValues += ", " + parameter.ParameterName + " = " + parameter.Value.ToString();
                string logText =
                    e.Message
                    + "\n" + this.ConnectionString
                    + "\n" + this.Sql
                    + "\n" + (parameterValues.Length > 2 ? parameterValues.Substring(2) : "");
                if (LogError)
                {
                    Console.WriteLine(logText);
                    SqlErrorLogger.Instance.Write(logText);
                }
            }
            return returnValue;
        }
        public object ExecuteScalar()
        {
            return ExecuteScalar(this.Parameters);
        }
        /// <summary>
        /// Executes query with given parameters instead of Parameters property.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteScalar(Dictionary<string, object> parameters)
        {
            object returnValue = null;
            DbConnection connection = (DbConnection)Activator.CreateInstance(ConnectionType);
            connection.ConnectionString = ConnectionString;
            DbCommand command = (DbCommand)Activator.CreateInstance(CommandType);
            try
            {
                connection.Open();
                command.CommandText = this.Sql;
                command.Connection = connection;
                if (parameters != null)
                    foreach (KeyValuePair<string, object> paramArg in parameters)
                    {
                        DbParameter parameter = (DbParameter)Activator.CreateInstance(ParameterType);
                        parameter.ParameterName = paramArg.Key;
                        if (paramArg.Value is DateTime)
                        {
                            parameter.Value = ((DateTime)paramArg.Value).ToString(DateTimeStringFormat);
                            parameter.DbType = DbType.DateTime;
                        }
                        else if (paramArg.Value is Enum)
                        {
                            parameter.Value = paramArg.Value;
                            parameter.DbType = DbType.Int32;
                        }
                        else
                        {
                            parameter.Value = paramArg.Value;
                            parameter.DbType = DbType.String;
                            parameter.Size = paramArg.ToString().Length;
                        }
                        command.Parameters.Add(parameter);
                    }
                command.Prepare();
                returnValue = command.ExecuteScalar();
                command.Dispose();
                connection.Close();
            }
            catch (Exception e)
            {
                string parameterValues = "";
                foreach (DbParameter parameter in command.Parameters)
                    parameterValues += ", " + parameter.ParameterName + " = " + parameter.Value.ToString();
                string logText =
                    e.Message
                    + "\n" + this.ConnectionString
                    + "\n" + this.Sql
                    + "\n" + (parameterValues.Length > 2 ? parameterValues.Substring(2) : "");
                if (LogError)
                {
                    Console.WriteLine(logText);
                    SqlErrorLogger.Instance.Write(logText);
                }
            }
            return returnValue;
        }

        private static Type _commandType = typeof(MySqlCommand);
        private static Type _connectionType = typeof(MySqlConnection);
        private static Type _parameterType = typeof(MySqlParameter);
    }

    public class SqlQueryBatch : BindingList<object>
    {
        public static Type CommandType
        {
            get
            {
                return _commandType;
            }
            protected set
            {
                _commandType = value;
            }
        }
        public static Type ConnectionType
        {
            get
            {
                return _connectionType;
            }
            protected set
            {
                _connectionType = value;
            }
        }
        public static Type ParameterType
        {
            get
            {
                return _parameterType;
            }
            protected set
            {
                _parameterType = value;
            }
        }

        public string ConnectionString { get; set; }

        public event SqlQueryBatchEventHandler ExecuteComplete;

        public SqlQueryBatch()
        {
            this.ConnectionString =
                   "server=" + Settings.Instance.Server
                   + ";userid=" + Settings.Instance.Username
                   + ";password=" + Settings.Instance.Password
                   + ";database=" + Settings.Instance.Database
                   + ";Allow Zero Datetime=true;Connect Timeout=300;Allow User Variables=True;Charset=utf8";
        }

        public new void Add(object item)
        {
            if (!(item is SqlQuery || item is SqlQueryBatch))
                throw new InvalidOperationException("'item' should be SqlQuery or SqlTransaction.");

            base.Add(item);
        }
        public new void Clear()
        {
            base.Clear();
            ExecuteComplete = null;
        }
        public object ExecuteTransaction()
        {
            object returnValue = 0;
            Dictionary<SqlQueryBatch, object> resultDictionary = new Dictionary<SqlQueryBatch, object>();
            DbConnection connection = (DbConnection)Activator.CreateInstance(ConnectionType);
            DbCommand command = (DbCommand)Activator.CreateInstance(CommandType);
            DbTransaction transaction = null;
            connection.ConnectionString = ConnectionString;
            bool isSuccess = true;
            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();
                returnValue = ExecuteInternalTransaction(this, transaction, command, connection, resultDictionary);
                transaction.Commit();
                connection.Close();
            }
            catch (DbException e)
            {
                string parameterValues = "";
                isSuccess = false;
                foreach (DbParameter parameter in _parametersTemp)
                    parameterValues += ", " + parameter.ParameterName + " = " + (parameter.Value != null ? parameter.Value.ToString() : "null");
                string logText =
                    e.Message
                    + "\n" + this.ConnectionString
                    + "\n" + this._sqlTemp
                    + "\n" + (parameterValues.Length > 2 ? parameterValues.Substring(2) : "");
                Console.WriteLine(logText);
                SqlErrorLogger.Instance.Write(logText);
                try
                {
                    transaction.Rollback();
                    logText = "Rollback Successful";
                    Console.WriteLine(logText);
                    SqlErrorLogger.Instance.Write(logText);
                }
                catch (DbException e2)
                {
                    logText = "Rollback Failed" + e2.Message;
                    Console.WriteLine(logText);
                    SqlErrorLogger.Instance.Write(logText);
                }
            }
            OnExecuteCompleteRecursion(this, resultDictionary, isSuccess);
            return returnValue;
        }
        public new bool Remove(object item)
        {
            if (!(item is SqlQuery || item is SqlQueryBatch))
                throw new InvalidOperationException("'item' should be SqlQuery or SqlTransaction.");
            return base.Remove(item);
        }

        private object ExecuteInternalTransaction(SqlQueryBatch queryBatch, DbTransaction transaction, DbCommand command, DbConnection connection, Dictionary<SqlQueryBatch, object> resultDictionary)
        {
            object returnValue = 0;
            foreach (object obj in queryBatch)
            {
                if (obj == null)
                    continue;
                if (obj is SqlQueryBatch)
                {
                    returnValue = ExecuteInternalTransaction((SqlQueryBatch)obj, transaction, command, connection, resultDictionary);
                    continue;
                }
                SqlQuery sqlQuery = (SqlQuery)obj;
                command = (DbCommand)Activator.CreateInstance(CommandType);
                _sqlTemp = sqlQuery.Sql;
                command.CommandText = sqlQuery.Sql;
                command.Connection = connection;
                command.Transaction = transaction;
                if (sqlQuery.Parameters != null)
                {
                    foreach (KeyValuePair<string, object> paramArg in sqlQuery.Parameters)
                    {
                        DbParameter parameter = (DbParameter)Activator.CreateInstance(ParameterType);
                        parameter.ParameterName = paramArg.Key;
                        if (paramArg.Value is DateTime)
                        {
                            parameter.Value = ((DateTime)paramArg.Value).ToString(SqlQuery.DateTimeStringFormat);
                            parameter.DbType = DbType.DateTime;
                        }
                        else if (paramArg.Value is Enum)
                        {
                            parameter.Value = paramArg.Value;
                            parameter.DbType = DbType.Int32;
                        }
                        else
                        {
                            parameter.Value = paramArg.Value;
                            parameter.DbType = DbType.String;
                            parameter.Size = paramArg.ToString().Length;
                        }
                        command.Parameters.Add(parameter);
                    }
                }
                _parametersTemp = command.Parameters;
                command.Prepare();
                if (sqlQuery.Sql.Trim().ToLower().StartsWith("select"))
                {
                    DataTable temp = new DataTable();
                    temp.Load(command.ExecuteReader());
                    returnValue = temp;
                }
                else
                    returnValue = command.ExecuteNonQuery();
                command.Dispose();
            }
            resultDictionary.Add(queryBatch, returnValue);
            return returnValue;
        }
        private void OnExecuteCompleteRecursion(SqlQueryBatch queryBatch, Dictionary<SqlQueryBatch, object> resultDictionary, bool isSuccess)
        {
            foreach (object obj in queryBatch)
            {
                if (!(obj is SqlQueryBatch))
                    continue;
                (obj as SqlQueryBatch).OnExecuteCompleteRecursion(obj as SqlQueryBatch, resultDictionary, isSuccess);
            }
            if (isSuccess == false)
            {
                isSuccess = false;
            }
            if (resultDictionary.ContainsKey(queryBatch))
                queryBatch.OnExecuteComplete(new SqlQueryBatchEventArgs(isSuccess, resultDictionary[queryBatch]));
            else
                queryBatch.OnExecuteComplete(new SqlQueryBatchEventArgs(isSuccess, null));
        }

        protected void OnExecuteComplete(SqlQueryBatchEventArgs e)
        {
            if (this.ExecuteComplete == null)
                return;
            this.ExecuteComplete(this, e);
        }
        public override string ToString()
        {
            return toStringRecursive(this);
        }

        private string toStringRecursive(SqlQueryBatch batch)
        {
            StringBuilder temp = new StringBuilder();
            foreach (object o in batch)
            {
                if (o is SqlQuery)
                {
                    temp.Append("\n\r");
                    temp.Append((o as SqlQuery).Sql);
                }
                else
                {
                    temp.Append(toStringRecursive(o as SqlQueryBatch));
                }
            }
            return temp.ToString();
        }

        private string _sqlTemp;
        private DbParameterCollection _parametersTemp;

        private static Type _commandType = SqlQuery.CommandType;
        private static Type _connectionType = SqlQuery.ConnectionType;
        private static Type _parameterType = SqlQuery.ParameterType;
    }

    public class SqlErrorLogger : Logger
    {
        public static SqlErrorLogger Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SqlErrorLogger();
                return _instance;
            }
        }
        private SqlErrorLogger()
            : base(Application.StartupPath + "\\" + GlobalVariables.LogsPath, "SqlErrors")
        {
            this.ShowDateTime = true;
        }
        private static SqlErrorLogger _instance;
    }

    public delegate void SqlQueryBatchEventHandler(object sender, SqlQueryBatchEventArgs e);

    public class SqlQueryBatchEventArgs : EventArgs
    {
        public bool IsSuccess { get; private set; }
        public object Result { get; private set; }
        public SqlQueryBatchEventArgs(bool isSuccess, object result)
        {
            this.IsSuccess = isSuccess;
            this.Result = result;
        }
    }
}
