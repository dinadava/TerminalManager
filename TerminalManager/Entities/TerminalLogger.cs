using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TerminalManager.Helpers;

namespace TerminalManager.Entities
{
    public class TerminalLogger : Logger
    {
        public static TerminalLogger Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TerminalLogger();
                return _instance;
            }
        }
        private TerminalLogger()
            : base(Application.StartupPath + "\\" + GlobalVariables.LogsPath, "SystemLogs")
        {
            this.ShowDateTime = true;
            this.AppendDateToFileName = true;
        }
        private static TerminalLogger _instance;
    }
}
