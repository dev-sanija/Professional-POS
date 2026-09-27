namespace PosSystem
{
    partial class Maintenance
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Maintenance));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.Export = new System.Windows.Forms.TabPage();
            this.btnExecuteBackup = new System.Windows.Forms.Button();
            this.chkCompress = new System.Windows.Forms.CheckBox();
            this.txtExportPath = new System.Windows.Forms.TextBox();
            this.btnBrowseExport = new System.Windows.Forms.Button();
            this.Import = new System.Windows.Forms.TabPage();
            this.lblLastBackup = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnExecuteRestore = new System.Windows.Forms.Button();
            this.txtImportPath = new System.Windows.Forms.TextBox();
            this.btnBrowseImport = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.lblCloudStatus = new System.Windows.Forms.Label();
            this.chkAutoSync = new System.Windows.Forms.CheckBox();
            this.btnRefreshGrid = new System.Windows.Forms.Button();
            this.btnSyncNow = new System.Windows.Forms.Button();
            this.btnConnectDrive = new System.Windows.Forms.Button();
            this.DGV1 = new System.Windows.Forms.DataGridView();
            this.colID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAction = new System.Windows.Forms.DataGridViewButtonColumn();
            this.ProgressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.timerCloud = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.Export.SuspendLayout();
            this.Import.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGV1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.Export);
            this.tabControl1.Controls.Add(this.Import);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Font = new System.Drawing.Font("Georgia", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(10, 6);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(745, 400);
            this.tabControl1.TabIndex = 0;
            // 
            // Export
            // 
            this.Export.BackColor = System.Drawing.Color.Ivory;
            this.Export.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Export.Controls.Add(this.btnExecuteBackup);
            this.Export.Controls.Add(this.chkCompress);
            this.Export.Controls.Add(this.txtExportPath);
            this.Export.Controls.Add(this.btnBrowseExport);
            this.Export.ForeColor = System.Drawing.Color.Coral;
            this.Export.Location = new System.Drawing.Point(4, 31);
            this.Export.Name = "Export";
            this.Export.Padding = new System.Windows.Forms.Padding(3);
            this.Export.Size = new System.Drawing.Size(737, 365);
            this.Export.TabIndex = 0;
            this.Export.Text = "Export";
            // 
            // btnExecuteBackup
            // 
            this.btnExecuteBackup.BackColor = System.Drawing.Color.Silver;
            this.btnExecuteBackup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnExecuteBackup.ForeColor = System.Drawing.Color.Black;
            this.btnExecuteBackup.Location = new System.Drawing.Point(312, 199);
            this.btnExecuteBackup.Name = "btnExecuteBackup";
            this.btnExecuteBackup.Size = new System.Drawing.Size(86, 25);
            this.btnExecuteBackup.TabIndex = 3;
            this.btnExecuteBackup.Text = "Backup";
            this.btnExecuteBackup.UseVisualStyleBackColor = false;
            this.btnExecuteBackup.Click += new System.EventHandler(this.btnExecuteBackup_Click);
            // 
            // chkCompress
            // 
            this.chkCompress.AutoSize = true;
            this.chkCompress.BackColor = System.Drawing.Color.Transparent;
            this.chkCompress.Location = new System.Drawing.Point(154, 164);
            this.chkCompress.Name = "chkCompress";
            this.chkCompress.Size = new System.Drawing.Size(115, 20);
            this.chkCompress.TabIndex = 2;
            this.chkCompress.Text = "Compressed";
            this.chkCompress.UseVisualStyleBackColor = false;
            this.chkCompress.CheckedChanged += new System.EventHandler(this.chkCompress_CheckedChanged);
            // 
            // txtExportPath
            // 
            this.txtExportPath.Location = new System.Drawing.Point(154, 136);
            this.txtExportPath.Name = "txtExportPath";
            this.txtExportPath.ReadOnly = true;
            this.txtExportPath.Size = new System.Drawing.Size(341, 22);
            this.txtExportPath.TabIndex = 1;
            this.txtExportPath.TextChanged += new System.EventHandler(this.txtExportPath_TextChanged);
            // 
            // btnBrowseExport
            // 
            this.btnBrowseExport.BackColor = System.Drawing.Color.Silver;
            this.btnBrowseExport.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnBrowseExport.ForeColor = System.Drawing.Color.Black;
            this.btnBrowseExport.Location = new System.Drawing.Point(501, 135);
            this.btnBrowseExport.Name = "btnBrowseExport";
            this.btnBrowseExport.Size = new System.Drawing.Size(86, 25);
            this.btnBrowseExport.TabIndex = 0;
            this.btnBrowseExport.Text = "Browse...";
            this.btnBrowseExport.UseVisualStyleBackColor = false;
            this.btnBrowseExport.Click += new System.EventHandler(this.btnBrowseExport_Click);
            // 
            // Import
            // 
            this.Import.BackColor = System.Drawing.Color.Ivory;
            this.Import.Controls.Add(this.lblLastBackup);
            this.Import.Controls.Add(this.label1);
            this.Import.Controls.Add(this.btnExecuteRestore);
            this.Import.Controls.Add(this.txtImportPath);
            this.Import.Controls.Add(this.btnBrowseImport);
            this.Import.ForeColor = System.Drawing.Color.Coral;
            this.Import.Location = new System.Drawing.Point(4, 31);
            this.Import.Name = "Import";
            this.Import.Padding = new System.Windows.Forms.Padding(3);
            this.Import.Size = new System.Drawing.Size(737, 365);
            this.Import.TabIndex = 1;
            this.Import.Text = "Import";
            // 
            // lblLastBackup
            // 
            this.lblLastBackup.AutoSize = true;
            this.lblLastBackup.ForeColor = System.Drawing.Color.Black;
            this.lblLastBackup.Location = new System.Drawing.Point(251, 168);
            this.lblLastBackup.Name = "lblLastBackup";
            this.lblLastBackup.Size = new System.Drawing.Size(45, 16);
            this.lblLastBackup.TabIndex = 8;
            this.lblLastBackup.Text = "None";
            this.lblLastBackup.Click += new System.EventHandler(this.lblLastBackup_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(152, 168);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 16);
            this.label1.TabIndex = 7;
            this.label1.Text = "Last Backup :";
            // 
            // btnExecuteRestore
            // 
            this.btnExecuteRestore.BackColor = System.Drawing.Color.Silver;
            this.btnExecuteRestore.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnExecuteRestore.ForeColor = System.Drawing.Color.Black;
            this.btnExecuteRestore.Location = new System.Drawing.Point(310, 202);
            this.btnExecuteRestore.Name = "btnExecuteRestore";
            this.btnExecuteRestore.Size = new System.Drawing.Size(86, 25);
            this.btnExecuteRestore.TabIndex = 6;
            this.btnExecuteRestore.Text = "Restore";
            this.btnExecuteRestore.UseVisualStyleBackColor = false;
            this.btnExecuteRestore.Click += new System.EventHandler(this.btnExecuteRestore_Click);
            // 
            // txtImportPath
            // 
            this.txtImportPath.Location = new System.Drawing.Point(152, 139);
            this.txtImportPath.Name = "txtImportPath";
            this.txtImportPath.ReadOnly = true;
            this.txtImportPath.Size = new System.Drawing.Size(341, 22);
            this.txtImportPath.TabIndex = 5;
            this.txtImportPath.TextChanged += new System.EventHandler(this.txtImportPath_TextChanged);
            // 
            // btnBrowseImport
            // 
            this.btnBrowseImport.BackColor = System.Drawing.Color.Silver;
            this.btnBrowseImport.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnBrowseImport.ForeColor = System.Drawing.Color.Black;
            this.btnBrowseImport.Location = new System.Drawing.Point(499, 138);
            this.btnBrowseImport.Name = "btnBrowseImport";
            this.btnBrowseImport.Size = new System.Drawing.Size(86, 25);
            this.btnBrowseImport.TabIndex = 4;
            this.btnBrowseImport.Text = "Browse...";
            this.btnBrowseImport.UseVisualStyleBackColor = false;
            this.btnBrowseImport.Click += new System.EventHandler(this.btnBrowseImport_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.Ivory;
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.lblCloudStatus);
            this.tabPage1.Controls.Add(this.chkAutoSync);
            this.tabPage1.Controls.Add(this.btnRefreshGrid);
            this.tabPage1.Controls.Add(this.btnSyncNow);
            this.tabPage1.Controls.Add(this.btnConnectDrive);
            this.tabPage1.Controls.Add(this.DGV1);
            this.tabPage1.ForeColor = System.Drawing.Color.Coral;
            this.tabPage1.Location = new System.Drawing.Point(4, 31);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(737, 365);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "Connect";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Georgia", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.ForestGreen;
            this.label2.Location = new System.Drawing.Point(4, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 14);
            this.label2.TabIndex = 8;
            this.label2.Text = "Cloud Status :";
            // 
            // lblCloudStatus
            // 
            this.lblCloudStatus.AutoSize = true;
            this.lblCloudStatus.Font = new System.Drawing.Font("Georgia", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCloudStatus.ForeColor = System.Drawing.Color.LimeGreen;
            this.lblCloudStatus.Location = new System.Drawing.Point(101, 3);
            this.lblCloudStatus.Name = "lblCloudStatus";
            this.lblCloudStatus.Size = new System.Drawing.Size(32, 14);
            this.lblCloudStatus.TabIndex = 8;
            this.lblCloudStatus.Text = "N/A";
            this.lblCloudStatus.Click += new System.EventHandler(this.lblCloudStatus_Click);
            // 
            // chkAutoSync
            // 
            this.chkAutoSync.AutoSize = true;
            this.chkAutoSync.Location = new System.Drawing.Point(635, 38);
            this.chkAutoSync.Name = "chkAutoSync";
            this.chkAutoSync.Size = new System.Drawing.Size(100, 20);
            this.chkAutoSync.TabIndex = 7;
            this.chkAutoSync.Text = "Auto Sync";
            this.chkAutoSync.UseVisualStyleBackColor = true;
            this.chkAutoSync.CheckedChanged += new System.EventHandler(this.chkAutoSync_CheckedChanged);
            // 
            // btnRefreshGrid
            // 
            this.btnRefreshGrid.BackColor = System.Drawing.Color.Silver;
            this.btnRefreshGrid.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRefreshGrid.ForeColor = System.Drawing.Color.Black;
            this.btnRefreshGrid.Location = new System.Drawing.Point(444, 6);
            this.btnRefreshGrid.Name = "btnRefreshGrid";
            this.btnRefreshGrid.Size = new System.Drawing.Size(86, 25);
            this.btnRefreshGrid.TabIndex = 6;
            this.btnRefreshGrid.Text = "Refresh";
            this.btnRefreshGrid.UseVisualStyleBackColor = false;
            this.btnRefreshGrid.Click += new System.EventHandler(this.btnRefreshGrid_Click);
            // 
            // btnSyncNow
            // 
            this.btnSyncNow.BackColor = System.Drawing.Color.Silver;
            this.btnSyncNow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSyncNow.ForeColor = System.Drawing.Color.Black;
            this.btnSyncNow.Location = new System.Drawing.Point(535, 6);
            this.btnSyncNow.Name = "btnSyncNow";
            this.btnSyncNow.Size = new System.Drawing.Size(86, 25);
            this.btnSyncNow.TabIndex = 6;
            this.btnSyncNow.Text = "Sync Now";
            this.btnSyncNow.UseVisualStyleBackColor = false;
            this.btnSyncNow.Click += new System.EventHandler(this.btnSyncNow_Click);
            // 
            // btnConnectDrive
            // 
            this.btnConnectDrive.BackColor = System.Drawing.Color.ForestGreen;
            this.btnConnectDrive.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnConnectDrive.ForeColor = System.Drawing.Color.Black;
            this.btnConnectDrive.Location = new System.Drawing.Point(626, 6);
            this.btnConnectDrive.Name = "btnConnectDrive";
            this.btnConnectDrive.Size = new System.Drawing.Size(105, 25);
            this.btnConnectDrive.TabIndex = 5;
            this.btnConnectDrive.Text = "Connect";
            this.btnConnectDrive.UseVisualStyleBackColor = false;
            this.btnConnectDrive.Click += new System.EventHandler(this.btnConnectDrive_Click);
            // 
            // DGV1
            // 
            this.DGV1.AllowUserToAddRows = false;
            this.DGV1.AllowUserToDeleteRows = false;
            this.DGV1.AllowUserToResizeColumns = false;
            this.DGV1.AllowUserToResizeRows = false;
            this.DGV1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colID,
            this.colName,
            this.colDate,
            this.colSize,
            this.colAction});
            this.DGV1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.DGV1.Location = new System.Drawing.Point(0, 79);
            this.DGV1.MultiSelect = false;
            this.DGV1.Name = "DGV1";
            this.DGV1.ReadOnly = true;
            this.DGV1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.DGV1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DGV1.Size = new System.Drawing.Size(734, 290);
            this.DGV1.TabIndex = 0;
            this.DGV1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGV1_CellContentClick_1);
            // 
            // colID
            // 
            this.colID.FillWeight = 5F;
            this.colID.HeaderText = "File ID";
            this.colID.Name = "colID";
            this.colID.ReadOnly = true;
            this.colID.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colID.Visible = false;
            this.colID.Width = 5;
            // 
            // colName
            // 
            this.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colName.HeaderText = "Backup Name";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            // 
            // colDate
            // 
            this.colDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colDate.HeaderText = "Upload Date";
            this.colDate.Name = "colDate";
            this.colDate.ReadOnly = true;
            this.colDate.Width = 150;
            // 
            // colSize
            // 
            this.colSize.HeaderText = "File Size";
            this.colSize.Name = "colSize";
            this.colSize.ReadOnly = true;
            // 
            // colAction
            // 
            this.colAction.HeaderText = "Restore";
            this.colAction.Name = "colAction";
            this.colAction.ReadOnly = true;
            this.colAction.Width = 150;
            // 
            // ProgressBar1
            // 
            this.ProgressBar1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.ProgressBar1.ForeColor = System.Drawing.Color.Chartreuse;
            this.ProgressBar1.Location = new System.Drawing.Point(0, 399);
            this.ProgressBar1.Name = "ProgressBar1";
            this.ProgressBar1.Size = new System.Drawing.Size(745, 23);
            this.ProgressBar1.TabIndex = 1;
            this.ProgressBar1.Click += new System.EventHandler(this.ProgressBar1_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.Font = new System.Drawing.Font("Georgia", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.Location = new System.Drawing.Point(2, 404);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(48, 15);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Status";
            this.lblStatus.Click += new System.EventHandler(this.lblStatus_Click);
            // 
            // timerCloud
            // 
            this.timerCloud.Interval = 1800000;
            // 
            // Maintenance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(745, 421);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.ProgressBar1);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Maintenance";
            this.ShowInTaskbar = false;
            this.Text = "Maintenance";
            this.tabControl1.ResumeLayout(false);
            this.Export.ResumeLayout(false);
            this.Export.PerformLayout();
            this.Import.ResumeLayout(false);
            this.Import.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGV1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage Export;
        private System.Windows.Forms.TabPage Import;
        private System.Windows.Forms.TextBox txtExportPath;
        private System.Windows.Forms.Button btnBrowseExport;
        private System.Windows.Forms.Button btnExecuteBackup;
        private System.Windows.Forms.CheckBox chkCompress;
        private System.Windows.Forms.ProgressBar ProgressBar1;
        private System.Windows.Forms.Button btnExecuteRestore;
        private System.Windows.Forms.TextBox txtImportPath;
        private System.Windows.Forms.Button btnBrowseImport;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblLastBackup;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.DataGridView DGV1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSize;
        private System.Windows.Forms.DataGridViewButtonColumn colAction;
        private System.Windows.Forms.CheckBox chkAutoSync;
        private System.Windows.Forms.Button btnRefreshGrid;
        private System.Windows.Forms.Button btnSyncNow;
        private System.Windows.Forms.Button btnConnectDrive;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblCloudStatus;
        private System.Windows.Forms.Timer timerCloud;
    }
}