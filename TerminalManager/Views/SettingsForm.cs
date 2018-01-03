using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace TerminalManager.Views
{
    public partial class SettingsForm : Form
    {
        public bool isSuccessful;
        public SettingsForm()
        {
            InitializeComponent();
            isSuccessful = false;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            SetInitialValue();
        }

        private void SetInitialValue()
        {
            checktimeTextBox.Text = Settings.Instance.CheckTime.ToString();
            serverpathTextBox.Text = Settings.Instance.ServerFolderPath;
            pospathTextBox.Text = Settings.Instance.POSFolderPath;
            ftpaddressTextBox.Text = Settings.Instance.FtpAddress;
            ftpusernameTextBox.Text = Settings.Instance.FtpUsername;
            ftppasswordTextBox.Text = Settings.Instance.FtpPassword;
            apiaddressTexBox.Text = Settings.Instance.ApiServerAddress;
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            SetInitialValue();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (checktimeTextBox.Text == "" || ftpusernameTextBox.Text == "" || ftpaddressTextBox.Text == "" ||
                ftppasswordTextBox.Text == "" || apiaddressTexBox.Text == "")
            {
                MessageBox.Show("Incomplete values!");
                return;
            }

            foreach (string time in checktimeTextBox.Text.Split(new string[] { "," } , StringSplitOptions.RemoveEmptyEntries))
            {
                if (!this.IsValidTime(time))
                {
                    MessageBox.Show("Invalid CheckTime value!");
                    return;
                }
            }

            switch (Settings.Instance.Type)
            {
                case 1:
                    if (serverpathTextBox.Text == "")
                    {
                        MessageBox.Show("Invalid Server path!");
                        return;
                    }
                    break;
                case 2:
                    if (serverpathTextBox.Text == "")
                    {
                        MessageBox.Show("Invalid Server path!");
                        return;
                    }
                    if (pospathTextBox.Text == "")
                    {
                        MessageBox.Show("Invalid POS path!");
                        return;
                    }
                    break;
                case 3:
                    if (pospathTextBox.Text == "")
                    {
                        MessageBox.Show("Invalid POS path!");
                        return;
                    }
                    break;
                default: break;
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Settings._xmlFilePath);
            string[] nodes = { "CheckTime", "ServerFolderPath", "PosFolderPath", "FtpAddress", "FtpUsername", "FtpPassword", "ApiServerAddress" };
            string[] value = { checktimeTextBox.Text, serverpathTextBox.Text, pospathTextBox.Text, ftpaddressTextBox.Text, ftpusernameTextBox.Text, ftppasswordTextBox.Text, apiaddressTexBox.Text };
            for (int i = 0; i < nodes.Length; i++)
                xmlDoc.SelectSingleNode("Setting/Application/" + nodes[i]).InnerText = value[i];
            xmlDoc.Save(Settings._xmlFilePath);
            isSuccessful = true;
        }

        public bool IsValidTime(string thetime)
        {
            Regex checktime = new Regex(@"^(20|21|22|23|[01]d|d)(([:][0-5]d){1,2})$");
            return checktime.IsMatch(thetime);
        }
    }
}
