using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TerminalManager.Helpers;

namespace TerminalManager.Entities
{
    public class ErrorLogger : Logger
    {
        public static ErrorLogger Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ErrorLogger();
                return _instance;
            }
        }
        private ErrorLogger()
            : base(Application.StartupPath + "\\" + GlobalVariables.LogsPath, "ErrorLogs")
        {
            this.ShowDateTime = true;
            this.AppendDateToFileName = true;
        }
        private static ErrorLogger _instance;
    }
}
