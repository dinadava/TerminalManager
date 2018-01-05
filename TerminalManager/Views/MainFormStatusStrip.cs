using System.ComponentModel;
using System.Windows.Forms;
using TerminalManager.Entities;

namespace TerminalManager.Views
{
    public class MainFormStatusStrip : StatusStrip
    {
        private ToolStripStatusLabel terminalTypeLabel;
        private ToolStripStatusLabel branchIdStatusLabel;
        private ToolStripStatusLabel applicationVersionLabel;

        public MainFormStatusStrip()
        {
            terminalTypeLabel = new ToolStripStatusLabel();
            branchIdStatusLabel = new ToolStripStatusLabel();
            applicationVersionLabel = new ToolStripStatusLabel();
        }

        public void Start()
        {
            // 
            // terminalTypeLabel
            // 
            terminalTypeLabel.ForeColor = System.Drawing.Color.Black;
            terminalTypeLabel.Name = "terminalTypeLabel";
            terminalTypeLabel.Size = new System.Drawing.Size(188, 17);
            terminalTypeLabel.Spring = true;
            terminalTypeLabel.Text = "Type - TerminalType";
            // 
            // branchIdStatusLabel
            // 
            branchIdStatusLabel.ForeColor = System.Drawing.Color.Black;
            branchIdStatusLabel.Name = "branchIdStatusLabel";
            branchIdStatusLabel.Size = new System.Drawing.Size(188, 17);
            branchIdStatusLabel.Spring = true;
            branchIdStatusLabel.Text = "XX - BranchName";
            // 
            // applicationVersionLabel
            // 
            applicationVersionLabel.ForeColor = System.Drawing.Color.Black;
            applicationVersionLabel.Name = "applicationVersionLabel";
            applicationVersionLabel.Size = new System.Drawing.Size(188, 17);
            applicationVersionLabel.Spring = true;
            applicationVersionLabel.Text = "TerminalManager X.X.X.X";

            BackColor = System.Drawing.SystemColors.Control;
            Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold);
            ForeColor = System.Drawing.Color.White;
            Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                applicationVersionLabel,
                branchIdStatusLabel,
                terminalTypeLabel
            });
            Location = new System.Drawing.Point(0, 500);
            Name = "statusStrip";
            RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            Size = new System.Drawing.Size(800, 22);
            SizingGrip = false;
            TabIndex = 25;
            Text = "statusStrip";

            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
                applicationVersionLabel.Text = GlobalVariables.ApplicationNameAndVersion;

            branchIdStatusLabel.Text = Settings.Instance.BranchId + " - " + GlobalVariables.BranchName;
            terminalTypeLabel.Text = "Type - " + (Enums.TerminalType)Settings.Instance.Type;
        }
    }
}
