namespace PosSystem
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.panelSlide = new System.Windows.Forms.Panel();
            this.btnMaintenanceMain = new System.Windows.Forms.Button();
            this.panelSubSettings = new System.Windows.Forms.Panel();
            this.btnStoreSettings = new System.Windows.Forms.Button();
            this.btnUsersettings = new System.Windows.Forms.Button();
            this.btnSettingsMain = new System.Windows.Forms.Button();
            this.btnPOSMain = new System.Windows.Forms.Button();
            this.btnbarcodebook = new System.Windows.Forms.Button();
            this.btnSalesHistoryMain = new System.Windows.Forms.Button();
            this.btnRecordsMain = new System.Windows.Forms.Button();
            this.panelSubProduct = new System.Windows.Forms.Panel();
            this.btnManageBrand = new System.Windows.Forms.Button();
            this.btnManageCategory = new System.Windows.Forms.Button();
            this.btnManageProduct = new System.Windows.Forms.Button();
            this.btnProductMain = new System.Windows.Forms.Button();
            this.panelSubStocks = new System.Windows.Forms.Panel();
            this.btnVendor = new System.Windows.Forms.Button();
            this.btnStockEntry = new System.Windows.Forms.Button();
            this.btnStockAdjestment = new System.Windows.Forms.Button();
            this.lblRole = new System.Windows.Forms.Label();
            this.lblUserName = new System.Windows.Forms.Label();
            this.btnStockMain = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnDashboardMain = new System.Windows.Forms.Button();
            this.panelUserProfile = new System.Windows.Forms.Panel();
            this.btnLogOutMain = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.lblTransno = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblDisplayTotal = new System.Windows.Forms.Label();
            this.lblPhone = new System.Windows.Forms.Label();
            this.lblSname = new System.Windows.Forms.Label();
            this.lblAddress = new System.Windows.Forms.Label();
            this.lblDiscount = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panelSlide.SuspendLayout();
            this.panelSubSettings.SuspendLayout();
            this.panelSubProduct.SuspendLayout();
            this.panelSubStocks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelSlide
            // 
            this.panelSlide.AutoScroll = true;
            this.panelSlide.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.panelSlide.Controls.Add(this.btnMaintenanceMain);
            this.panelSlide.Controls.Add(this.panelSubSettings);
            this.panelSlide.Controls.Add(this.btnSettingsMain);
            this.panelSlide.Controls.Add(this.btnPOSMain);
            this.panelSlide.Controls.Add(this.btnbarcodebook);
            this.panelSlide.Controls.Add(this.btnSalesHistoryMain);
            this.panelSlide.Controls.Add(this.btnRecordsMain);
            this.panelSlide.Controls.Add(this.panelSubProduct);
            this.panelSlide.Controls.Add(this.btnProductMain);
            this.panelSlide.Controls.Add(this.panelSubStocks);
            this.panelSlide.Controls.Add(this.lblRole);
            this.panelSlide.Controls.Add(this.lblUserName);
            this.panelSlide.Controls.Add(this.btnStockMain);
            this.panelSlide.Controls.Add(this.pictureBox1);
            this.panelSlide.Controls.Add(this.btnDashboardMain);
            this.panelSlide.Controls.Add(this.panelUserProfile);
            this.panelSlide.Controls.Add(this.btnLogOutMain);
            this.panelSlide.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelSlide.Location = new System.Drawing.Point(0, 0);
            this.panelSlide.Name = "panelSlide";
            this.panelSlide.Size = new System.Drawing.Size(260, 661);
            this.panelSlide.TabIndex = 1;
            this.panelSlide.Paint += new System.Windows.Forms.PaintEventHandler(this.panel2_Paint);
            // 
            // btnMaintenanceMain
            // 
            this.btnMaintenanceMain.AutoSize = true;
            this.btnMaintenanceMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnMaintenanceMain.FlatAppearance.BorderSize = 0;
            this.btnMaintenanceMain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMaintenanceMain.Font = new System.Drawing.Font("Bahnschrift", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMaintenanceMain.ForeColor = System.Drawing.Color.White;
            this.btnMaintenanceMain.Image = global::PosSystem.Properties.Resources.data_recovery;
            this.btnMaintenanceMain.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMaintenanceMain.Location = new System.Drawing.Point(0, 705);
            this.btnMaintenanceMain.Name = "btnMaintenanceMain";
            this.btnMaintenanceMain.Size = new System.Drawing.Size(243, 38);
            this.btnMaintenanceMain.TabIndex = 23;
            this.btnMaintenanceMain.Text = "Maintenance";
            this.btnMaintenanceMain.UseVisualStyleBackColor = true;
            this.btnMaintenanceMain.Click += new System.EventHandler(this.btnMaintenanceMain_Click);
            // 
            // panelSubSettings
            // 
            this.panelSubSettings.Controls.Add(this.btnStoreSettings);
            this.panelSubSettings.Controls.Add(this.btnUsersettings);
            this.panelSubSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSubSettings.Location = new System.Drawing.Point(0, 635);
            this.panelSubSettings.Name = "panelSubSettings";
            this.panelSubSettings.Size = new System.Drawing.Size(243, 70);
            this.panelSubSettings.TabIndex = 6;
            this.panelSubSettings.Paint += new System.Windows.Forms.PaintEventHandler(this.panelSubSettings_Paint);
            // 
            // btnStoreSettings
            // 
            this.btnStoreSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(50)))));
            this.btnStoreSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnStoreSettings.FlatAppearance.BorderSize = 0;
            this.btnStoreSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStoreSettings.Font = new System.Drawing.Font("Bahnschrift", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStoreSettings.ForeColor = System.Drawing.Color.White;
            this.btnStoreSettings.Image = ((System.Drawing.Image)(resources.GetObject("btnStoreSettings.Image")));
            this.btnStoreSettings.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStoreSettings.Location = new System.Drawing.Point(0, 35);
            this.btnStoreSettings.Name = "btnStoreSettings";
            this.btnStoreSettings.Size = new System.Drawing.Size(243, 35);
            this.btnStoreSettings.TabIndex = 9;
            this.btnStoreSettings.Text = "Store Settings";
            this.btnStoreSettings.UseVisualStyleBackColor = false;
            this.btnStoreSettings.Click += new System.EventHandler(this.button7_Click);
            // 
            // btnUsersettings
            // 
            this.btnUsersettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(50)))));
            this.btnUsersettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnUsersettings.FlatAppearance.BorderSize = 0;
            this.btnUsersettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUsersettings.Font = new System.Drawing.Font("Bahnschrift", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUsersettings.ForeColor = System.Drawing.Color.White;
            this.btnUsersettings.Image = ((System.Drawing.Image)(resources.GetObject("btnUsersettings.Image")));
            this.btnUsersettings.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnUsersettings.Location = new System.Drawing.Point(0, 0);
            this.btnUsersettings.Name = "btnUsersettings";
            this.btnUsersettings.Size = new System.Drawing.Size(243, 35);
            this.btnUsersettings.TabIndex = 10;
            this.btnUsersettings.Text = "User Settings";
            this.btnUsersettings.UseVisualStyleBackColor = false;
            this.btnUsersettings.Click += new System.EventHandler(this.button8_Click);
            // 
            // btnSettingsMain
            // 
            this.btnSettingsMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSettingsMain.FlatAppearance.BorderSize = 0;
            this.btnSettingsMain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettingsMain.Font = new System.Drawing.Font("Bahnschrift", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSettingsMain.ForeColor = System.Drawing.Color.White;
            this.btnSettingsMain.Image = global::PosSystem.Properties.Resources.settings;
            this.btnSettingsMain.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSettingsMain.Location = new System.Drawing.Point(0, 600);
            this.btnSettingsMain.Name = "btnSettingsMain";
            this.btnSettingsMain.Size = new System.Drawing.Size(243, 35);
            this.btnSettingsMain.TabIndex = 22;
            this.btnSettingsMain.Text = "Settings";
            this.btnSettingsMain.UseVisualStyleBackColor = true;
            this.btnSettingsMain.Click += new System.EventHandler(this.btnSettingsMain_Click);
            // 
            // btnPOSMain
            // 
            this.btnPOSMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnPOSMain.FlatAppearance.BorderSize = 0;
            this.btnPOSMain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPOSMain.Font = new System.Drawing.Font("Bahnschrift", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPOSMain.ForeColor = System.Drawing.Color.White;
            this.btnPOSMain.Image = global::PosSystem.Properties.Resources.pos;
            this.btnPOSMain.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPOSMain.Location = new System.Drawing.Point(0, 565);
            this.btnPOSMain.Name = "btnPOSMain";
            this.btnPOSMain.Size = new System.Drawing.Size(243, 35);
            this.btnPOSMain.TabIndex = 21;
            this.btnPOSMain.Text = "POS";
            this.btnPOSMain.UseVisualStyleBackColor = true;
            this.btnPOSMain.Click += new System.EventHandler(this.btnPOSMain_Click);
            // 
            // btnbarcodebook
            // 
            this.btnbarcodebook.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnbarcodebook.FlatAppearance.BorderSize = 0;
            this.btnbarcodebook.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnbarcodebook.Font = new System.Drawing.Font("Bahnschrift", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnbarcodebook.ForeColor = System.Drawing.Color.White;
            this.btnbarcodebook.Image = global::PosSystem.Properties.Resources.audit;
            this.btnbarcodebook.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnbarcodebook.Location = new System.Drawing.Point(0, 530);
            this.btnbarcodebook.Name = "btnbarcodebook";
            this.btnbarcodebook.Size = new System.Drawing.Size(243, 35);
            this.btnbarcodebook.TabIndex = 20;
            this.btnbarcodebook.Text = "Barcode Book";
            this.btnbarcodebook.UseVisualStyleBackColor = true;
            this.btnbarcodebook.Click += new System.EventHandler(this.btnAuditMain_Click);
            // 
            // btnSalesHistoryMain
            // 
            this.btnSalesHistoryMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSalesHistoryMain.FlatAppearance.BorderSize = 0;
            this.btnSalesHistoryMain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalesHistoryMain.Font = new System.Drawing.Font("Bahnschrift", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSalesHistoryMain.ForeColor = System.Drawing.Color.White;
            this.btnSalesHistoryMain.Image = global::PosSystem.Properties.Resources.history;
            this.btnSalesHistoryMain.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSalesHistoryMain.Location = new System.Drawing.Point(0, 495);
            this.btnSalesHistoryMain.Name = "btnSalesHistoryMain";
            this.btnSalesHistoryMain.Size = new System.Drawing.Size(243, 35);
            this.btnSalesHistoryMain.TabIndex = 8;
            this.btnSalesHistoryMain.Text = "Sales History";
            this.btnSalesHistoryMain.UseVisualStyleBackColor = true;
            this.btnSalesHistoryMain.Click += new System.EventHandler(this.btnSalesHistory_Click);
            // 
            // btnRecordsMain
            // 
            this.btnRecordsMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnRecordsMain.FlatAppearance.BorderSize = 0;
            this.btnRecordsMain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRecordsMain.Font = new System.Drawing.Font("Bahnschrift", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRecordsMain.ForeColor = System.Drawing.Color.White;
            this.btnRecordsMain.Image = global::PosSystem.Properties.Resources.database;
            this.btnRecordsMain.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRecordsMain.Location = new System.Drawing.Point(0, 460);
            this.btnRecordsMain.Name = "btnRecordsMain";
            this.btnRecordsMain.Size = new System.Drawing.Size(243, 35);
            this.btnRecordsMain.TabIndex = 8;
            this.btnRecordsMain.Text = "Records";
            this.btnRecordsMain.UseVisualStyleBackColor = true;
            this.btnRecordsMain.Click += new System.EventHandler(this.button6_Click);
            // 
            // panelSubProduct
            // 
            this.panelSubProduct.Controls.Add(this.btnManageBrand);
            this.panelSubProduct.Controls.Add(this.btnManageCategory);
            this.panelSubProduct.Controls.Add(this.btnManageProduct);
            this.panelSubProduct.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSubProduct.Location = new System.Drawing.Point(0, 355);
            this.panelSubProduct.Name = "panelSubProduct";
            this.panelSubProduct.Size = new System.Drawing.Size(243, 105);
            this.panelSubProduct.TabIndex = 0;
            this.panelSubProduct.Paint += new System.Windows.Forms.PaintEventHandler(this.panelSubProduct_Paint);
            // 
            // btnManageBrand
            // 
            this.btnManageBrand.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(50)))));
            this.btnManageBrand.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnManageBrand.FlatAppearance.BorderSize = 0;
            this.btnManageBrand.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManageBrand.Font = new System.Drawing.Font("Bahnschrift", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnManageBrand.ForeColor = System.Drawing.Color.White;
            this.btnManageBrand.Image = ((System.Drawing.Image)(resources.GetObject("btnManageBrand.Image")));
            this.btnManageBrand.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnManageBrand.Location = new System.Drawing.Point(0, 70);
            this.btnManageBrand.Name = "btnManageBrand";
            this.btnManageBrand.Size = new System.Drawing.Size(243, 35);
            this.btnManageBrand.TabIndex = 7;
            this.btnManageBrand.Text = "Manage Brand";
            this.btnManageBrand.UseVisualStyleBackColor = false;
            this.btnManageBrand.Click += new System.EventHandler(this.btnBrand_Click);
            // 
            // btnManageCategory
            // 
            this.btnManageCategory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(50)))));
            this.btnManageCategory.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnManageCategory.FlatAppearance.BorderSize = 0;
            this.btnManageCategory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManageCategory.Font = new System.Drawing.Font("Bahnschrift", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnManageCategory.ForeColor = System.Drawing.Color.White;
            this.btnManageCategory.Image = ((System.Drawing.Image)(resources.GetObject("btnManageCategory.Image")));
            this.btnManageCategory.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnManageCategory.Location = new System.Drawing.Point(0, 35);
            this.btnManageCategory.Name = "btnManageCategory";
            this.btnManageCategory.Size = new System.Drawing.Size(243, 35);
            this.btnManageCategory.TabIndex = 6;
            this.btnManageCategory.Text = "Manage Category";
            this.btnManageCategory.UseVisualStyleBackColor = false;
            this.btnManageCategory.Click += new System.EventHandler(this.button4_Click);
            // 
            // btnManageProduct
            // 
            this.btnManageProduct.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(50)))));
            this.btnManageProduct.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnManageProduct.FlatAppearance.BorderSize = 0;
            this.btnManageProduct.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManageProduct.Font = new System.Drawing.Font("Bahnschrift", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnManageProduct.ForeColor = System.Drawing.Color.White;
            this.btnManageProduct.Image = ((System.Drawing.Image)(resources.GetObject("btnManageProduct.Image")));
            this.btnManageProduct.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnManageProduct.Location = new System.Drawing.Point(0, 0);
            this.btnManageProduct.Name = "btnManageProduct";
            this.btnManageProduct.Size = new System.Drawing.Size(243, 35);
            this.btnManageProduct.TabIndex = 5;
            this.btnManageProduct.Text = "Manage Products";
            this.btnManageProduct.UseVisualStyleBackColor = false;
            this.btnManageProduct.Click += new System.EventHandler(this.btnProduct_Click);
            // 
            // btnProductMain
            // 
            this.btnProductMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnProductMain.FlatAppearance.BorderSize = 0;
            this.btnProductMain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProductMain.Font = new System.Drawing.Font("Bahnschrift", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnProductMain.ForeColor = System.Drawing.Color.White;
            this.btnProductMain.Image = global::PosSystem.Properties.Resources.productdropdownbtn3;
            this.btnProductMain.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnProductMain.Location = new System.Drawing.Point(0, 320);
            this.btnProductMain.Name = "btnProductMain";
            this.btnProductMain.Size = new System.Drawing.Size(243, 35);
            this.btnProductMain.TabIndex = 17;
            this.btnProductMain.Text = "Products";
            this.btnProductMain.UseVisualStyleBackColor = true;
            this.btnProductMain.Click += new System.EventHandler(this.btnProductMain_Click);
            // 
            // panelSubStocks
            // 
            this.panelSubStocks.Controls.Add(this.btnVendor);
            this.panelSubStocks.Controls.Add(this.btnStockEntry);
            this.panelSubStocks.Controls.Add(this.btnStockAdjestment);
            this.panelSubStocks.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSubStocks.Location = new System.Drawing.Point(0, 215);
            this.panelSubStocks.Name = "panelSubStocks";
            this.panelSubStocks.Size = new System.Drawing.Size(243, 105);
            this.panelSubStocks.TabIndex = 19;
            this.panelSubStocks.Paint += new System.Windows.Forms.PaintEventHandler(this.panelSubStocks_Paint);
            // 
            // btnVendor
            // 
            this.btnVendor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(50)))));
            this.btnVendor.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnVendor.FlatAppearance.BorderSize = 0;
            this.btnVendor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVendor.Font = new System.Drawing.Font("Bahnschrift", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVendor.ForeColor = System.Drawing.Color.White;
            this.btnVendor.Image = ((System.Drawing.Image)(resources.GetObject("btnVendor.Image")));
            this.btnVendor.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnVendor.Location = new System.Drawing.Point(0, 70);
            this.btnVendor.Name = "btnVendor";
            this.btnVendor.Size = new System.Drawing.Size(243, 35);
            this.btnVendor.TabIndex = 12;
            this.btnVendor.Text = "Suppliers";
            this.btnVendor.UseVisualStyleBackColor = false;
            this.btnVendor.Click += new System.EventHandler(this.btnVendor_Click);
            // 
            // btnStockEntry
            // 
            this.btnStockEntry.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(50)))));
            this.btnStockEntry.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnStockEntry.FlatAppearance.BorderSize = 0;
            this.btnStockEntry.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStockEntry.Font = new System.Drawing.Font("Bahnschrift", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStockEntry.ForeColor = System.Drawing.Color.White;
            this.btnStockEntry.Image = ((System.Drawing.Image)(resources.GetObject("btnStockEntry.Image")));
            this.btnStockEntry.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStockEntry.Location = new System.Drawing.Point(0, 35);
            this.btnStockEntry.Name = "btnStockEntry";
            this.btnStockEntry.Size = new System.Drawing.Size(243, 35);
            this.btnStockEntry.TabIndex = 12;
            this.btnStockEntry.Text = "Stock Entry";
            this.btnStockEntry.UseVisualStyleBackColor = false;
            this.btnStockEntry.Click += new System.EventHandler(this.btnStock_Click);
            // 
            // btnStockAdjestment
            // 
            this.btnStockAdjestment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(50)))));
            this.btnStockAdjestment.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnStockAdjestment.FlatAppearance.BorderSize = 0;
            this.btnStockAdjestment.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStockAdjestment.Font = new System.Drawing.Font("Bahnschrift", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStockAdjestment.ForeColor = System.Drawing.Color.White;
            this.btnStockAdjestment.Image = ((System.Drawing.Image)(resources.GetObject("btnStockAdjestment.Image")));
            this.btnStockAdjestment.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStockAdjestment.Location = new System.Drawing.Point(0, 0);
            this.btnStockAdjestment.Name = "btnStockAdjestment";
            this.btnStockAdjestment.Size = new System.Drawing.Size(243, 35);
            this.btnStockAdjestment.TabIndex = 13;
            this.btnStockAdjestment.Text = "Stock Adjustment";
            this.btnStockAdjestment.UseVisualStyleBackColor = false;
            this.btnStockAdjestment.Click += new System.EventHandler(this.button2_Click);
            // 
            // lblRole
            // 
            this.lblRole.BackColor = System.Drawing.Color.Transparent;
            this.lblRole.Font = new System.Drawing.Font("Franklin Gothic Medium", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRole.ForeColor = System.Drawing.Color.White;
            this.lblRole.Location = new System.Drawing.Point(3, 104);
            this.lblRole.Name = "lblRole";
            this.lblRole.Size = new System.Drawing.Size(237, 23);
            this.lblRole.TabIndex = 2;
            this.lblRole.Text = "Administrator";
            this.lblRole.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRole.Click += new System.EventHandler(this.lblRole_Click);
            // 
            // lblUserName
            // 
            this.lblUserName.Font = new System.Drawing.Font("Georgia", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUserName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(158)))), ((int)(((byte)(132)))));
            this.lblUserName.Location = new System.Drawing.Point(46, 82);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(150, 29);
            this.lblUserName.TabIndex = 1;
            this.lblUserName.Text = "User Name";
            this.lblUserName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblUserName.Click += new System.EventHandler(this.lblName_Click);
            // 
            // btnStockMain
            // 
            this.btnStockMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnStockMain.FlatAppearance.BorderSize = 0;
            this.btnStockMain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStockMain.Font = new System.Drawing.Font("Bahnschrift", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStockMain.ForeColor = System.Drawing.Color.White;
            this.btnStockMain.Image = global::PosSystem.Properties.Resources.btnStockMain;
            this.btnStockMain.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStockMain.Location = new System.Drawing.Point(0, 180);
            this.btnStockMain.Name = "btnStockMain";
            this.btnStockMain.Size = new System.Drawing.Size(243, 35);
            this.btnStockMain.TabIndex = 18;
            this.btnStockMain.Text = "Stocks";
            this.btnStockMain.UseVisualStyleBackColor = true;
            this.btnStockMain.Click += new System.EventHandler(this.btnStockMain_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(82, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(73, 72);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 15;
            this.pictureBox1.TabStop = false;
            // 
            // btnDashboardMain
            // 
            this.btnDashboardMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnDashboardMain.FlatAppearance.BorderSize = 0;
            this.btnDashboardMain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDashboardMain.Font = new System.Drawing.Font("Bahnschrift", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDashboardMain.ForeColor = System.Drawing.Color.White;
            this.btnDashboardMain.Image = global::PosSystem.Properties.Resources.dashboard;
            this.btnDashboardMain.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDashboardMain.Location = new System.Drawing.Point(0, 145);
            this.btnDashboardMain.Name = "btnDashboardMain";
            this.btnDashboardMain.Size = new System.Drawing.Size(243, 35);
            this.btnDashboardMain.TabIndex = 3;
            this.btnDashboardMain.Text = "Dashboard";
            this.btnDashboardMain.UseVisualStyleBackColor = true;
            this.btnDashboardMain.Click += new System.EventHandler(this.button1_Click);
            // 
            // panelUserProfile
            // 
            this.panelUserProfile.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelUserProfile.Location = new System.Drawing.Point(0, 0);
            this.panelUserProfile.Name = "panelUserProfile";
            this.panelUserProfile.Size = new System.Drawing.Size(243, 145);
            this.panelUserProfile.TabIndex = 16;
            // 
            // btnLogOutMain
            // 
            this.btnLogOutMain.AutoSize = true;
            this.btnLogOutMain.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnLogOutMain.FlatAppearance.BorderSize = 0;
            this.btnLogOutMain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogOutMain.Font = new System.Drawing.Font("Bahnschrift", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLogOutMain.ForeColor = System.Drawing.Color.White;
            this.btnLogOutMain.Image = global::PosSystem.Properties.Resources.logout;
            this.btnLogOutMain.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLogOutMain.Location = new System.Drawing.Point(0, 743);
            this.btnLogOutMain.Name = "btnLogOutMain";
            this.btnLogOutMain.Size = new System.Drawing.Size(243, 38);
            this.btnLogOutMain.TabIndex = 11;
            this.btnLogOutMain.Text = "Log Out";
            this.btnLogOutMain.UseVisualStyleBackColor = true;
            this.btnLogOutMain.Click += new System.EventHandler(this.button9_Click);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.White;
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.ForeColor = System.Drawing.Color.Black;
            this.panel3.Location = new System.Drawing.Point(260, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1066, 661);
            this.panel3.TabIndex = 2;
            this.panel3.Paint += new System.Windows.Forms.PaintEventHandler(this.panel3_Paint);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(10, 10);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.Visible = false;
            // 
            // lblTransno
            // 
            this.lblTransno.AutoSize = true;
            this.lblTransno.Location = new System.Drawing.Point(0, 0);
            this.lblTransno.Name = "lblTransno";
            this.lblTransno.Size = new System.Drawing.Size(0, 17);
            this.lblTransno.TabIndex = 3;
            this.lblTransno.Visible = false;
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(0, 0);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(0, 17);
            this.lblTotal.TabIndex = 4;
            this.lblTotal.Visible = false;
            // 
            // lblDisplayTotal
            // 
            this.lblDisplayTotal.AutoSize = true;
            this.lblDisplayTotal.Location = new System.Drawing.Point(0, 0);
            this.lblDisplayTotal.Name = "lblDisplayTotal";
            this.lblDisplayTotal.Size = new System.Drawing.Size(0, 17);
            this.lblDisplayTotal.TabIndex = 5;
            this.lblDisplayTotal.Visible = false;
            // 
            // lblPhone
            // 
            this.lblPhone.Location = new System.Drawing.Point(0, 0);
            this.lblPhone.Name = "lblPhone";
            this.lblPhone.Size = new System.Drawing.Size(100, 23);
            this.lblPhone.TabIndex = 0;
            this.lblPhone.Visible = false;
            // 
            // lblSname
            // 
            this.lblSname.Location = new System.Drawing.Point(0, 0);
            this.lblSname.Name = "lblSname";
            this.lblSname.Size = new System.Drawing.Size(100, 23);
            this.lblSname.TabIndex = 0;
            this.lblSname.Visible = false;
            // 
            // lblAddress
            // 
            this.lblAddress.Location = new System.Drawing.Point(0, 0);
            this.lblAddress.Name = "lblAddress";
            this.lblAddress.Size = new System.Drawing.Size(100, 23);
            this.lblAddress.TabIndex = 0;
            this.lblAddress.Visible = false;
            // 
            // lblDiscount
            // 
            this.lblDiscount.Location = new System.Drawing.Point(0, 0);
            this.lblDiscount.Name = "lblDiscount";
            this.lblDiscount.Size = new System.Drawing.Size(100, 23);
            this.lblDiscount.TabIndex = 0;
            this.lblDiscount.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(158)))), ((int)(((byte)(132)))));
            this.ClientSize = new System.Drawing.Size(1326, 661);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panelSlide);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.lblTransno);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.lblDisplayTotal);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MinimumSize = new System.Drawing.Size(1250, 700);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ADMINISTRATOR POS";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panelSlide.ResumeLayout(false);
            this.panelSlide.PerformLayout();
            this.panelSubSettings.ResumeLayout(false);
            this.panelSubProduct.ResumeLayout(false);
            this.panelSubStocks.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panelSlide;
        private System.Windows.Forms.Button btnLogOutMain;
        private System.Windows.Forms.Button btnStoreSettings;
        private System.Windows.Forms.Button btnRecordsMain;
        private System.Windows.Forms.Panel panel3;
        public System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.Button btnUsersettings;
        private System.Windows.Forms.Button btnSalesHistoryMain;
        public System.Windows.Forms.Label lblRole;
        private System.Windows.Forms.PictureBox pictureBox1;
        //private Guna.UI2.WinForms.Guna2BorderlessForm guna2BorderlessForm1;
        //private Guna.UI2.WinForms.Guna2BorderlessForm guna2BorderlessForm2;

        // Missing Objects Defined
        public System.Windows.Forms.DataGridView dataGridView1;
        public System.Windows.Forms.Label lblTransno;
        public System.Windows.Forms.Label lblTotal;
        public System.Windows.Forms.Label lblDisplayTotal;
        public System.Windows.Forms.Label lblPhone;
        public System.Windows.Forms.Label lblSname;
        public System.Windows.Forms.Label lblAddress;
        public System.Windows.Forms.Label lblDiscount;
        private System.Windows.Forms.Panel panelUserProfile;
        private System.Windows.Forms.Panel panelSubStocks;
        private System.Windows.Forms.Button btnStockEntry;
        private System.Windows.Forms.Button btnStockAdjestment;
        private System.Windows.Forms.Button btnStockMain;
        private System.Windows.Forms.Button btnVendor;
        private System.Windows.Forms.Button btnManageProduct;
        private System.Windows.Forms.Button btnManageCategory;
        private System.Windows.Forms.Button btnManageBrand;
        private System.Windows.Forms.Button btnProductMain;
        private System.Windows.Forms.Button btnDashboardMain;
        private System.Windows.Forms.Panel panelSubProduct;
        private System.Windows.Forms.Button btnbarcodebook;
        private System.Windows.Forms.Button btnPOSMain;
        private System.Windows.Forms.Button btnSettingsMain;
        private System.Windows.Forms.Panel panelSubSettings;
        private System.Windows.Forms.Button btnMaintenanceMain;
        private System.Windows.Forms.Timer timer1;
    }
}