using System;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosSystem
{
    public partial class Form1 : Form
    {
        public string _pass;
        public string _user;
        public string _role;
        public string _name;

        private frmRecords _frmRecords;
        private frmDashbord _frmDashboard;

        public Form1()
        {
            InitializeComponent();
            panelSlide.MinimumSize = new Size(260, 0);
            panelSlide.Width = 260;

            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.MinimumSize = new Size(1100, 700);

            CustomizeDesign();
            NotifyCriticalItems();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await MyDashbordAsync();
        }

        private void CloseAllChildContent()
        {
            try
            {
                if (panel3.Controls.Count > 0)
                {
                    for (int i = panel3.Controls.Count - 1; i >= 0; i--)
                    {
                        Control ctrl = panel3.Controls[i];
                        panel3.Controls.RemoveAt(i);
                        ctrl.Dispose();
                    }
                }

                panel3.Controls.Clear();

                if (_frmDashboard != null)
                {
                    _frmDashboard.Dispose();
                    _frmDashboard = null;
                }

                if (_frmRecords != null)
                {
                    _frmRecords.Dispose();
                    _frmRecords = null;
                }
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Form1_CloseAllChildContent");
            }
        }

        #region Sidebar UI Toggle Logic
        // Sets initial state of the dropdown panels to hidden
        private void CustomizeDesign()
        {
            panelSubProduct.Visible = false;
            panelSubStocks.Visible = false;
            panelSubSettings.Visible = false;
            // Add other sub-panels here as you design them
        }

        // Handles the "Shrink and Drop" logic
        private void HideSubMenu()
        {
            if (panelSubProduct.Visible) panelSubProduct.Visible = false;
            if (panelSubStocks.Visible) panelSubStocks.Visible = false;
            if (panelSubSettings.Visible) panelSubSettings.Visible = false;
        }

        private void ShowSubMenu(Panel subMenu)
        {
            if (subMenu.Visible == false)
            {
                HideSubMenu(); // Shrink currently open dropdowns
                subMenu.Visible = true; // Drop the new one
            }
            else
            {
                subMenu.Visible = false; // Toggle off if clicked again
            }
        }
        #endregion

        #region User Session
        public void SetUserSession(string user, string role, string name)
        {
            _user = user;
            _role = role;
            _name = name;

            lblUserName.Text = "Hello " + _name + " !";
            lblRole.Text = _role;

            // FIXED: Support both "Admin" and "Administrator" variations (Case-Insensitive)
            bool isAdmin = _role.Equals("Administrator", StringComparison.OrdinalIgnoreCase) ||
                           _role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            btnManageBrand.Enabled = isAdmin;
            btnManageCategory.Enabled = isAdmin;
            btnManageProduct.Enabled = isAdmin;
            btnVendor.Enabled = isAdmin;
            btnUsersettings.Enabled = isAdmin;
            btnStoreSettings.Enabled = isAdmin;
        }
        #endregion

        #region Child Form Management
        private void OpenChildForm(Form childForm)
        {
            // 1. Check if there is already a form in the panel
            if (panel3.Controls.Count > 0)
            {
                Control oldControl = panel3.Controls[0];
                panel3.Controls.Remove(oldControl);
                oldControl.Dispose(); // Properly kill the old form
            }

            // 2. Setup the new form
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            // 3. Add to panel
            panel3.Controls.Add(childForm);
            panel3.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        private void BringOpenFormToFront(Form frm)
        {
            if (frm == null || frm.IsDisposed) return;

            if (frm.WindowState == FormWindowState.Minimized)
                frm.WindowState = FormWindowState.Normal;

            frm.Show();
            frm.BringToFront();
            frm.TopMost = true;
            frm.TopMost = false;
            frm.Activate();
            frm.Focus();
        }
        #endregion

        #region Dashboard
        public async Task MyDashbordAsync()
        {
            try
            {
                if (_frmDashboard != null)
                    _frmDashboard.Dispose();

                _frmDashboard = new frmDashbord
                {
                    TopLevel = false,
                    Dock = DockStyle.Fill
                };

                panel3.Controls.Clear();
                panel3.Controls.Add(_frmDashboard);

                var dailySalesTask = Task.Run(() => DBConnection.DailySales());
                var productLineTask = Task.Run(() => DBConnection.ProductLine());
                var stockTask = Task.Run(() => DBConnection.StockOnHand());
                var criticalTask = Task.Run(() => DBConnection.CriticalItems());

                await Task.WhenAll(dailySalesTask, productLineTask, stockTask, criticalTask);

                _frmDashboard.lblDailySales.Text = dailySalesTask.Result.ToString("#,##0.00");
                _frmDashboard.lblProduct.Text = productLineTask.Result.ToString("#,##0");
                _frmDashboard.lblStock.Text = stockTask.Result.ToString("#,##0");
                _frmDashboard.lblCriticalItems.Text = criticalTask.Result.ToString("#,##0");

                _frmDashboard.BringToFront();
                _frmDashboard.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading dashboard: " + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Critical Item Notification
        public void NotifyCriticalItems()
        {
            int totalCount = 0;
            var previewItems = new System.Collections.Generic.List<string>();

            try
            {
                using (SQLiteConnection cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    cn.Open();

                    string query = @"
                SELECT pdesc, qty, reorder
                FROM TblProduct1
                WHERE isactive = 1 AND qty <= reorder
                ORDER BY qty ASC, pdesc ASC;";

                    using (SQLiteCommand cm = new SQLiteCommand(query, cn))
                    using (SQLiteDataReader dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            totalCount++;

                            if (previewItems.Count < 5)
                            {
                                string name = dr["pdesc"]?.ToString() ?? "Unknown Item";
                                int qty = dr["qty"] == DBNull.Value ? 0 : Convert.ToInt32(dr["qty"]);
                                int reorder = dr["reorder"] == DBNull.Value ? 0 : Convert.ToInt32(dr["reorder"]);

                                previewItems.Add($"• {name}  ({qty}/{reorder})");
                            }
                        }
                    }
                }

                if (totalCount <= 0) return;

                Task.Delay(700).ContinueWith(_ =>
                {
                    try
                    {
                        if (IsDisposed) return;

                        Action showToast = () =>
                        {
                            if (IsDisposed) return;

                            string itemLines = string.Join(Environment.NewLine, previewItems);
                            if (totalCount > previewItems.Count)
                                itemLines += Environment.NewLine + $"• +{totalCount - previewItems.Count} more item(s)...";

                            Form toast = new Form
                            {
                                FormBorderStyle = FormBorderStyle.None,
                                StartPosition = FormStartPosition.Manual,
                                ShowInTaskbar = false,
                                TopMost = true,
                                BackColor = Color.White,
                                Width = 420,
                                Height = 210,
                                MaximizeBox = false,
                                MinimizeBox = false
                            };

                            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
                            toast.Location = new Point(wa.Right - toast.Width - 20, wa.Bottom - toast.Height - 20);

                            Panel leftAccent = new Panel
                            {
                                Dock = DockStyle.Left,
                                Width = 6,
                                BackColor = Color.FromArgb(220, 53, 69)
                            };

                            Panel topBar = new Panel
                            {
                                Dock = DockStyle.Top,
                                Height = 12,
                                BackColor = Color.FromArgb(255, 243, 205)
                            };

                            Label lblTitle = new Label
                            {
                                AutoSize = false,
                                Text = "Critical Stock Alert",
                                Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
                                ForeColor = Color.FromArgb(33, 37, 41),
                                Location = new Point(20, 22),
                                Size = new Size(250, 28)
                            };

                            Label lblCount = new Label
                            {
                                AutoSize = false,
                                Text = totalCount + " ITEM(S)",
                                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                                ForeColor = Color.White,
                                BackColor = Color.FromArgb(220, 53, 69),
                                TextAlign = ContentAlignment.MiddleCenter,
                                Location = new Point(285, 24),
                                Size = new Size(100, 24)
                            };

                            Label lblSub = new Label
                            {
                                AutoSize = false,
                                Text = "The following products are at or below reorder level.",
                                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                                ForeColor = Color.FromArgb(108, 117, 125),
                                Location = new Point(20, 55),
                                Size = new Size(370, 20)
                            };

                            Panel line = new Panel
                            {
                                BackColor = Color.FromArgb(230, 230, 230),
                                Location = new Point(20, 82),
                                Size = new Size(370, 1)
                            };

                            Label lblItems = new Label
                            {
                                AutoSize = false,
                                Text = itemLines,
                                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                                ForeColor = Color.FromArgb(52, 58, 64),
                                Location = new Point(20, 92),
                                Size = new Size(370, 78)
                            };

                            Label lblFooter = new Label
                            {
                                AutoSize = false,
                                Text = "Review full list in Records > Critical Stocks",
                                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                                ForeColor = Color.FromArgb(20, 158, 132),
                                Location = new Point(20, 175),
                                Size = new Size(250, 18)
                            };

                            Button btnClose = new Button
                            {
                                Text = "×",
                                FlatStyle = FlatStyle.Flat,
                                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                                ForeColor = Color.FromArgb(120, 120, 120),
                                BackColor = Color.White,
                                Size = new Size(30, 30),
                                Location = new Point(382, 12),
                                TabStop = false
                            };
                            btnClose.FlatAppearance.BorderSize = 0;
                            btnClose.Click += (s, e) =>
                            {
                                try { toast.Close(); } catch { }
                            };

                            System.Windows.Forms.Timer closeTimer = new System.Windows.Forms.Timer
                            {
                                Interval = 6500
                            };

                            closeTimer.Tick += (s, e) =>
                            {
                                closeTimer.Stop();
                                closeTimer.Dispose();
                                try
                                {
                                    if (!toast.IsDisposed) toast.Close();
                                }
                                catch { }
                            };

                            toast.FormClosed += (s, e) =>
                            {
                                try { closeTimer.Dispose(); } catch { }
                                try { toast.Dispose(); } catch { }
                            };

                            toast.Controls.Add(leftAccent);
                            toast.Controls.Add(topBar);
                            toast.Controls.Add(lblTitle);
                            toast.Controls.Add(lblCount);
                            toast.Controls.Add(lblSub);
                            toast.Controls.Add(line);
                            toast.Controls.Add(lblItems);
                            toast.Controls.Add(lblFooter);
                            toast.Controls.Add(btnClose);

                            closeTimer.Start();
                            toast.Show();
                            toast.BringToFront();
                        };

                        if (IsHandleCreated)
                            BeginInvoke(showToast);
                        else
                            showToast();
                    }
                    catch (Exception ex)
                    {
                        Program.SilentLog(ex, "Form1_NotifyCriticalItems_ShowToast");
                    }
                });
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Form1_NotifyCriticalItems");
            }
        }
        #endregion

        #region Button Handlers (Old Buttons - Fixed)
        private void btnBrand_Click(object sender, EventArgs e) => OpenChildForm(new frmBrandList());

        private void button4_Click(object sender, EventArgs e)
        {
            var frm = new frmCategoryList();
            frm.LoadCategory();
            OpenChildForm(frm);
        }

        private void btnProduct_Click(object sender, EventArgs e)
        {
            var frm = new frmProduct_List();
            frm.LoadRecords();
            OpenChildForm(frm);
        }

        private void btnStock_Click(object sender, EventArgs e)
        {
            var existing = Application.OpenForms.OfType<frmStockin>().FirstOrDefault();
            if (existing != null)
            {
                BringOpenFormToFront(existing);
                return;
            }

            var frm = new frmStockin(DBConnection.MyConnection());
            frm.StartPosition = FormStartPosition.CenterScreen;
            frm.Owner = this;
            frm.Show();
            BringOpenFormToFront(frm);
        }

        private void btnVendor_Click(object sender, EventArgs e)
        {
            var f = new frm_VendorList();
            f.LoadRecords();
            OpenChildForm(f);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            var frm = new frmUserAccount(this);
            frm.txtU.Text = _user;
            OpenChildForm(frm);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // Create a fresh instance to prevent the "Disposed Object" error
            _frmRecords = new frmRecords();
            OpenChildForm(_frmRecords);
        }

        private void btnSalesHistory_Click(object sender, EventArgs e)
        {
            var existing = Application.OpenForms.OfType<frmSoldItems>().FirstOrDefault();
            if (existing != null)
            {
                BringOpenFormToFront(existing);
                return;
            }

            var frm = new frmSoldItems { suser = _user };
            frm.StartPosition = FormStartPosition.CenterScreen;
            frm.Owner = this;
            frm.Show();
            BringOpenFormToFront(frm);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var frm = new frmStoreSetting();
            frm.LoadRecord();
            frm.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HideSubMenu(); // Shrink panels when returning to Dashboard
            _ = MyDashbordAsync();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            var existing = Application.OpenForms.OfType<frmAdjustment>().FirstOrDefault();
            if (existing != null)
            {
                BringOpenFormToFront(existing);
                return;
            }

            try
            {
                var frm = new frmAdjustment(this);
                await frm.LoadRecordsAsync();
                frm.txtUser.Text = _user;
                frm.StartPosition = FormStartPosition.CenterScreen;
                frm.Show();
                BringOpenFormToFront(frm);
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Form1_button2_Click");
                MessageBox.Show("Unable to open Stock Adjustment.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Logout Application?", "Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Hide();
                new frmUserLogin().Show();
            }
        }
        #endregion

        #region New Toggle Handlers
        private void btnStockMain_Click(object sender, EventArgs e)
        {
            ShowSubMenu(panelSubStocks);
        }

        private void btnProductMain_Click(object sender, EventArgs e)
        {
            ShowSubMenu(panelSubProduct);
        }

        private void btnSettingsMain_Click(object sender, EventArgs e)
        {
            ShowSubMenu(panelSubSettings);
        }

        private void btnPOSMain_Click(object sender, EventArgs e)
        {
            HideSubMenu();

            frmPOS frm = null;

            try
            {
                this.Hide();

                frm = new frmPOS(this);
                frm.StartPosition = FormStartPosition.CenterScreen;
                frm.ShowDialog();
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Form1_btnPOSMain_Click");
                MessageBox.Show("Unable to open POS.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                if (frm != null)
                {
                    try { frm.Dispose(); } catch { }
                }

                if (!this.IsDisposed)
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    this.BringToFront();
                    this.Activate();
                }
            }
        }

        private void btnAuditMain_Click(object sender, EventArgs e)
        {
            // Reserved for future design
        }

        private void btnMaintenanceMain_Click(object sender, EventArgs e)
        {
            try
            {
                if (Application.OpenForms["Maintenance"] is Maintenance existingForm)
                {
                    existingForm.BringToFront();
                    existingForm.Activate();
                    return;
                }

                HideSubMenu();
                CloseAllChildContent();

                foreach (Form openForm in Application.OpenForms.Cast<Form>().ToList())
                {
                    if (openForm == this) continue;
                    if (openForm is Maintenance) continue;

                    try
                    {
                        openForm.Close();
                    }
                    catch (Exception ex)
                    {
                        Program.SilentLog(ex, "Form1_btnMaintenanceMain_CloseOpenForm");
                    }
                }

                Maintenance frm = new Maintenance();
                frm.StartPosition = FormStartPosition.CenterScreen;

                frm.FormClosed += async (s, args) =>
                {
                    try
                    {
                        if (!this.IsDisposed)
                        {
                            this.Show();
                            this.WindowState = FormWindowState.Normal;
                            this.BringToFront();
                            this.Activate();
                            await MyDashbordAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Program.SilentLog(ex, "Form1_btnMaintenanceMain_FormClosed");
                    }
                    finally
                    {
                        try { frm.Dispose(); } catch { }
                    }
                };

                this.Hide();
                frm.Show();
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "Form1_btnMaintenanceMain_Click");
                MessageBox.Show("Unable to open Maintenance.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                if (!this.IsDisposed)
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    this.BringToFront();
                    this.Activate();
                }
            }
        }
        #endregion

        #region Utility Methods (Unchanged)
        public void LoadCart()
        {
            try
            {
                if (dataGridView1 == null) return;
                dataGridView1.Rows.Clear();
                int i = 0; double total = 0;
                using (SQLiteConnection cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    cn.Open();
                    string query = @"SELECT c.id, c.pcode, p.pdesc, c.price, c.qty, c.disc, c.total
                                     FROM tblCart1 c
                                     INNER JOIN TblProduct1 p ON c.pcode = p.pcode
                                     WHERE status='Pending' AND transno=@transno";

                    using (SQLiteCommand cm = new SQLiteCommand(query, cn))
                    {
                        cm.Parameters.AddWithValue("@transno", lblTransno.Text);
                        using (SQLiteDataReader dr = cm.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                i++;
                                total += Convert.ToDouble(dr["total"]);
                                dataGridView1.Rows.Add(i, dr["id"], dr["pcode"], dr["pdesc"],
                                    dr["price"], dr["qty"], dr["disc"], dr["total"]);
                            }
                        }
                    }
                }
                lblTotal.Text = total.ToString("#,##0.00");
                lblDisplayTotal.Text = total.ToString("#,##0.00");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        public void GetTransNo()
        {
            try
            {
                string sdate = DateTime.Now.ToString("yyyyMMdd");
                using (SQLiteConnection cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    cn.Open();
                    string sql = "SELECT transno FROM tblCart1 WHERE transno LIKE @sdate ORDER BY id DESC LIMIT 1";
                    using (SQLiteCommand cm = new SQLiteCommand(sql, cn))
                    {
                        cm.Parameters.AddWithValue("@sdate", sdate + "%");
                        object result = cm.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            string transno = result.ToString();
                            int count = int.Parse(transno.Substring(8, 4));
                            lblTransno.Text = sdate + (count + 1).ToString("D4");
                        }
                        else { lblTransno.Text = sdate + "1001"; }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
        #endregion

        #region Empty/Standard Events
        private void guna2ControlBox1_Click(object sender, EventArgs e) => Application.Exit();
        private void panel3_Paint(object sender, PaintEventArgs e) { }
        private void lblRole_Click(object sender, EventArgs e) { }
        private void lblName_Click(object sender, EventArgs e) { }
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void panelSubProduct_Paint(object sender, PaintEventArgs e) { }
        private void panelSubStocks_Paint(object sender, PaintEventArgs e) { }
        private void panelSubSettings_Paint(object sender, PaintEventArgs e) { }
        #endregion
    }
}