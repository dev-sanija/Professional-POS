namespace PosSystem
{
    partial class frmPOS
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPOS));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnSales = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSattle = new System.Windows.Forms.Button();
            this.btnDiscount = new System.Windows.Forms.Button();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnTrans = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnReprint = new System.Windows.Forms.Button();
            this.comboBoxprinter = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.textBoxbarcode = new System.Windows.Forms.TextBox();
            this.btnscanbarcode = new System.Windows.Forms.Button();
            this.label16 = new System.Windows.Forms.Label();
            this.lblDate = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblTransno = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panel6 = new System.Windows.Forms.Panel();
            this.lblVat = new System.Windows.Forms.Label();
            this.lblVatable = new System.Windows.Forms.Label();
            this.lblDiscount = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.lblPhone = new System.Windows.Forms.Label();
            this.lblAddress = new System.Windows.Forms.Label();
            this.lblSname = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblDisplayTotal = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.LblUser = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColRemove = new System.Windows.Forms.DataGridViewImageColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.collAdd = new System.Windows.Forms.DataGridViewImageColumn();
            this.Delete = new System.Windows.Forms.DataGridViewImageColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.discount_per_unit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.dataGridViewImageColumn1 = new System.Windows.Forms.DataGridViewImageColumn();
            this.dataGridViewImageColumn2 = new System.Windows.Forms.DataGridViewImageColumn();
            this.dataGridViewImageColumn3 = new System.Windows.Forms.DataGridViewImageColumn();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(158)))), ((int)(((byte)(132)))));
            this.label1.Location = new System.Drawing.Point(120, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "POS Software";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Controls.Add(this.btnSales);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnSattle);
            this.panel1.Controls.Add(this.btnDiscount);
            this.panel1.Controls.Add(this.btnSearch);
            this.panel1.Controls.Add(this.btnTrans);
            this.panel1.Location = new System.Drawing.Point(760, 58);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(247, 274);
            this.panel1.TabIndex = 0;
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Image = ((System.Drawing.Image)(resources.GetObject("btnClose.Image")));
            this.btnClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClose.Location = new System.Drawing.Point(0, 234);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(247, 39);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = " [F10] Close";
            this.btnClose.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnSales
            // 
            this.btnSales.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(158)))), ((int)(((byte)(132)))));
            this.btnSales.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSales.FlatAppearance.BorderSize = 0;
            this.btnSales.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSales.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSales.ForeColor = System.Drawing.Color.White;
            this.btnSales.Image = ((System.Drawing.Image)(resources.GetObject("btnSales.Image")));
            this.btnSales.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSales.Location = new System.Drawing.Point(0, 195);
            this.btnSales.Name = "btnSales";
            this.btnSales.Size = new System.Drawing.Size(247, 39);
            this.btnSales.TabIndex = 5;
            this.btnSales.Text = " [F6] Daily Sales";
            this.btnSales.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSales.UseVisualStyleBackColor = false;
            this.btnSales.Click += new System.EventHandler(this.btnSales_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(158)))), ((int)(((byte)(132)))));
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(0, 156);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(247, 39);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = " [F5] Clear/Cancel";
            this.btnCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSattle
            // 
            this.btnSattle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(158)))), ((int)(((byte)(132)))));
            this.btnSattle.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSattle.FlatAppearance.BorderSize = 0;
            this.btnSattle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSattle.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSattle.ForeColor = System.Drawing.Color.White;
            this.btnSattle.Image = ((System.Drawing.Image)(resources.GetObject("btnSattle.Image")));
            this.btnSattle.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSattle.Location = new System.Drawing.Point(0, 117);
            this.btnSattle.Name = "btnSattle";
            this.btnSattle.Size = new System.Drawing.Size(247, 39);
            this.btnSattle.TabIndex = 3;
            this.btnSattle.Text = " [F4] Sattle Payment";
            this.btnSattle.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSattle.UseVisualStyleBackColor = false;
            this.btnSattle.Click += new System.EventHandler(this.btnSattle_Click);
            // 
            // btnDiscount
            // 
            this.btnDiscount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(158)))), ((int)(((byte)(132)))));
            this.btnDiscount.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnDiscount.FlatAppearance.BorderSize = 0;
            this.btnDiscount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDiscount.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDiscount.ForeColor = System.Drawing.Color.White;
            this.btnDiscount.Image = ((System.Drawing.Image)(resources.GetObject("btnDiscount.Image")));
            this.btnDiscount.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDiscount.Location = new System.Drawing.Point(0, 78);
            this.btnDiscount.Name = "btnDiscount";
            this.btnDiscount.Size = new System.Drawing.Size(247, 39);
            this.btnDiscount.TabIndex = 2;
            this.btnDiscount.Text = " [F3] Add Discount";
            this.btnDiscount.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnDiscount.UseVisualStyleBackColor = false;
            this.btnDiscount.Click += new System.EventHandler(this.btnDiscount_Click);
            // 
            // btnSearch
            // 
            this.btnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(158)))), ((int)(((byte)(132)))));
            this.btnSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSearch.ForeColor = System.Drawing.Color.White;
            this.btnSearch.Image = ((System.Drawing.Image)(resources.GetObject("btnSearch.Image")));
            this.btnSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSearch.Location = new System.Drawing.Point(0, 39);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(247, 39);
            this.btnSearch.TabIndex = 1;
            this.btnSearch.Text = " [F2] Search Product";
            this.btnSearch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnTrans
            // 
            this.btnTrans.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(156)))), ((int)(((byte)(18)))));
            this.btnTrans.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnTrans.FlatAppearance.BorderSize = 0;
            this.btnTrans.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTrans.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTrans.ForeColor = System.Drawing.Color.White;
            this.btnTrans.Image = ((System.Drawing.Image)(resources.GetObject("btnTrans.Image")));
            this.btnTrans.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTrans.Location = new System.Drawing.Point(0, 0);
            this.btnTrans.Name = "btnTrans";
            this.btnTrans.Size = new System.Drawing.Size(247, 39);
            this.btnTrans.TabIndex = 0;
            this.btnTrans.Text = " [F1] New Transaction";
            this.btnTrans.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnTrans.UseVisualStyleBackColor = false;
            this.btnTrans.Click += new System.EventHandler(this.btnTrans_Click);
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.BackColor = System.Drawing.Color.White;
            this.panel3.Controls.Add(this.btnReprint);
            this.panel3.Controls.Add(this.comboBoxprinter);
            this.panel3.Controls.Add(this.button1);
            this.panel3.Controls.Add(this.textBoxbarcode);
            this.panel3.Controls.Add(this.btnscanbarcode);
            this.panel3.Controls.Add(this.label16);
            this.panel3.Controls.Add(this.lblDate);
            this.panel3.Controls.Add(this.label5);
            this.panel3.Controls.Add(this.lblTransno);
            this.panel3.Controls.Add(this.label3);
            this.panel3.Location = new System.Drawing.Point(0, 59);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(758, 89);
            this.panel3.TabIndex = 1;
            // 
            // btnReprint
            // 
            this.btnReprint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReprint.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.btnReprint.FlatAppearance.BorderSize = 0;
            this.btnReprint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReprint.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReprint.ForeColor = System.Drawing.Color.White;
            this.btnReprint.Location = new System.Drawing.Point(543, 5);
            this.btnReprint.Name = "btnReprint";
            this.btnReprint.Size = new System.Drawing.Size(75, 30);
            this.btnReprint.TabIndex = 21;
            this.btnReprint.Text = "Reprint";
            this.btnReprint.UseVisualStyleBackColor = false;
            this.btnReprint.Click += new System.EventHandler(this.btnReprint_Click_1);
            // 
            // comboBoxprinter
            // 
            this.comboBoxprinter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxprinter.FormattingEnabled = true;
            this.comboBoxprinter.Location = new System.Drawing.Point(624, 5);
            this.comboBoxprinter.Name = "comboBoxprinter";
            this.comboBoxprinter.Size = new System.Drawing.Size(121, 28);
            this.comboBoxprinter.TabIndex = 20;
            this.comboBoxprinter.SelectedIndexChanged += new System.EventHandler(this.comboBoxprinter_SelectedIndexChanged_1);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(306, 51);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 30);
            this.button1.TabIndex = 19;
            this.button1.Text = "Done";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBoxbarcode
            // 
            this.textBoxbarcode.Location = new System.Drawing.Point(140, 52);
            this.textBoxbarcode.Name = "textBoxbarcode";
            this.textBoxbarcode.Size = new System.Drawing.Size(160, 27);
            this.textBoxbarcode.TabIndex = 18;
            this.textBoxbarcode.TextChanged += new System.EventHandler(this.textBoxbarcode_TextChanged);
            // 
            // btnscanbarcode
            // 
            this.btnscanbarcode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.btnscanbarcode.FlatAppearance.BorderSize = 0;
            this.btnscanbarcode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnscanbarcode.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnscanbarcode.ForeColor = System.Drawing.Color.White;
            this.btnscanbarcode.Location = new System.Drawing.Point(387, 51);
            this.btnscanbarcode.Name = "btnscanbarcode";
            this.btnscanbarcode.Size = new System.Drawing.Size(75, 30);
            this.btnscanbarcode.TabIndex = 17;
            this.btnscanbarcode.Text = "Scan";
            this.btnscanbarcode.UseVisualStyleBackColor = false;
            this.btnscanbarcode.Click += new System.EventHandler(this.btnscanbarcode_Click);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(17, 55);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(95, 20);
            this.label16.TabIndex = 15;
            this.label16.Text = "[F8] Barcode";
            // 
            // lblDate
            // 
            this.lblDate.AutoSize = true;
            this.lblDate.Location = new System.Drawing.Point(135, 28);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(121, 20);
            this.lblDate.TabIndex = 1;
            this.lblDate.Text = "00000000000000";
            this.lblDate.Click += new System.EventHandler(this.lblDate_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 28);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 20);
            this.label5.TabIndex = 0;
            this.label5.Text = "Date";
            // 
            // lblTransno
            // 
            this.lblTransno.AutoSize = true;
            this.lblTransno.ForeColor = System.Drawing.Color.Red;
            this.lblTransno.Location = new System.Drawing.Point(135, 4);
            this.lblTransno.Name = "lblTransno";
            this.lblTransno.Size = new System.Drawing.Size(121, 20);
            this.lblTransno.TabIndex = 1;
            this.lblTransno.Text = "00000000000000";
            this.lblTransno.Click += new System.EventHandler(this.lblTransno_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(112, 20);
            this.label3.TabIndex = 0;
            this.label3.Text = "Transaction No";
            // 
            // panel6
            // 
            this.panel6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panel6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.panel6.Controls.Add(this.lblVat);
            this.panel6.Controls.Add(this.lblVatable);
            this.panel6.Controls.Add(this.lblDiscount);
            this.panel6.Controls.Add(this.lblTotal);
            this.panel6.Controls.Add(this.label12);
            this.panel6.Controls.Add(this.label14);
            this.panel6.Controls.Add(this.label10);
            this.panel6.Controls.Add(this.label8);
            this.panel6.Location = new System.Drawing.Point(487, 501);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(268, 109);
            this.panel6.TabIndex = 3;
            this.panel6.Paint += new System.Windows.Forms.PaintEventHandler(this.panel6_Paint);
            // 
            // lblVat
            // 
            this.lblVat.Location = new System.Drawing.Point(131, 58);
            this.lblVat.Name = "lblVat";
            this.lblVat.Size = new System.Drawing.Size(121, 20);
            this.lblVat.TabIndex = 1;
            this.lblVat.Text = "0.00";
            this.lblVat.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblVat.Click += new System.EventHandler(this.lblVat_Click);
            // 
            // lblVatable
            // 
            this.lblVatable.Location = new System.Drawing.Point(131, 86);
            this.lblVatable.Name = "lblVatable";
            this.lblVatable.Size = new System.Drawing.Size(121, 20);
            this.lblVatable.TabIndex = 1;
            this.lblVatable.Text = "0.00";
            this.lblVatable.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblVatable.Click += new System.EventHandler(this.lblVatable_Click);
            // 
            // lblDiscount
            // 
            this.lblDiscount.Location = new System.Drawing.Point(132, 32);
            this.lblDiscount.Name = "lblDiscount";
            this.lblDiscount.Size = new System.Drawing.Size(121, 20);
            this.lblDiscount.TabIndex = 1;
            this.lblDiscount.Text = "0.00";
            this.lblDiscount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblDiscount.Click += new System.EventHandler(this.lblDiscount_Click);
            // 
            // lblTotal
            // 
            this.lblTotal.Location = new System.Drawing.Point(132, 5);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(121, 20);
            this.lblTotal.TabIndex = 1;
            this.lblTotal.Text = "0.00";
            this.lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblTotal.Click += new System.EventHandler(this.lblTotal_Click);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(7, 58);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(31, 20);
            this.label12.TabIndex = 0;
            this.label12.Text = "Vat";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 83);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(46, 20);
            this.label14.TabIndex = 0;
            this.label14.Text = "Total ";
            this.label14.Click += new System.EventHandler(this.label14_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 32);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(69, 20);
            this.label10.TabIndex = 0;
            this.label10.Text = "Discount";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 5);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(80, 20);
            this.label8.TabIndex = 0;
            this.label8.Text = "Sales Total";
            // 
            // panel5
            // 
            this.panel5.AccessibleRole = System.Windows.Forms.AccessibleRole.Cursor;
            this.panel5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.panel5.Controls.Add(this.label4);
            this.panel5.Controls.Add(this.lblPhone);
            this.panel5.Controls.Add(this.lblAddress);
            this.panel5.Controls.Add(this.lblSname);
            this.panel5.Controls.Add(this.lblName);
            this.panel5.Controls.Add(this.lblDisplayTotal);
            this.panel5.Controls.Add(this.label1);
            this.panel5.Controls.Add(this.pictureBox1);
            this.panel5.Controls.Add(this.LblUser);
            this.panel5.Location = new System.Drawing.Point(0, 0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(1012, 60);
            this.panel5.TabIndex = 2;
            // 
            // lblPhone
            // 
            this.lblPhone.AutoSize = true;
            this.lblPhone.Location = new System.Drawing.Point(779, 11);
            this.lblPhone.Name = "lblPhone";
            this.lblPhone.Size = new System.Drawing.Size(51, 20);
            this.lblPhone.TabIndex = 23;
            this.lblPhone.Text = "label4";
            this.lblPhone.Visible = false;
            this.lblPhone.Click += new System.EventHandler(this.lblPhone_Click);
            // 
            // lblAddress
            // 
            this.lblAddress.AutoSize = true;
            this.lblAddress.Location = new System.Drawing.Point(711, 9);
            this.lblAddress.Name = "lblAddress";
            this.lblAddress.Size = new System.Drawing.Size(51, 20);
            this.lblAddress.TabIndex = 22;
            this.lblAddress.Text = "label4";
            this.lblAddress.Visible = false;
            this.lblAddress.Click += new System.EventHandler(this.lblAddress_Click);
            // 
            // lblSname
            // 
            this.lblSname.AutoSize = true;
            this.lblSname.Location = new System.Drawing.Point(711, 9);
            this.lblSname.Name = "lblSname";
            this.lblSname.Size = new System.Drawing.Size(51, 20);
            this.lblSname.TabIndex = 21;
            this.lblSname.Text = "label4";
            this.lblSname.Visible = false;
            this.lblSname.Click += new System.EventHandler(this.lblSname_Click);
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.ForeColor = System.Drawing.Color.White;
            this.lblName.Location = new System.Drawing.Point(168, 29);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(53, 20);
            this.lblName.TabIndex = 2;
            this.lblName.Text = "STORE";
            this.lblName.Click += new System.EventHandler(this.lblName_Click);
            // 
            // lblDisplayTotal
            // 
            this.lblDisplayTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDisplayTotal.Font = new System.Drawing.Font("Consolas", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDisplayTotal.ForeColor = System.Drawing.Color.LawnGreen;
            this.lblDisplayTotal.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblDisplayTotal.Location = new System.Drawing.Point(699, 0);
            this.lblDisplayTotal.Name = "lblDisplayTotal";
            this.lblDisplayTotal.Size = new System.Drawing.Size(310, 57);
            this.lblDisplayTotal.TabIndex = 0;
            this.lblDisplayTotal.Text = "0.00";
            this.lblDisplayTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblDisplayTotal.Click += new System.EventHandler(this.label7_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(7, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(107, 54);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // LblUser
            // 
            this.LblUser.AutoSize = true;
            this.LblUser.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblUser.ForeColor = System.Drawing.Color.Black;
            this.LblUser.Location = new System.Drawing.Point(583, 11);
            this.LblUser.Name = "LblUser";
            this.LblUser.Size = new System.Drawing.Size(53, 20);
            this.LblUser.TabIndex = 3;
            this.LblUser.Text = "STORE";
            this.LblUser.Visible = false;
            this.LblUser.Click += new System.EventHandler(this.LblUser_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column8,
            this.Column3,
            this.ColRemove,
            this.Column5,
            this.collAdd,
            this.Delete,
            this.Column4,
            this.Column7,
            this.Column6,
            this.discount_per_unit});
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.LemonChiffon;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.Desktop;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridView1.EnableHeadersVisualStyles = false;
            this.dataGridView1.Location = new System.Drawing.Point(2, 148);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(754, 349);
            this.dataGridView1.TabIndex = 3;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dataGridView1.SelectionChanged += new System.EventHandler(this.dataGridView1_SelectionChanged);
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Column1.HeaderText = "#";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 40;
            // 
            // Column2
            // 
            this.Column2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Column2.HeaderText = "ID";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Visible = false;
            this.Column2.Width = 49;
            // 
            // Column8
            // 
            this.Column8.HeaderText = "PCode";
            this.Column8.Name = "Column8";
            this.Column8.ReadOnly = true;
            this.Column8.Visible = false;
            this.Column8.Width = 78;
            // 
            // Column3
            // 
            this.Column3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column3.HeaderText = "DESCRIPTION";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            // 
            // ColRemove
            // 
            this.ColRemove.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColRemove.HeaderText = "";
            this.ColRemove.Image = ((System.Drawing.Image)(resources.GetObject("ColRemove.Image")));
            this.ColRemove.Name = "ColRemove";
            this.ColRemove.ReadOnly = true;
            this.ColRemove.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ColRemove.Width = 30;
            // 
            // Column5
            // 
            this.Column5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Column5.DefaultCellStyle = dataGridViewCellStyle2;
            this.Column5.HeaderText = "QTY";
            this.Column5.Name = "Column5";
            this.Column5.ReadOnly = true;
            this.Column5.Width = 61;
            // 
            // collAdd
            // 
            this.collAdd.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.collAdd.HeaderText = "";
            this.collAdd.Image = ((System.Drawing.Image)(resources.GetObject("collAdd.Image")));
            this.collAdd.Name = "collAdd";
            this.collAdd.ReadOnly = true;
            this.collAdd.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.collAdd.Width = 30;
            // 
            // Delete
            // 
            this.Delete.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Delete.HeaderText = "";
            this.Delete.Image = ((System.Drawing.Image)(resources.GetObject("Delete.Image")));
            this.Delete.Name = "Delete";
            this.Delete.ReadOnly = true;
            this.Delete.Width = 30;
            // 
            // Column4
            // 
            this.Column4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.Column4.DefaultCellStyle = dataGridViewCellStyle3;
            this.Column4.HeaderText = "PRICE";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.Width = 150;
            // 
            // Column7
            // 
            this.Column7.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.Column7.DefaultCellStyle = dataGridViewCellStyle4;
            this.Column7.HeaderText = "DISCOUNT";
            this.Column7.Name = "Column7";
            this.Column7.ReadOnly = true;
            this.Column7.Width = 150;
            // 
            // Column6
            // 
            this.Column6.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.Column6.DefaultCellStyle = dataGridViewCellStyle5;
            this.Column6.HeaderText = "TOTAL";
            this.Column6.Name = "Column6";
            this.Column6.ReadOnly = true;
            this.Column6.Width = 180;
            // 
            // discount_per_unit
            // 
            this.discount_per_unit.HeaderText = "Discount Per Unit";
            this.discount_per_unit.Name = "discount_per_unit";
            this.discount_per_unit.ReadOnly = true;
            this.discount_per_unit.Visible = false;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Location = new System.Drawing.Point(-1, 503);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(491, 109);
            this.panel2.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.label6.Font = new System.Drawing.Font("Arial Narrow", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(5, 78);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 20);
            this.label6.TabIndex = 1;
            this.label6.Text = "N/A";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 39.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(236, 71);
            this.label2.TabIndex = 0;
            this.label2.Text = "00:00:00";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // dataGridViewImageColumn1
            // 
            this.dataGridViewImageColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewImageColumn1.HeaderText = "";
            this.dataGridViewImageColumn1.Image = ((System.Drawing.Image)(resources.GetObject("dataGridViewImageColumn1.Image")));
            this.dataGridViewImageColumn1.Name = "dataGridViewImageColumn1";
            this.dataGridViewImageColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // dataGridViewImageColumn2
            // 
            this.dataGridViewImageColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewImageColumn2.HeaderText = "";
            this.dataGridViewImageColumn2.Image = ((System.Drawing.Image)(resources.GetObject("dataGridViewImageColumn2.Image")));
            this.dataGridViewImageColumn2.Name = "dataGridViewImageColumn2";
            this.dataGridViewImageColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // dataGridViewImageColumn3
            // 
            this.dataGridViewImageColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewImageColumn3.HeaderText = "";
            this.dataGridViewImageColumn3.Image = ((System.Drawing.Image)(resources.GetObject("dataGridViewImageColumn3.Image")));
            this.dataGridViewImageColumn3.Name = "dataGridViewImageColumn3";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox2.Image = global::PosSystem.Properties.Resources.Gemini_Generated_Image_8623t88623t88623;
            this.pictureBox2.Location = new System.Drawing.Point(760, 358);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(247, 252);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 5;
            this.pictureBox2.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(126, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 20);
            this.label4.TabIndex = 24;
            this.label4.Text = "Hello";
            // 
            // frmPOS
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1008, 611);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.panel6);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximumSize = new System.Drawing.Size(1920, 1080);
            this.MinimumSize = new System.Drawing.Size(1024, 650);
            this.Name = "frmPOS";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "POS";
            this.Load += new System.EventHandler(this.frmPOS_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmPOS_KeyDown);
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.Button btnTrans;
        public System.Windows.Forms.Button btnClose;
        public System.Windows.Forms.Button btnSales;
        public System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.Button btnSattle;
        public System.Windows.Forms.Button btnDiscount;
        public System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.Label lblTransno;
        public System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label lblDisplayTotal;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label10;
        public System.Windows.Forms.Label lblVat;
        public System.Windows.Forms.Label lblVatable;
        public System.Windows.Forms.Label lblDiscount;
        public System.Windows.Forms.Label lblTotal;
        public System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.PictureBox pictureBox2;
        public MetroFramework.Controls.MetroTextBox txtSearch;
        private System.Windows.Forms.Label label16;
        public MetroFramework.Controls.MetroTextBox txtQty;
        private System.Windows.Forms.DataGridViewImageColumn dataGridViewImageColumn1;
        private System.Windows.Forms.DataGridViewImageColumn dataGridViewImageColumn2;
        private System.Windows.Forms.DataGridViewImageColumn dataGridViewImageColumn3;
        public System.Windows.Forms.Label lblName;
        public System.Windows.Forms.Label LblUser;
        public System.Windows.Forms.Label lblPhone;
        public System.Windows.Forms.Label lblAddress;
        public System.Windows.Forms.Button btnscanbarcode;
        public System.Windows.Forms.Label lblSname;
        private System.Windows.Forms.TextBox textBoxbarcode;
        public System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox comboBoxprinter;
        public System.Windows.Forms.Button btnReprint;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column8;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewImageColumn ColRemove;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.DataGridViewImageColumn collAdd;
        private System.Windows.Forms.DataGridViewImageColumn Delete;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column7;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6;
        private System.Windows.Forms.DataGridViewTextBoxColumn discount_per_unit;
        public System.Windows.Forms.Label label4;
    }
}