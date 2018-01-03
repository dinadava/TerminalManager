using System;
using System.Windows.Forms;

namespace TerminalManager.Views
{
    public partial class PermissionForm : Form
    {
        public PermissionForm()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            if (PasswordTexBox.Text.Equals("121586"))
            {
                this.DialogResult = DialogResult.OK;
                Close();
            }
            else
                MessageBox.Show("Incorrect Password!");
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void PermissionForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                OKButton.PerformClick();
        }
    }
}
