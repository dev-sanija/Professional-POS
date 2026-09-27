
namespace PosSystem
{
    partial class frmscanBarcode
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmscanBarcode));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.cbCamara = new MetroFramework.Controls.MetroComboBox();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnReStart = new System.Windows.Forms.Button();
            this.metroComboBoxMPlevel = new MetroFramework.Controls.MetroComboBox();
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(158)))), ((int)(((byte)(132)))));
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(-2, -1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(607, 38);
            this.panel1.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(4, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(109, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "Scan Barcode";
            // 
            // cbCamara
            // 
            this.cbCamara.FormattingEnabled = true;
            this.cbCamara.ItemHeight = 23;
            this.cbCamara.Location = new System.Drawing.Point(65, 66);
            this.cbCamara.Name = "cbCamara";
            this.cbCamara.Size = new System.Drawing.Size(176, 29);
            this.cbCamara.TabIndex = 2;
            this.cbCamara.UseSelectable = true;
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(6, 101);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(312, 180);
            this.pictureBox.TabIndex = 3;
            this.pictureBox.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(119, 42);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 21);
            this.label1.TabIndex = 4;
            this.label1.Text = "Camera";
            // 
            // btnStart
            // 
            this.btnStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.btnStart.FlatAppearance.BorderSize = 0;
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.ForeColor = System.Drawing.Color.White;
            this.btnStart.Location = new System.Drawing.Point(12, 287);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 30);
            this.btnStart.TabIndex = 15;
            this.btnStart.Text = "Start!";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnReStart
            // 
            this.btnReStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.btnReStart.FlatAppearance.BorderSize = 0;
            this.btnReStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReStart.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReStart.ForeColor = System.Drawing.Color.White;
            this.btnReStart.Location = new System.Drawing.Point(109, 287);
            this.btnReStart.Name = "btnReStart";
            this.btnReStart.Size = new System.Drawing.Size(75, 30);
            this.btnReStart.TabIndex = 16;
            this.btnReStart.Text = "Restart";
            this.btnReStart.UseVisualStyleBackColor = false;
            // 
            // metroComboBoxMPlevel
            // 
            this.metroComboBoxMPlevel.FormattingEnabled = true;
            this.metroComboBoxMPlevel.ItemHeight = 23;
            this.metroComboBoxMPlevel.Location = new System.Drawing.Point(212, 288);
            this.metroComboBoxMPlevel.Name = "metroComboBoxMPlevel";
            this.metroComboBoxMPlevel.Size = new System.Drawing.Size(97, 29);
            this.metroComboBoxMPlevel.TabIndex = 17;
            this.metroComboBoxMPlevel.UseSelectable = true;
            this.metroComboBoxMPlevel.SelectedIndexChanged += new System.EventHandler(this.metroComboBoxMPlevel_SelectedIndexChanged);
            // 
            // pnlStatus
            // 
            this.pnlStatus.Location = new System.Drawing.Point(252, 74);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(15, 15);
            this.pnlStatus.TabIndex = 18;
            // 
            // frmscanBarcode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(321, 329);
            this.Controls.Add(this.pnlStatus);
            this.Controls.Add(this.metroComboBoxMPlevel);
            this.Controls.Add(this.btnReStart);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.cbCamara);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmscanBarcode";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmscanBarcode_FormClosing);
            this.Load += new System.EventHandler(this.frmscanBarcode_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private MetroFramework.Controls.MetroComboBox cbCamara;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Button btnStart;
        public System.Windows.Forms.Button btnReStart;
        private MetroFramework.Controls.MetroComboBox metroComboBoxMPlevel;
        private System.Windows.Forms.Panel pnlStatus;
    }
}