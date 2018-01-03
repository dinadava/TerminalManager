using System;
using System.Diagnostics;
using System.IO;

namespace TerminalManager.Helpers
{
    public class Logger
    {
        public bool AppendDateToFileName { get; protected set; }
        public string FileDirectory { get; protected set; }
        public string FileName { get; protected set; }
        public bool ShowProcessId { get; protected set; }
        public bool ShowDateTime { get; protected set; }

        public Logger(string fileName)
            : this(AppDomain.CurrentDomain.BaseDirectory, fileName)
        {
        }
        public Logger(string fileDirectory, string fileName)
        {
            this.FileDirectory = fileDirectory;
            this.FileName = fileName;
        }
        public void Write(string text)
        {
            string date = DateTime.Now.Date.ToString("yyyyMMdd");
            string tempFileName = this.FileName + (this.AppendDateToFileName ? date : "") + ".txt";

            try
            {
                if (!(Directory.Exists(this.FileDirectory)))
                    Directory.CreateDirectory(this.FileDirectory);
                FileStream fs = new FileStream(this.FileDirectory + "\\" + tempFileName, FileMode.OpenOrCreate,
                    FileAccess.ReadWrite);
                StreamWriter s = new StreamWriter(fs);
                s.Close();
                fs.Close();
            }
            catch (Exception e){ }

            try
            {
                FileStream fs1 = new FileStream(this.FileDirectory + "\\" + tempFileName, FileMode.Append, FileAccess.Write);
                StreamWriter s1 = new StreamWriter(fs1);
                s1.WriteLine((this.ShowDateTime ? ("[" + DateTime.Now + "]") : "") + (this.ShowProcessId ? ("[" + _process.Id + "]") : "") + text);
                s1.Close();
                fs1.Close();
            }
            catch (Exception) { }
        }

        private static readonly Process _process = Process.GetCurrentProcess();
    }
}
