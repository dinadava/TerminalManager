using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using TerminalManager.Entities;
using TerminalManager.Repository;

namespace TerminalManager
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (Mutex mutex = new Mutex(false, Application.ProductName))
            {
                if(!mutex.WaitOne(1000,false))
                {
                    MessageBox.Show(Application.ProductName + " is already open.");
                    return;
                }

                TerminalLogger.Instance.Write("[STATUS] Start");
                if (RunCheckers())
                    Application.Run(new MainForm());
                else
                {
                    TerminalLogger.Instance.Write("[STATUS] Exit");
                    Application.Exit();
                }
                //Application.Run(new MainForm());
            }
        }

        static bool RunCheckers()
        {
            MainForm form = new MainForm();
            // check if mysql is running
            if (!form.checkMySQLService())
            {
                MessageBox.Show("Terminal Manager will now exit. Please start the MySQL service and start autosync again.", "TerminalManager Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Instance.Write("[RunCheckers] Terminal Manager will now exit. Please start the MySQL service and start again.");
                return false;
            }
            // check if Settings.xml exist 
            if (!File.Exists(Settings._xmlFilePath))
            {
                MessageBox.Show("Settings file is either missing or corrupted. Application is now closing.");
                ErrorLogger.Instance.Write("[RunCheckers] Settings file is either missing or corrupted. Application is now closing.");
                return false;
            }
            // check if terminal type is valid
            if (Settings.Instance.Type < 0 && Settings.Instance.Type > 3)
            {
                MessageBox.Show("Invalid TerminalType on Settings file!");
                ErrorLogger.Instance.Write("[RunCheckers] Invalid Type on Settings file.");
                return false;
            }
            // check if clientid valid
            NelsoftDbRepository nelsoftDb = new NelsoftDbRepository();
            if (!nelsoftDb._clientExist)
            {
                MessageBox.Show("Invalid ClientNetworkID on Settings file!");
                ErrorLogger.Instance.Write("[RunCheckers] Invalid ClientNetworkID on Settings file!");
                return false;
            }
            // check if branchid match config
            if (Settings.Instance.BranchId != DatabaseRepository.GetDbBranchId())
            {
                MessageBox.Show("BranchId in settings and in config does not match!");
                ErrorLogger.Instance.Write("[RunCheckers] BranchId in settings and in config does not match!");
                return false;
            }
            // check if terminalno match config, if terminaltype is POS or Server
            if (Settings.Instance.Type != 2 && Settings.Instance.TerminalNo != DatabaseRepository.GetDbTerminalNo())
            {
                MessageBox.Show("TerminalNo in settings and in config does not match!");
                ErrorLogger.Instance.Write("[RunCheckers] TerminalNo in settings and in config does not match!");
                return false;
            }
            // check if terminal exist
            if (Settings.Instance.TerminalNo != 0 && !TerminalRepository.PosTerminals.Where(t => t.Wid.Equals(Settings.Instance.TerminalNo)).Select(t => t).Any())
            {
                MessageBox.Show("TerminalNo in settings does not exist on terminal table.");
                ErrorLogger.Instance.Write("[RunCheckers] TerminalNo in settings does not exist on terminal table.");
                return false;
            }
            // check ftp address, username, and password
            if (Settings.Instance.FtpUsername != ""||
                Settings.Instance.FtpPassword != "" ||
                Settings.Instance.FtpAddress != "")
            {
                MessageBox.Show("Incomplete FTP Credentials on Settings File. Application is now closing.");
                ErrorLogger.Instance.Write("[RunCheckers] Incomplete FTP Credentials on Settings File.");
                return false;
            }
            // check folder paths
            if (Settings.Instance.Type != 3) // for PosType 1,2 only
            {
                if (Settings.Instance.ServerFolderPath != "")
                {
                    MessageBox.Show("ServerFolderPath in settings is empty. Application is now closing.");
                    ErrorLogger.Instance.Write("[RunCheckers] ServerFolderPath in settings is blank!");
                    return false;
                }
            }
            if (Settings.Instance.Type != 1) // for PosType 2,3 only
            {
                if (Settings.Instance.POSFolderPath != "")
                {
                    MessageBox.Show("POSFolderPath in settings is empty. Application is now closing.");
                    ErrorLogger.Instance.Write("[RunCheckers] POSFolderPath in settings is blank!");
                    return false;
                }
            }
            return true;
        }
    }
}
