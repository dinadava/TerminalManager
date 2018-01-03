using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using TerminalManager.Helpers;
using TerminalManager.Entities;
using TerminalManager.Repository;

namespace TerminalManager.BackgroundWorkers
{
    public class TerminalBackgroundWorker : BackgroundWorker
    {
        public TerminalBackgroundWorker()
        {
            this.DoWork += TerminalBackgroundWorker_DoWork;
            this.WorkerReportsProgress = true;
        }

        private void TerminalBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            TerminalLogger.Instance.Write("[CHECKER] Terminal Checking Started");
            try
            {
                int i = 0;
                this.ReportProgress(i + 10, "Running Disk Check.. ");
                CheckBadSectors();
                //if (RunDiskCheck())
                //{
                //    this.ReportProgress(i + 10, "Checking Bad Sectors.. ");
                //    CheckBadSectors();
                //}

                this.ReportProgress(i + 10, "Checking Improper shutdown.. ");
                LogImproperShutdown();

                this.ReportProgress(i + 10, "Checking Autosync state.. ");
                LogAutosyncStatus();

                this.ReportProgress(i + 10, "Checking Firewall state.. ");
                LogFirewallStatus(new string[] { "Domain", "Public", "Private" });

                this.ReportProgress(i + 10, "Checking 360Antivirus State.. ");
                Check360Antivirus();

                this.ReportProgress(i + 10, "Checking wkhtmltopdf.. ");
                CheckWkthml();

                this.ReportProgress(i + 10, "Downloading MySql script from FTP Server.. ");
                RunMySqlScript();

                this.ReportProgress(i + 10, "Check latest backup.. ");
                LogLatestBackUpDate();

                this.ReportProgress(100, "");
                e.Result = true;
            }
            catch
            {
                e.Result = false;
            }
        }
        
