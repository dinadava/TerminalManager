using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Shell32;
using TerminalManager.Entities;
using TerminalManager.Repository;
using TerminalManager.Views;

namespace TerminalManager
{
    public partial class MainForm : Form
    {
        private const int timeoutSqlService = 20;
        private bool RestartFlag = false;
        private bool CloseFlag = false;
        private string _status;
        public MainForm()
        {
            InitializeComponent();
            _terminalmanagerBackgroundWorker.ProgressChanged += _terminalmanagerBackgroundWorker_ProgressChanged;
            if (Settings.Instance.BranchId == 10)
                RefreshButton.Visible = false;
            progressBar.Value = 0;
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            this.notifyIcon.Text = Application.ProductName;
            runningTimer.Start();
            _statusStrip.Start();
            _statuscheckerBackgroundWorker.RunWorkerAsync();

            RemoveAutoUpdator();
            UpdateNelsoftDb();
        }
        
        private void runningTimer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            if (now.Minute == 0 && now.Second == 0) // every hour
            {
                TerminalLogger.Instance.Write("[CHECKER] Scheduled Server/POS State Checking");
                this.RefreshButton.Enabled = false;
                this.ManualCheckButton.Enabled = false;
                this.UploadLogsButton.Enabled = false;
                if(!_statuscheckerBackgroundWorker.IsBusy && !_terminalmanagerBackgroundWorker.IsBusy)
                    _statuscheckerBackgroundWorker.RunWorkerAsync();
            }
            foreach (string time in Settings.Instance.CheckTime)
            {
                string[] wholetime = time.Split(':');
                int hour = Convert.ToInt32(wholetime[0]);
                int minute = Convert.ToInt32(wholetime[1]);
                if (now.Hour == hour && now.Minute == minute && now.Second == 0)
                {
                    if (!_terminalmanagerBackgroundWorker.IsBusy && !_statuscheckerBackgroundWorker.IsBusy)
                    {
                        TerminalLogger.Instance.Write("[CHECKER] Scheduled Terminal Checking");
                        notifyIcon.BalloonTipTitle = "Terminal Manager";
                        notifyIcon.BalloonTipText = "Scheduled Terminal Checking is starting.";
                        notifyIcon.ShowBalloonTip(500);
                        this.ManualCheckButton.Enabled = false;
                        this.RefreshButton.Enabled = false;
                        this.UploadLogsButton.Enabled = false;
                        _terminalmanagerBackgroundWorker.RunWorkerAsync();
                    }
                }
            }
        }
        private void RefreshButton_Click(object sender, EventArgs e)
        {
            if (!_statuscheckerBackgroundWorker.IsBusy)
            {
                this.RefreshButton.Enabled = false;
                this.ManualCheckButton.Enabled = false;
                this.UploadLogsButton.Enabled = false;
                _statuscheckerBackgroundWorker.RunWorkerAsync();
            }
        }

        private void ManualCheckButton_Click(object sender, EventArgs e)
        {
            if (!_terminalmanagerBackgroundWorker.IsBusy)
            {
                TerminalLogger.Instance.Write("[MANUAL CHECK] Manual Terminal Checking");
                notifyIcon.BalloonTipTitle = "Terminal Manager";
                notifyIcon.BalloonTipText = "Terminal Checking is starting.";
                notifyIcon.ShowBalloonTip(500);
                this.ManualCheckButton.Enabled = false;
                this.RefreshButton.Enabled = false;
                this.UploadLogsButton.Enabled = false;
                _terminalmanagerBackgroundWorker.RunWorkerAsync();
            }
        }
        
        private void _terminalmanagerBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            labelStatus.Text = "Terminal Checking Completed";
            if ((bool) e.Result)
            {
                TerminalLogger.Instance.Write("[CHECKER] Terminal Checking Completed!");
                if (!Directory.Exists(GlobalVariables.LogsPath))
                {
                    TerminalLogger.Instance.Write("[UPLOADLOGS] Uploading of Logs has been Cancelled! Logs folder " + GlobalVariables.LogsPath + " is missing");
                    return;
                }
            }
            else if (e.Cancelled)
                TerminalLogger.Instance.Write("[CHECKER] Terminal Checking was Cancelled.");
            else
                TerminalLogger.Instance.Write("[CHECKER] Terminal Checking Failed!");

            if (UploadLogs())
                TerminalLogger.Instance.Write("[UPLOADLOGS] Uploading Logs to FTP Server Successful");
            else
                TerminalLogger.Instance.Write("[UPLOADLOGS] Uploading Logs to FTP Server Failed! Check Error Logs.");
            
