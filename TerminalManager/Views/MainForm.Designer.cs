namespace TerminalManager
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runningTimer = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.ServersStatusLabel = new System.Windows.Forms.Label();
            this.RefreshButton = new System.Windows.Forms.Button();
            this.UploadLogsButton = new System.Windows.Forms.Button();
            this.ManualCheckButton = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.labelStatus = new System.Windows.Forms.Label();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._statusStrip = new TerminalManager.Views.MainFormStatusStrip();
            this._statuscheckerBackgroundWorker = new TerminalManager.BackgroundWorkers.StatusCheckerBackgroundWorker();
            this._terminalmanagerBackgroundWorker = new TerminalManager.BackgroundWorkers.TerminalBackgroundWorker();
            this.contextMenuStrip.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Terminal Manager";
            this.notifyIcon.Visible = true;
            this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_DoubleClick);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(93, 26);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // runningTimer
            // 
            this.runningTimer.Tick += new System.EventHandler(this.runningTimer_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(18, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server State:";
            // 
            // ServersStatusLabel
            // 
            this.ServersStatusLabel.AutoSize = true;
            this.ServersStatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ServersStatusLabel.ForeColor = System.Drawing.Color.Green;
            this.ServersStatusLabel.Location = new System.Drawing.Point(110, 34);
            this.ServersStatusLabel.Name = "ServersStatusLabel";
            this.ServersStatusLabel.Size = new System.Drawing.Size(52, 16);
            this.ServersStatusLabel.TabIndex = 2;
            this.ServersStatusLabel.Text = "Online";
            // 
            // RefreshButton
            // 
            this.RefreshButton.Location = new System.Drawing.Point(124, 107);
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(101, 45);
            this.RefreshButton.TabIndex = 4;
            this.RefreshButton.Text = "Refresh State";
            this.RefreshButton.UseVisualStyleBackColor = true;
            this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // UploadLogsButton
            // 
            this.UploadLogsButton.Location = new System.Drawing.Point(313, 107);
            this.UploadLogsButton.Name = "UploadLogsButton";
            this.UploadLogsButton.Size = new System.Drawing.Size(101, 45);
            this.UploadLogsButton.TabIndex = 5;
            this.UploadLogsButton.Text = "Upload Logs";
            this.UploadLogsButton.UseVisualStyleBackColor = true;
            this.UploadLogsButton.Click += new System.EventHandler(this.UploadLogsButton_Click);
            // 
            // ManualCheckButton
            // 
            this.ManualCheckButton.Location = new System.Drawing.Point(19, 107);
            this.ManualCheckButton.Name = "ManualCheckButton";
            this.ManualCheckButton.Size = new System.Drawing.Size(101, 45);
            this.ManualCheckButton.TabIndex = 6;
            this.ManualCheckButton.Text = "Manual Check";
            this.ManualCheckButton.UseVisualStyleBackColor = true;
            this.ManualCheckButton.Click += new System.EventHandler(this.ManualCheckButton_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(20, 75);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(394, 24);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 8;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(17, 57);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(41, 15);
            this.labelStatus.TabIndex = 9;
            this.labelStatus.Text = "Status";
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(436, 24);
            this.menuStrip.TabIndex = 10;
            this.menuStrip.Text = "Settings";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // _statusStrip
            // 
            this._statusStrip.Location = new System.Drawing.Point(0, 167);
            this._statusStrip.Name = "_statusStrip";
            this._statusStrip.Size = new System.Drawing.Size(436, 22);
            this._statusStrip.TabIndex = 3;
            this._statusStrip.Text = "statusStrip1";
            // 
            // _statuscheckerBackgroundWorker
            // 
            this._statuscheckerBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this._statuscheckerBackgroundWorker_RunWorkerCompleted);
            // 
            // _terminalmanagerBackgroundWorker
            // 
            this._terminalmanagerBackgroundWorker.WorkerReportsProgress = true;
            this._terminalmanagerBackgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this._terminalmanagerBackgroundWorker_ProgressChanged);
            this._terminalmanagerBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this._terminalmanagerBackgroundWorker_RunWorkerCompleted);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(436, 189);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.ManualCheckButton);
            this.Controls.Add(this.UploadLogsButton);
            this.Controls.Add(this.RefreshButton);
            this.Controls.Add(this._statusStrip);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.ServersStatusLabel);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.Text = "TerminalManager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.contextMenuStrip.ResumeLayout(false);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.Timer runningTimer;
        private BackgroundWorkers.TerminalBackgroundWorker _terminalmanagerBackgroundWorker;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label ServersStatusLabel;
        private BackgroundWorkers.StatusCheckerBackgroundWorker _statuscheckerBackgroundWorker;
        private Views.MainFormStatusStrip _statusStrip;
        private System.Windows.Forms.Button RefreshButton;
        private System.Windows.Forms.Button UploadLogsButton;
        private System.Windows.Forms.Button ManualCheckButton;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
    }
}