        private bool RunDiskCheck()
        {
            TerminalLogger.Instance.Write("[CHKDSK] Disk Scan Started");
            if (!IsAdministrator())
            {
                TerminalLogger.Instance.Write("[CHKDSK] Disk Scan has been Cancelled! Switch to Administrator Mode");
                return false;
            }
            try
            {
                Application.DoEvents(); // prevents from crashing
                Process process = CmdProcess("/c chkdsk c:/i"); // Performs a less vigorous check of index entries, which reduces the amount of time required to run chkdsk.
                process.Start();
                var result = process.StandardOutput.ReadToEnd();
                if (result.Contains("Stage 3"))
                    TerminalLogger.Instance.Write("[CHKDSK] Disk Scan Completed!");
                else
                {
                    TerminalLogger.Instance.Write("[CHKDSK] Disk Scan was terminated. Check Error Logs");
                    ErrorLogger.Instance.Write("[CHKDSK] Disk Scan failed to proceed to final stage of checking. EventLog: " + result);
                    return false;
                }
            }
            catch (Exception ex)
            {
                TerminalLogger.Instance.Write("[CHKDSK] Disck Scan Failed! Check Error Logs.");
                ErrorLogger.Instance.Write("[CHKKDSK] Error on Disk Scan. Exception: " + ex);
                return false;
            }
            return true;
        }
        private void CheckBadSectors()
        {
            TerminalLogger.Instance.Write("[CHKBADSEC] Scan for Bad Sectors Started");
            Process process = CmdProcess(@"/c wevtutil qe Application /q:*""[System[Provider[@Name = 'Chkdsk']]]"" /f:text /c:1 /rd:true /r:" + Dns.GetHostName());
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            try
            {
                if (result != "")
                {
                    if (!result.Contains("Stage 3")) // final stage of checking, displays bad sectors
                    {
                        TerminalLogger.Instance.Write("[CHKBADSEC] Scan for Bad Sectors failed! Chkdsk log file is either incomplete or corrupted");
                        return;
                    }

                    string[] output = result.Split(new string[] {"Stage"}, StringSplitOptions.RemoveEmptyEntries);
                    string date =
                        output[0].Split(new string[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries)
                            .AsEnumerable()
                            .Where(o => o.Contains("Date"))
                            .Select(o => o)
                            .Single()
                            .Replace("Date:", "")
                            .Trim();
                    string message = "[CHKBADSEC] "
                                     +
                                     output[3].Split(new string[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries)
                                         .AsEnumerable()
                                         .Where(o => o.Contains("bad sectors")).Select(o => o)
                                         .Single().Trim().Replace(".", "") + " found on Last Disk Scan dated: " + date;
                    TerminalLogger.Instance.Write(message);
                    TerminalLogger.Instance.Write("[CHKBADSEC] Scan for Bad Sectors Successful!");
                }
                else
                {
                    TerminalLogger.Instance.Write("[CHKBADSEC] Scan for Bad Sectors failed! No Chkdsk logs found.");
                    return;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Instance.Write("[CHKBADSEC] Error when scanning for Bad Sectors. Exception: " + ex);
                TerminalLogger.Instance.Write("[CHKBADSEC] Scan for Bad Sectors Failed! Check Error Logs.");
            }
        }
        
        private void LogImproperShutdown()
        {
            TerminalLogger.Instance.Write("[IMPSHUTDWN] Scan for Improper Shutdown Started");
            try
            {
                Process process = CmdProcess("/c wevtutil qe System impshutdown_queryfilter.txt /f:text /rd:true /r:" + Dns.GetHostName());//+(File.Exists(logfilePath) ? " >> " : " > ") + logfilePath);
                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                if (result != "")
                {
                    string[] output = result.Split(new string[] { "Event[" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string eventlog in output)
                    {
                        string[] temp = eventlog.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        TerminalLogger.Instance.Write(("[SHUTDOWNLOGS] " + temp[temp.Length - 1]).Replace("?",""));
                    }
                }
                TerminalLogger.Instance.Write("[IMPSHUTDWN] Scan for Improper Shutdown Successful!");
            }
            catch (Exception ex)
            {
                ErrorLogger.Instance.Write("[IMPSHUTDWN] Error when scanning for Improper Shutdown. Exception: "+ex);
                TerminalLogger.Instance.Write("[IMPSHUTDWN] Scan for Improper Shutdown Failed! Check Error Logs.");
            }
        }

        private void LogAutosyncStatus()
        {
            TerminalLogger.Instance.Write("[AUTOSYNC] AutoSync Status Checking Started");
            try
            {
                string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\AutoSync.exe.lnk";
                if (!File.Exists(startupFolderPath))
                {
                    ErrorLogger.Instance.Write(" AutoSync.exe is not found under the Startup Folder.");
                    return;
                }
                Process[] processes = Process.GetProcessesByName("AutoSync");
                if (processes.Length <= 0)
                {
                    ErrorLogger.Instance.Write(" AutoSync is turned off and is not running.");
                }
                TerminalLogger.Instance.Write("[AUTOSYNC] AutoSync Status Checking Successful!");
            }
            catch(Exception ex)
            {
                TerminalLogger.Instance.Write("[AUTOSYNC] AutoSync Status Checking Failed! Check Error Logs.");
                ErrorLogger.Instance.Write("[AUTOSYNC] Error when checking Autosync Status. Exception: "+ex);
            }
        }
        private void LogFirewallStatus(string[] types)
        {
            TerminalLogger.Instance.Write("[CHKFIREWALL] Firewall State Checking Started");
            try
            {
                Process process;
                foreach (string type in types)
                {
                    process = CmdProcess("/c netsh advfirewall show " + type + "profile state");
                    process.Start();
                    if (process.StandardOutput.ReadToEnd().Contains("ON"))
                        ErrorLogger.Instance.Write("[FIREWALL]" + type + " Firewall State is ON");                    
                }

                if (!IsAdministrator())
                    TerminalLogger.Instance.Write("[FIREWALL] Firewall cannot be turned off. Switch to Administrator Mode.");
                else
                {
                    process = CmdProcess("/c netsh advfirewall set allprofiles state off");
                    process.Start();
                    TerminalLogger.Instance.Write("[FIREWALL] All Firewall State has been turned Off");
                }
                TerminalLogger.Instance.Write("[CHKFIREWALL] Firewall State Checking Successful");
            }
            catch (Exception ex)
            {
                TerminalLogger.Instance.Write("[CHKFIREWALL] Firewall State Checking Failed! Check Error Logs.");
                ErrorLogger.Instance.Write("[CHKFIREWALL] Error when checking Firewall State. Exception: "+ex);
            }
        }

        private void Check360Antivirus()
        {
            TerminalLogger.Instance.Write("[CHK360ANTIV] 360AntiVirus Checking Started");
            try
            {
                RegistryKey root = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node", false); // checks local machine
                if (root.GetSubKeyNames().Where(key => key.Equals("360Safe") || key.Equals("360TotalSecurity")).Select(key => key).Any())
                {
                    if (Process.GetProcesses().Where(p => p.ProcessName.Equals("LiveUpdate360") || p.ProcessName.Equals("QHActiveDefense")
                    || p.ProcessName.Equals("QHSafeTray") || p.ProcessName.Equals("QHWatchdog")).Select(p => p).Any())
                    {
                        TerminalLogger.Instance.Write(" A 360 Antivirus is Installed and is currently Running.");
                    }
                    else
                        ErrorLogger.Instance.Write(" A 360 Antivirus is Installed but is not Running.");
                }
                TerminalLogger.Instance.Write("[CHK360ANTIV] 360AntiVirus Checking Successful!");
            }
            catch (Exception ex)
            {
                TerminalLogger.Instance.Write("[CHK360ANTIV] 360AntiVirus Checking Failed! Check Error Logs.");
                ErrorLogger.Instance.Write("Error when checking 360AntiVirus State. Exception: "+ex);
            }
        }

        private void CheckWkthml()
        {
            TerminalLogger.Instance.Write("[WKHTML] Scanning wkhtmltopdf.exe Started");
            try
            {
                bool exist = File.Exists(@"C:\wkhtmltopdf.exe");
                if (!exist)
                    ErrorLogger.Instance.Write(" wkhtmltopdf.exe file is missing.");

                TerminalLogger.Instance.Write("[WKHTML] Scanning wkhtmltopdf.exe Successful!");
            }
            catch (Exception ex)
            {
                TerminalLogger.Instance.Write("[WKHTML] Scanning wkhtmltopdf.exe Failed! Check Error Logs.");
                ErrorLogger.Instance.Write("[WKHTML] Error when scanning wkhtml. Exception: "+ex);
            }
        }
        
        private bool DownloadScripts()
        {
            if (!Directory.Exists(GlobalVariables.FtpScriptPath))
                Directory.CreateDirectory(GlobalVariables.FtpScriptPath);
            #region Get all files from ftpserver

            FtpWebRequest request;
            List<string> files;
            string address = Settings.Instance.FtpAddress;
            if (!address.StartsWith("ftp://")) address = "ftp://" + address;
            NetworkCredential ftpCredentials = new NetworkCredential(Settings.Instance.FtpUsername, Settings.Instance.FtpPassword);

            try
            {
                request = (FtpWebRequest)WebRequest.Create(address);
                request.Credentials = ftpCredentials;
                request.KeepAlive = false;
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                StreamReader reader;
                using (reader = new StreamReader(response.GetResponseStream()))
                {
                    files = reader.ReadToEnd().Split(new string[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    reader.Close();
                }
                response.Close();
                files = files.Where(file => file.Contains(".sql")).Select(file => file).ToList();
                if (files.Count <= 0)
                {
                    ErrorLogger.Instance.Write(" There were no sql files retrieved on the Ftp Server");
                    return false;
                }
            }
            catch(Exception ex)
            {
                ErrorLogger.Instance.Write(" Error when trying to get files from FtpServer. Exception: "+ex);
                return false;
            }

            #endregion
            
            #region Download sql files

            try
            {
                foreach (string file in files)
                {
                    request = (FtpWebRequest)WebRequest.Create(address + "/" + file);
                    request.Credentials = ftpCredentials;
                    request.Method = WebRequestMethods.Ftp.DownloadFile;
                    request.KeepAlive = false;

                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    Stream stream = response.GetResponseStream();
                    StreamReader reader;
                    using (reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        Application.DoEvents(); //prevent application from crashing
                        StreamWriter writer = new StreamWriter(GlobalVariables.FtpScriptPath + "\\" + file, false);
                        writer.Write(reader.ReadToEnd());
                        writer.Close();
                    }

                    reader.Close();
                    stream.Close();
                    response.Close();
                }
            }
            catch(Exception ex)
            {
                ErrorLogger.Instance.Write(" Error when downloading files from FtpServer. Exception: " + ex);
                return false;
            }

            #endregion

            return true;
        }

        private bool IsValidIPAddress(string localnetworkip)
        {
            IPAddress address;
            if (IPAddress.TryParse(localnetworkip, out address))
                return true;
            return false;
        }
        private void RunMySqlScript()
        {
            TerminalLogger.Instance.Write("[RUNSCRIPT] Running MySql scripts Started.");
            if (!DownloadScripts())
            {
                TerminalLogger.Instance.Write("[RUNSCRIPT] Running MySql scripts has been cancelled. Check Error Logs.");
                return;
            }

            #region Check DbSize and LocalNetworkIp (if server and main, check for all branches)

            try
            {
                TerminalLogger.Instance.Write("[DBCHECK] Database Size: " + DatabaseRepository.GetDbSize() + "MB");
            }
            catch
            {
                TerminalLogger.Instance.Write("[DBCHECK] Failed to retrieve size of database.");
            }
            
            try
            {
                if (Settings.Instance.Type == 1 && Settings.Instance.BranchId == 10)
                {
                    foreach (Branch branch in BranchRepository.BranchList)
                    {
                        if (IsValidIPAddress(branch.LocalNetworkIP))
                            TerminalLogger.Instance.Write("[DBCHECK] Branch " + branch.BranchID + " has Invalid LocalNetworkIP value on branch table. Set as local computer name");
                    }
                }
                else
                {
                    if (DatabaseRepository.GetLocalNetworkIP() != null)
                    {
                        if (IsValidIPAddress(DatabaseRepository.GetLocalNetworkIP()))
                            TerminalLogger.Instance.Write("[DBCHECK] Branch " + Settings.Instance.BranchId + " has Invalid LocalNetworkIP value on branch table. Set as local computer name");
                    }
                }
            }
            catch (Exception ex)
            {
                TerminalLogger.Instance.Write("[DBCHECK] LocalNetworkIP Checking Failed! Check Error Logs");
                ErrorLogger.Instance.Write("[DBCHECK] Error when checking localnetworkip on branch table. Exception: "+ex);
            }
            
            #endregion

            #region Run Scripts from FTPServer
            try
            {
                string[] files = Directory.GetFiles(GlobalVariables.FtpScriptPath, "*.sql");
                foreach (string file in files)
                {
                    string script = File.ReadAllText(file);
                    if (SqlQuery.ExecuteScript(script))
                    {
                        TerminalLogger.Instance.Write("[RUNSCRIPT] " + file + " script run successful!");
                        File.Delete(file);
                    }
                    else
                        TerminalLogger.Instance.Write("[RUNSCRIPT] " + file + " script run failed! Check Error Logs.");
                }
            }
            catch (Exception ex)
            {
                TerminalLogger.Instance.Write("[RUNSCRIPT] Running MySql scripts failed! Exception: " + ex);
            }
            #endregion
        }

        private void LogLatestBackUpDate()
        {
            TerminalLogger.Instance.Write("[CHKBACKUP] Autosync backup Checking Started");
            try
            {
                string backupfolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\dumps";
                string[] zipFiles = Directory.GetFiles(backupfolderPath, "*.zip", SearchOption.AllDirectories);
                TerminalLogger.Instance.Write(zipFiles.Length > 0 ? "[BACKUP] Latest AutoSync Backup Date was on " + File.GetCreationTime(zipFiles[0]) : "[BACKUP] AutoSync has no exisiting backup!");
                TerminalLogger.Instance.Write("[CHKBACKUP] Autosync backup Checking Successful!");
            }
            catch (Exception ex)
            {
                TerminalLogger.Instance.Write("[CHKBACKUP] Autosync backup Checking Failed! Check Error Logs.");
                ErrorLogger.Instance.Write("[CHKBACKUP] Error when checking autosync backup. Exception: "+ex);
            }
        }
        private bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }

        private Process CmdProcess(string command)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            return process;
        }
    }
}