            if (RestartFlag)
            {
                TerminalLogger.Instance.Write("[STATUS] Restart");
                Application.Restart();
            }

            this.ManualCheckButton.Enabled = true;
            this.RefreshButton.Enabled = true;
            this.UploadLogsButton.Enabled = true;
            progressBar.Value = 0;
        }
        private void _statuscheckerBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((bool)e.Result)
                TerminalLogger.Instance.Write("[CHECKER] Server/POS State Checking Completed!");
            else
                TerminalLogger.Instance.Write("[CHECKER] Server/POS State Checking Failed!");

            if (RestartFlag)
            {
                TerminalLogger.Instance.Write("[STATUS] Restart");
                Application.Restart();
            }

            this.ServersStatusLabel.Text = GlobalVariables.ServerStatus ? "Online" : "Offline";
            ServersStatusLabel.ForeColor = ServersStatusLabel.Text == "Online" ? Color.Green : Color.DimGray;
            this.RefreshButton.Enabled = true;
            this.ManualCheckButton.Enabled = true;
            this.UploadLogsButton.Enabled = true;
        }
        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            BringToFront();
            Focus();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.CloseFlag = true;
            PermissionForm permissionForm = new PermissionForm();
            if(permissionForm.ShowDialog() == DialogResult.OK)
                this.Close();
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CloseFlag)
            {
                e.Cancel = true;
                this.Hide();
                return;
            }

            if (_terminalmanagerBackgroundWorker.IsBusy)
            {
                if (MessageBox.Show("Terminal Checking is currently in progress. Would you like to proceed? ", "Warning") == DialogResult.Yes)
                {
                    _statuscheckerBackgroundWorker.CancelAsync();
                    TerminalLogger.Instance.Write("[STATUS] Exit");
                }
            }
        }

        private void UploadLogsButton_Click(object sender, EventArgs e)
        {
            TerminalLogger.Instance.Write("[MANUALUPLOAD] Manual Uploading of Logs Started");
            if (!Directory.Exists(GlobalVariables.LogsPath))
            {
                TerminalLogger.Instance.Write("[MANUALUPLOAD] Manula Upload has been Cancelled! Logs folder "+GlobalVariables.LogsPath+" is missing");
                return;
            }
            if (UploadLogs())
                TerminalLogger.Instance.Write("[MANUALUPLOAD] Manual Uploading of Logs to FTP Server Successful");
            else
                TerminalLogger.Instance.Write("[MANUALUPLOAD] Manual Uploading of Logs to FTP Server Failed! Check Error Logs.");
        }
        
        private void _terminalmanagerBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            Thread.Sleep(500);
            labelStatus.Text = e.UserState.ToString() + progressBar.Value + "%";
        }
        public bool checkMySQLService()
        {
            bool mySqlFound = false;
            for (int i = 0; i < timeoutSqlService; i++)
            {
                try
                {
                    Process[] processList = Process.GetProcessesByName("mysqld");
                    if (processList.Length != 0)
                        mySqlFound = true;
                }
                catch (Exception)
                {
                }

                if (mySqlFound)
                {
                    TerminalLogger.Instance.Write(" MySQL Service found! " + Application.ProductName + " starts");
                    notifyIcon.BalloonTipTitle = "MySQL Service is running.";
                    notifyIcon.BalloonTipText = Application.ProductName + " starts.";
                    notifyIcon.ShowBalloonTip(500);
                    return true;
                }
                else
                {
                    TerminalLogger.Instance.Write(" MySQL Service is not running! Delaying " + Application.ProductName +
                                                  " process by " + (timeoutSqlService - i) + " seconds.");
                    notifyIcon.BalloonTipTitle = "MySQL Service is not running.";
                    notifyIcon.BalloonTipText = "Delaying " + Application.ProductName + " process by " +
                                                (timeoutSqlService - i) + " seconds.";
                    notifyIcon.ShowBalloonTip(500);
                }
                System.Threading.Thread.Sleep(1000);
            }
            return false;
        }

        private bool UploadLogs()
        {
            string address = Settings.Instance.FtpAddress;
            if (!address.StartsWith("ftp://")) address = "ftp://" + address;
            try
            {
                string[] logs = Directory.GetFiles(GlobalVariables.LogsPath);
                if (logs.Length <= 0)
                {
                    ErrorLogger.Instance.Write("[UPLOADLOGS] Logs folder is empty!");
                    return false;
                }

                #region Check if Logs folder exist
                List<string> files;
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(address);
                request.Credentials = new NetworkCredential(Settings.Instance.FtpUsername, Settings.Instance.FtpPassword);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    files = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    reader.Close();
                }
                response.Close();
                if (!files.Contains(GlobalVariables.LogsPath))
                {
                    request = (FtpWebRequest)WebRequest.Create(address + "//" + GlobalVariables.LogsPath);
                    request.Credentials = new NetworkCredential(Settings.Instance.FtpUsername, Settings.Instance.FtpPassword);
                    request.Method = WebRequestMethods.Ftp.MakeDirectory;
                    response = (FtpWebResponse)request.GetResponse();
                    if (response.StatusCode != FtpStatusCode.FileActionOK)
                    {
                        TerminalLogger.Instance.Write("[UPLOADLOGS] Failed to create logs folder on FTP Server. Check Error Logs.");
                        ErrorLogger.Instance.Write("[UPLOADLOGS] Error when trying to upload logs on FTP Server. Request Error: " + response.StatusDescription);
                        return false;
                    }
                }
                #endregion
                #region Upload logs
                foreach (string log in logs)
                {
                    request = (FtpWebRequest) WebRequest.Create((address + "//"+ log).Replace(@"\","//"));
                    request.Credentials = new NetworkCredential(Settings.Instance.FtpUsername, Settings.Instance.FtpPassword);
                    request.KeepAlive = false;
                    request.Method = WebRequestMethods.Ftp.AppendFile;

                    byte[] fileBytes = File.ReadAllBytes(log);
                    using (var requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(fileBytes, 0, fileBytes.Length);
                    }
                    response = (FtpWebResponse)request.GetResponse();
                    if (response.StatusDescription.ToLower().Contains("successfully transferred"))
                    {
                        TerminalLogger.Instance.Write("[UPLOADLOGS] " + log + " Successfully uploaded!");
                        File.Delete(log); // delete log after uploading
                    }
                    else
                    {
                        TerminalLogger.Instance.Write("[UPLOADLOGS] " + log + " Failed to upload! Check Error Logs");
                        ErrorLogger.Instance.Write("[UPLOADLOGS] Error when trying to upload "+log+ ". Error: " +response);
                    }
                }
                #endregion
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.Instance.Write("[UPLOADLOGS] Error when trying to upload logs on FTP Server. Exception: " + ex);
                return false;
            }
        }

        private void UpdateNelsoftDb()
        {
            NelsoftDbRepository nelsoftDb = new NelsoftDbRepository();
            if(nelsoftDb._isSuccessful)
                TerminalLogger.Instance.Write("[TERMINALUPDATE] ClientTerminalDetails on Port82 successfully updated!");
            else
                TerminalLogger.Instance.Write("[TERMINALUPDATE] ClientTerminalDetails on Port82 update failed!");
        }
        private void RemoveAutoUpdator()
        {
            string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\autoupdator.exe.lnk";
            if (File.Exists(startupFolderPath))
            {
                Process.GetProcessesByName("autoupdator").ToList().ForEach(pr => pr.Kill());
                string pathOnly = Path.GetDirectoryName(startupFolderPath);
                string filenameOnly = Path.GetFileName(startupFolderPath);
                string targetPath = "";

                Shell shell = new Shell();
                Folder folder = shell.NameSpace(pathOnly);
                FolderItem folderItem = folder.ParseName(filenameOnly);
                if (folderItem != null)
                {
                    Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
                    targetPath = link.Path;
                }

                try
                {
                    Directory.Delete(Directory.GetParent(targetPath).ToString(), true);
                    TerminalLogger.Instance.Write("AutoUpdator and all subdirectories has been Successfully Removed!");
                }
                catch (Exception e)
                {
                    ErrorLogger.Instance.Write("Failed to Remove AutoUpdator. Exception: " + e);
                }
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PermissionForm permissionForm = new PermissionForm();
            if (permissionForm.ShowDialog() == DialogResult.OK)
            {
                SettingsForm settingsForm = new SettingsForm();
                settingsForm.ShowDialog();

                if (settingsForm.isSuccessful)
                {
                    TerminalLogger.Instance.Write("[SETTINGS] Settings has been changed");
                    if (_terminalmanagerBackgroundWorker.IsBusy || _statuscheckerBackgroundWorker.IsBusy)
                        MessageBox.Show(
                            "Settings successfully saved! Terminal manager checking is currently in progress. Restart Application to apply changes after checking.");
                    else
                        RestartFlag = true;
                }
            }
        }
    }
}
