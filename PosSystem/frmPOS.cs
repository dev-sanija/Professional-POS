using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PosSystem
{
    public partial class frmPOS : Form
    {
        #region WINAPI FOR DRAGGING
        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0); }
        }
        #endregion

        string stitle = "POS System";
        Form1 f;
        bool isTransactionStarted = false;
        string logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PosSystem", "pos_error.log");
        private string lastPrintedTransNo = "";
        frmscanBarcode _activeScanner = null;
        private Timer _label6ResetTimer;
        private string _label6DefaultText = "Items In Cart: 0";
        private bool _label6WarningActive = false;
        private bool _scannerReadyState = false;
        private readonly Size _safeBaseWindowSize = new Size(1184, 639);

        public sealed class DiscountTarget
        {
            public int CartId { get; set; }
            public string PCode { get; set; }
            public string Description { get; set; }
            public int Qty { get; set; }
            public decimal Price { get; set; }
            public decimal CurrentDiscountPerUnit { get; set; }
        }

        public frmPOS(Form1 frm)
        {
            InitializeComponent();
            f = frm;
            this.KeyPreview = true;
            timer1.Start();
            LoadPrinterList();
            ConfigureGrid();

            // Safe barcode keyboard/scanner handling
            textBoxbarcode.KeyDown -= textBoxbarcode_KeyDown;
            textBoxbarcode.KeyDown += textBoxbarcode_KeyDown;

            // Runtime status/warning timer for label6
            _label6ResetTimer = new Timer();
            _label6ResetTimer.Interval = 2200;
            _label6ResetTimer.Tick += label6ResetTimer_Tick;

            this.FormClosing += frmPOS_FormClosing;
        }

        private void frmPOS_Load(object sender, EventArgs e)
        {
            LoadStoreInfo();
            CleanOrphanedCart();
            lblTransno.Text = "000000000000000000";
            lblDate.Text = DateTime.Now.ToShortDateString();

            if (f != null)
            {
                LblUser.Text = f._user;
                lblName.Text = f._name;
            }

            SetScannerReadyState(false);
            ApplySafeWindowLayout();
        }

        private void ConfigureGrid()
        {
            System.Reflection.PropertyInfo propertyInfo = typeof(DataGridView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            propertyInfo.SetValue(dataGridView1, true, null);

            if (dataGridView1.Columns.Contains("Column2")) dataGridView1.Columns["Column2"].Visible = false;
            if (dataGridView1.Columns.Contains("Column8")) dataGridView1.Columns["Column8"].Visible = false;
            if (dataGridView1.Columns.Contains("Column1")) dataGridView1.Columns["Column1"].Width = 40;
            if (dataGridView1.Columns.Contains("Column3")) dataGridView1.Columns["Column3"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dataGridView1.ScrollBars = ScrollBars.Both;
        }

        private void ApplySafeWindowLayout()
        {
            try
            {
                Rectangle workingArea = Screen.FromControl(this).WorkingArea;

                // Small and common shop screens
                if (workingArea.Width <= 1366 || workingArea.Height <= 800)
                {
                    WindowState = FormWindowState.Maximized;
                    return;
                }

                // Larger normal monitors
                WindowState = FormWindowState.Normal;
                Size target = new Size(
                    Math.Min(_safeBaseWindowSize.Width, workingArea.Width),
                    Math.Min(_safeBaseWindowSize.Height, workingArea.Height));

                Size = target;
                StartPosition = FormStartPosition.CenterScreen;
                CenterToScreen();
            }
            catch (Exception ex)
            {
                LogError("ApplySafeWindowLayout", ex);
            }
        }

        private void LoadPrinterList()
        {
            comboBoxprinter.Items.Clear();
            comboBoxprinter.Items.Add("Auto");
            comboBoxprinter.Items.Add("80mm Thermal");
            comboBoxprinter.Items.Add("58mm Thermal");

            var installedPrinters = PrinterSettings.InstalledPrinters.Cast<string>().ToList();
            foreach (string printer in installedPrinters)
                comboBoxprinter.Items.Add(printer);

            string savedPrinter = "Auto";
            try { savedPrinter = Properties.Settings.Default.DefaultPrinter; } catch { }

            comboBoxprinter.Text = (!string.IsNullOrEmpty(savedPrinter) && (savedPrinter == "Auto" || installedPrinters.Contains(savedPrinter)))
                                   ? savedPrinter : "Auto";
        }

        #region CORE LOGIC
        private bool CheckTransaction()
        {
            if (!isTransactionStarted || string.IsNullOrWhiteSpace(lblTransno.Text) || lblTransno.Text.Contains("000000000000"))
            {
                MessageBox.Show("Please click 'New Transaction' [F1] first!", stitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        public void GetTransNo()
        {
            try
            {
                string date = DateTime.Now.ToString("yyyyMMdd");
                string prefix = "TRX-" + date + "-";

                using (SQLiteConnection cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    cn.Open();
                    string sql = "SELECT transno FROM tblCart1 WHERE transno LIKE @prefix ORDER BY transno DESC LIMIT 1";
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("@prefix", prefix + "%");
                        object obj = cmd.ExecuteScalar();

                        if (obj != null)
                        {
                            string lastTransNo = obj.ToString();
                            string lastNumPart = lastTransNo.Substring(prefix.Length);
                            if (int.TryParse(lastNumPart, out int nextNum))
                            {
                                lblTransno.Text = prefix + (nextNum + 1).ToString("D5");
                            }
                            else
                            {
                                lblTransno.Text = prefix + "00001";
                            }
                        }
                        else
                        {
                            lblTransno.Text = prefix + "00001";
                        }
                    }
                }

                isTransactionStarted = true;
                lblDate.Text = DateTime.Now.ToShortDateString();
            }
            catch (Exception ex)
            {
                isTransactionStarted = false;
                LogError("GetTransNo", ex);
                MessageBox.Show("Database Error: " + ex.Message, stitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void LoadCart()
        {
            dataGridView1.Rows.Clear();
            decimal total = 0m;
            decimal discount = 0m;
            int itemsCount = 0;

            try
            {
                using (SQLiteConnection cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    cn.Open();
                    string sql = @"
                        SELECT c.*, p.pdesc
                        FROM tblCart1 c
                        JOIN TblProduct1 p ON c.pcode = p.pcode
                        WHERE c.transno = @t AND c.status = 'Pending'
                        ORDER BY c.id";

                    using (SQLiteCommand cmd = new SQLiteCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("@t", lblTransno.Text);
                        using (SQLiteDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                itemsCount++;

                                decimal price = Convert.ToDecimal(dr["price"]);
                                int qty = Convert.ToInt32(dr["qty"]);
                                decimal discPerUnit = dr["discount_per_unit"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["discount_per_unit"]);

                                decimal rowTotal = qty * (price - discPerUnit);
                                decimal rowDiscount = discPerUnit * qty;

                                total += rowTotal;
                                discount += rowDiscount;

                                dataGridView1.Rows.Add(
                                    itemsCount,                           // 0 - #
                                    dr["id"],                             // 1 - cart id
                                    dr["pcode"],                          // 2 - pcode
                                    dr["pdesc"],                          // 3 - description
                                    null,                                 // 4 - image placeholder
                                    qty,                                  // 5 - qty
                                    null,                                 // 6 - add
                                    null,                                 // 7 - remove
                                    price.ToString("N2"),                 // 8 - unit price
                                    rowDiscount.ToString("N2"),           // 9 - line discount
                                    rowTotal.ToString("N2"),              // 10 - line total
                                    discPerUnit                           // 11 - hidden/source discount per unit
                                );
                            }
                        }
                    }
                }

                lblTotal.Text = total.ToString("N2");
                lblDiscount.Text = discount.ToString("N2");
                lblDisplayTotal.Text = total.ToString("N2");

                decimal vatRate = 0.00m;
                decimal vat = total * vatRate;
                lblVat.Text = vat.ToString("N2");
                lblVatable.Text = (total - vat).ToString("N2");

                SetCartStatus(itemsCount);
            }
            catch (Exception ex)
            {
                LogError("LoadCart", ex);
                MessageBox.Show("Error loading cart: " + ex.Message, "POS Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RecalculateCart()
        {
            decimal grandTotal = 0m;
            decimal totalDiscount = 0m;

            try
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    decimal qty = SafeParse(row.Cells[5].Value);
                    decimal price = SafeParse(row.Cells[8].Value);
                    decimal discPerUnit = row.Cells.Count > 11 ? SafeParse(row.Cells[11].Value) : 0m;

                    decimal rowTotal = qty * (price - discPerUnit);
                    decimal rowDiscount = qty * discPerUnit;

                    row.Cells[9].Value = rowDiscount.ToString("N2");
                    row.Cells[10].Value = rowTotal.ToString("N2");

                    grandTotal += rowTotal;
                    totalDiscount += rowDiscount;
                }

                lblTotal.Text = grandTotal.ToString("N2");
                lblDiscount.Text = totalDiscount.ToString("N2");
                lblDisplayTotal.Text = grandTotal.ToString("N2");
            }
            catch (Exception ex)
            {
                LogError("RecalculateCart", ex);
            }
        }

        private decimal SafeParse(object value)
        {
            if (value == null || value == DBNull.Value || value is System.Drawing.Image)
                return 0m;

            string cleanValue = value.ToString().Replace(",", "");
            return decimal.TryParse(cleanValue, out decimal result) ? result : 0m;
        }

        private int SafeParseInt(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            int.TryParse(value.ToString(), out int result);
            return result;
        }

        public List<DiscountTarget> GetSelectedDiscountTargets()
        {
            var targets = new List<DiscountTarget>();
            var selectedRows = new List<DataGridViewRow>();

            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    selectedRows = dataGridView1.SelectedRows
                        .Cast<DataGridViewRow>()
                        .Where(r => !r.IsNewRow)
                        .OrderBy(r => r.Index)
                        .ToList();
                }
                else if (dataGridView1.CurrentRow != null && !dataGridView1.CurrentRow.IsNewRow)
                {
                    selectedRows.Add(dataGridView1.CurrentRow);
                }

                HashSet<int> seen = new HashSet<int>();

                foreach (DataGridViewRow row in selectedRows)
                {
                    int cartId = SafeParseInt(row.Cells[1].Value);
                    if (cartId <= 0 || seen.Contains(cartId)) continue;

                    int qty = SafeParseInt(row.Cells[5].Value);
                    decimal price = SafeParse(row.Cells[8].Value);
                    decimal currentDiscPerUnit = row.Cells.Count > 11 ? SafeParse(row.Cells[11].Value) : 0m;

                    targets.Add(new DiscountTarget
                    {
                        CartId = cartId,
                        PCode = row.Cells[2].Value?.ToString(),
                        Description = row.Cells[3].Value?.ToString(),
                        Qty = qty <= 0 ? 1 : qty,
                        Price = price,
                        CurrentDiscountPerUnit = currentDiscPerUnit
                    });

                    seen.Add(cartId);
                }
            }
            catch (Exception ex)
            {
                LogError("GetSelectedDiscountTargets", ex);
            }

            return targets;
        }

        private void ExecuteCartAction(string action, string pcode, string cartId = "")
        {
            using (SQLiteConnection cn = new SQLiteConnection(DBConnection.MyConnection()))
            {
                cn.Open();

                try
                {
                    if (action == "ADD_NEW" || action == "INCREMENT")
                    {
                        int stock = 0;
                        using (SQLiteCommand cmd = new SQLiteCommand("SELECT qty FROM TblProduct1 WHERE pcode=@p", cn))
                        {
                            cmd.Parameters.AddWithValue("@p", pcode);
                            object result = cmd.ExecuteScalar();
                            stock = (result != null) ? Convert.ToInt32(result) : 0;
                        }

                        int currentInCart = 0;
                        if (!string.IsNullOrEmpty(cartId))
                        {
                            using (SQLiteCommand cmd = new SQLiteCommand("SELECT qty FROM tblCart1 WHERE id=@id", cn))
                            {
                                cmd.Parameters.AddWithValue("@id", cartId);
                                object result = cmd.ExecuteScalar();
                                currentInCart = (result != null) ? Convert.ToInt32(result) : 0;
                            }
                        }

                        if (currentInCart >= stock)
                        {
                            MessageBox.Show($"Cannot add more. Remaining stock limit is {stock}!", stitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        string sql = string.IsNullOrEmpty(cartId)
                            ? @"INSERT INTO tblCart1
                                (transno, pcode, price, qty, sdate, status, total, [user], disc, discount_per_unit)
                                SELECT @tn, pcode, price, 1, @dt, 'Pending', price, @user, 0.00, 0.00
                                FROM TblProduct1
                                WHERE pcode=@pc"
                            : @"UPDATE tblCart1
                                SET qty = qty + 1,
                                    disc = ROUND((qty + 1) * IFNULL(discount_per_unit, 0), 4),
                                    total = ROUND((qty + 1) * (price - IFNULL(discount_per_unit, 0)), 4)
                                WHERE id=@id";

                        using (SQLiteCommand cmd = new SQLiteCommand(sql, cn))
                        {
                            cmd.Parameters.AddWithValue("@tn", lblTransno.Text);
                            cmd.Parameters.AddWithValue("@pc", pcode);
                            cmd.Parameters.AddWithValue("@id", cartId);
                            cmd.Parameters.AddWithValue("@dt", DateTime.Now.ToString("yyyy-MM-dd"));
                            cmd.Parameters.AddWithValue("@user", LblUser.Text);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else if (action == "DECREMENT")
                    {
                        using (SQLiteCommand cmd = new SQLiteCommand(@"
                            UPDATE tblCart1
                            SET qty = qty - 1,
                                disc = ROUND((qty - 1) * IFNULL(discount_per_unit, 0), 4),
                                total = ROUND((qty - 1) * (price - IFNULL(discount_per_unit, 0)), 4)
                            WHERE id=@id AND qty > 1", cn))
                        {
                            cmd.Parameters.AddWithValue("@id", cartId);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                MessageBox.Show("Quantity cannot be less than 1. Use 'Delete' to remove the item.", stitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    else if (action == "DELETE")
                    {
                        using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM tblCart1 WHERE id=@id", cn))
                        {
                            cmd.Parameters.AddWithValue("@id", cartId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError("ExecuteCartAction:" + action, ex);
                    MessageBox.Show("Error processing cart action: " + ex.Message, stitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            LoadCart();
        }

        public void AddToCart(string pcode)
        {
            if (!CheckTransaction()) return;

            try
            {
                using (SQLiteConnection cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    cn.Open();
                    string foundPcode = "";

                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT pcode FROM TblProduct1 WHERE (pcode=@p OR barcode=@p) AND isactive=1", cn))
                    {
                        cmd.Parameters.AddWithValue("@p", pcode);
                        foundPcode = cmd.ExecuteScalar()?.ToString();
                    }

                    if (string.IsNullOrEmpty(foundPcode))
                    {
                        ShowTemporaryWarning("Product not available in database/store");
                        textBoxbarcode.Clear();
                        FocusBarcodeBox();
                        return;
                    }

                    string cartId = "";
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT id FROM tblCart1 WHERE pcode=@pc AND transno=@tn AND status='Pending'", cn))
                    {
                        cmd.Parameters.AddWithValue("@pc", foundPcode);
                        cmd.Parameters.AddWithValue("@tn", lblTransno.Text);
                        cartId = cmd.ExecuteScalar()?.ToString() ?? "";
                    }

                    ExecuteCartAction(string.IsNullOrEmpty(cartId) ? "ADD_NEW" : "INCREMENT", foundPcode, cartId);
                }
            }
            catch (Exception ex)
            {
                LogError("AddToCart", ex);
            }
        }
        #endregion

        #region INTERFACE EVENTS
        private void frmPOS_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                btnTrans.PerformClick();
            }
            else if (e.KeyCode == Keys.F2)
            {
                if (CheckTransaction()) btnSearch.PerformClick();
            }
            else if (e.KeyCode == Keys.F3)
            {
                if (CheckTransaction()) btnDiscount.PerformClick();
            }
            else if (e.KeyCode == Keys.F4)
            {
                // Keep settle on F4 only.
                // Do NOT allow global Enter to settle because barcode scanners usually send Enter.
                if (!textBoxbarcode.Focused)
                    btnSattle.PerformClick();
            }
            else if (e.KeyCode == Keys.F5)
            {
                btnCancel.PerformClick();
            }
            else if (e.KeyCode == Keys.F6)
            {
                btnSales.PerformClick();
            }
            else if (e.KeyCode == Keys.F8)
            {
                // Keep existing all-in-one scanner form behavior
                btnscanbarcode.PerformClick();
            }
            else if (e.KeyCode == Keys.F10)
            {
                btnClose.PerformClick();
            }
        }

        private void SetCartStatus(int itemsCount)
        {
            try
            {
                string scannerText = _scannerReadyState ? "Scanner Ready" : "Scanner Off";
                _label6DefaultText = scannerText + " | Items In Cart: " + itemsCount;

                if (_label6WarningActive)
                    return;

                label6.ForeColor = Color.White;
                label6.Text = _label6DefaultText;
            }
            catch (Exception ex)
            {
                LogError("SetCartStatus", ex);
            }
        }

        private void ShowTemporaryWarning(string message)
        {
            try
            {
                _label6WarningActive = true;

                if (_label6ResetTimer != null)
                {
                    _label6ResetTimer.Stop();
                }

                label6.ForeColor = Color.Yellow;
                label6.Text = message;

                if (_label6ResetTimer != null)
                {
                    _label6ResetTimer.Start();
                }
            }
            catch (Exception ex)
            {
                LogError("ShowTemporaryWarning", ex);
            }
        }

        private void RestoreLabel6Status()
        {
            try
            {
                _label6WarningActive = false;
                label6.ForeColor = Color.White;
                label6.Text = _label6DefaultText;
            }
            catch (Exception ex)
            {
                LogError("RestoreLabel6Status", ex);
            }
        }

        public void SetScannerReadyState(bool isReady)
        {
            try
            {
                _scannerReadyState = isReady;
                SetCartStatus(dataGridView1.Rows.Count);
            }
            catch (Exception ex)
            {
                LogError("SetScannerReadyState", ex);
            }
        }

        private void ShowTemporarySuccess(string message)
        {
            try
            {
                _label6WarningActive = true;

                if (_label6ResetTimer != null)
                    _label6ResetTimer.Stop();

                label6.ForeColor = Color.LimeGreen;
                label6.Text = message;

                System.Media.SystemSounds.Asterisk.Play();

                if (_label6ResetTimer != null)
                    _label6ResetTimer.Start();
            }
            catch (Exception ex)
            {
                LogError("ShowTemporarySuccess", ex);
            }
        }

        private void label6ResetTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_label6ResetTimer != null)
                    _label6ResetTimer.Stop();

                RestoreLabel6Status();
            }
            catch (Exception ex)
            {
                LogError("label6ResetTimer_Tick", ex);
            }
        }

        private void btnTrans_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0)
            {
                if (MessageBox.Show("Clear current cart and start new transaction?", stitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }

            GetTransNo();
            _label6WarningActive = false;
            LoadCart();
            textBoxbarcode.Clear();
            FocusBarcodeBox();
            SetScannerReadyState(true);
        }

        private void btnSattle_Click(object sender, EventArgs e)
        {
            if (!CheckTransaction() || dataGridView1.Rows.Count == 0) return;

            frmSettel frm = new frmSettel(this);
            frm.txtSale.Text = lblDisplayTotal.Text;

            if (frm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    lastPrintedTransNo = lblTransno.Text;

                    KickDrawer();
                    PrintThermalBill(lastPrintedTransNo);

                    PrepareNextTransactionAfterPayment();
                }
                catch (Exception ex)
                {
                    LogError("btnSattle_Click", ex);
                    MessageBox.Show("Payment completed, but POS reset failed: " + ex.Message,
                        stitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    FocusBarcodeBox();
                }
            }
        }

        private void btnReprint_Click(object sender, EventArgs e)
        {
            try
            {
                using (frmRecentTransactions frm = new frmRecentTransactions())
                {
                    if (frm.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(frm.SelectedTransNo))
                    {
                        using (frmResipt receipt = new frmResipt(this))
                        {
                            receipt.LoadReportByTransactionNo(frm.SelectedTransNo);
                            receipt.ShowDialog();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("btnReprint_Click", ex);
                MessageBox.Show("Unable to open receipt reprint: " + ex.Message,
                    stitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            string id = dataGridView1.Rows[e.RowIndex].Cells["Column2"].Value?.ToString();
            string pcode = dataGridView1.Rows[e.RowIndex].Cells["Column8"].Value?.ToString();

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pcode)) return;

            if (colName.Equals("colAdd", StringComparison.OrdinalIgnoreCase) || colName.Equals("collAdd", StringComparison.OrdinalIgnoreCase))
            {
                ExecuteCartAction("INCREMENT", pcode, id);
            }
            else if (colName.Equals("colRemove", StringComparison.OrdinalIgnoreCase) || colName.Equals("ColRemove", StringComparison.OrdinalIgnoreCase))
            {
                ExecuteCartAction("DECREMENT", pcode, id);
            }
            else if (colName.Equals("colDelete", StringComparison.OrdinalIgnoreCase) || colName.Equals("Delete", StringComparison.OrdinalIgnoreCase))
            {
                if (MessageBox.Show("Remove this item from cart?", stitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    ExecuteCartAction("DELETE", pcode, id);
                }
            }
        }

        public void CleanOrphanedCart()
        {
            try
            {
                using (SQLiteConnection cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    cn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT COUNT(*) FROM tblCart1 WHERE status = 'Pending'", cn))
                    {
                        int pendingCount = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                        if (pendingCount > 0)
                        {
                            LogError("Recovery", new Exception($"Detected {pendingCount} preserved pending cart item(s) from a previous session."));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("CleanOrphanedCart", ex);
            }
        }

        private void SilentLog(Exception ex, string context)
        {
            Program.SilentLog(ex, context);
        }

        private void btnSalesHistory_Click(object sender, EventArgs e)
        {
            frmSoldItems frm = new frmSoldItems();
            frm.dt1.Value = DateTime.Now;
            frm.dt2.Value = DateTime.Now;
            frm._user = LblUser.Text;
            frm.ShowDialog();
        }
        #endregion

        #region MISSING HANDLERS
        private void button1_Click(object sender, EventArgs e)
        {
            if (!CheckTransaction())
            {
                textBoxbarcode.Clear();
                FocusBarcodeBox();
                return;
            }

            string barcode = NormalizeBarcodeText(textBoxbarcode.Text);

            if (string.IsNullOrWhiteSpace(barcode))
            {
                ShowTemporaryWarning("Please scan or enter a barcode");
                textBoxbarcode.Clear();
                FocusBarcodeBox();
                return;
            }

            AddToCart(barcode);

            textBoxbarcode.Clear();
            FocusBarcodeBox();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (!isTransactionStarted || lblTransno.Text.Contains("000000000000"))
            {
                frmCancelDetails frm = new frmCancelDetails(null, 0);
                frm.ShowDialog();
                return;
            }

            if (dataGridView1.Rows.Count == 0)
            {
                isTransactionStarted = false;
                lblTransno.Text = "000000000000000000";
                ResetCashierUiState();
                SetScannerReadyState(false);
                FocusBarcodeBox();
                return;
            }

            if (MessageBox.Show("Are you sure you want to CANCEL this transaction? All items in the cart will be removed.",
                stitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    using (SQLiteConnection cn = new SQLiteConnection(DBConnection.MyConnection()))
                    {
                        cn.Open();
                        using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM tblCart1 WHERE transno=@t AND status='Pending'", cn))
                        {
                            cmd.Parameters.AddWithValue("@t", lblTransno.Text);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    isTransactionStarted = false;
                    lblTransno.Text = "000000000000000000";
                    ResetCashierUiState();
                    SetScannerReadyState(false);

                    MessageBox.Show("Transaction cancelled successfully.", stitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    FocusBarcodeBox();
                }
                catch (Exception ex)
                {
                    LogError("CancelTransaction", ex);
                    MessageBox.Show("Error: Database is busy or inaccessible. Item removal failed.", stitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDiscount_Click(object sender, EventArgs e)
        {
            if (!CheckTransaction()) return;

            List<DiscountTarget> targets = GetSelectedDiscountTargets();
            if (targets.Count == 0)
            {
                MessageBox.Show("Please select one or more cart rows first.", stitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            frmDiscount frm = new frmDiscount(this);
            frm.SetDiscountTargets(targets);
            frm.OnDiscountApplied = (amount, percent) => LoadCart();
            frm.ShowDialog();
        }

        private void btnSales_Click(object sender, EventArgs e)
        {
            btnSalesHistory_Click(sender, e);
        }

        private void btnscanbarcode_Click(object sender, EventArgs e)
        {
            if (!CheckTransaction()) return;

            if (_activeScanner != null && !_activeScanner.IsDisposed)
            {
                _activeScanner.BringToFront();
                return;
            }

            _activeScanner = new frmscanBarcode(null, this);
            _activeScanner.Show();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (!CheckTransaction()) return;

            frmLookUp frm = new frmLookUp((pcode, price, desc, qty) =>
            {
                AddToCart(pcode);
            });
            frm.ShowDialog();
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e) { }
        private void label14_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label6_Click(object sender, EventArgs e) { }
        private void label7_Click(object sender, EventArgs e) { }
        private void lblAddress_Click(object sender, EventArgs e) { }
        private void lblDate_Click(object sender, EventArgs e) { }
        private void lblDiscount_Click(object sender, EventArgs e) { }
        private void lblName_Click(object sender, EventArgs e) { }
        private void lblPhone_Click(object sender, EventArgs e) { }
        private void lblSname_Click(object sender, EventArgs e) { }
        private void lblTotal_Click(object sender, EventArgs e) { }
        private void lblTransno_Click(object sender, EventArgs e) { }
        private void LblUser_Click(object sender, EventArgs e) { }
        private void lblVatable_Click(object sender, EventArgs e) { }
        private void lblVat_Click(object sender, EventArgs e) { }
        private void panel6_Paint(object sender, PaintEventArgs e) { }

        private void textBoxbarcode_TextChanged(object sender, EventArgs e)
        {
            // Intentionally left empty.
            // Barcode input is now handled by textBoxbarcode_KeyDown for reliable USB scanner / keyboard scanner support.
        }

        private void textBoxbarcode_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;

                    if (!CheckTransaction())
                    {
                        textBoxbarcode.Clear();
                        textBoxbarcode.Focus();
                        return;
                    }

                    string barcode = NormalizeBarcodeText(textBoxbarcode.Text);

                    if (string.IsNullOrWhiteSpace(barcode))
                    {
                        textBoxbarcode.Clear();
                        textBoxbarcode.Focus();
                        return;
                    }

                    // Your requested behavior:
                    // Enter inside barcode box must trigger button1
                    button1.PerformClick();
                }
            }
            catch (Exception ex)
            {
                LogError("textBoxbarcode_KeyDown", ex);
                textBoxbarcode.Focus();
            }
        }

        private string NormalizeBarcodeText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string value = input.Trim();

            // Remove scanner suffix leftovers if any
            value = value.Replace("\r", string.Empty)
                         .Replace("\n", string.Empty)
                         .Replace("\t", string.Empty);

            return value.Trim();
        }

        private void FocusBarcodeBox()
        {
            try
            {
                if (!textBoxbarcode.IsDisposed && textBoxbarcode.CanFocus)
                {
                    textBoxbarcode.Focus();
                    textBoxbarcode.SelectionStart = textBoxbarcode.TextLength;
                }
            }
            catch (Exception ex)
            {
                LogError("FocusBarcodeBox", ex);
            }
        }

        private void ResetCashierUiState()
        {
            try
            {
                textBoxbarcode.Clear();

                if (_label6ResetTimer != null)
                    _label6ResetTimer.Stop();

                _label6WarningActive = false;
                _label6DefaultText = "Items In Cart: 0";

                label6.ForeColor = Color.White;
                label6.Text = _label6DefaultText;

                lblTotal.Text = "0.00";
                lblDiscount.Text = "0.00";
                lblVat.Text = "0.00";
                lblVatable.Text = "0.00";
                lblDisplayTotal.Text = "0.00";

                dataGridView1.Rows.Clear();
            }
            catch (Exception ex)
            {
                LogError("ResetCashierUiState", ex);
            }
        }

        private void PrepareNextTransactionAfterPayment()
        {
            try
            {
                isTransactionStarted = false;
                lblTransno.Text = "000000000000000000";

                ResetCashierUiState();

                GetTransNo();
                LoadCart();
                FocusBarcodeBox();
                SetScannerReadyState(true);
                ShowTemporarySuccess("Payment saved - Ready for next customer");
            }
            catch (Exception ex)
            {
                LogError("PrepareNextTransactionAfterPayment", ex);
                FocusBarcodeBox();
            }
        }

        private void CleanupActiveScanner()
        {
            try
            {
                if (_activeScanner != null)
                {
                    if (!_activeScanner.IsDisposed)
                    {
                        _activeScanner.Close();
                        _activeScanner.Dispose();
                    }

                    _activeScanner = null;
                }
            }
            catch (Exception ex)
            {
                LogError("CleanupActiveScanner", ex);
            }
        }

        private bool IsThermalModeSelection(string selectedValue)
        {
            return string.Equals(selectedValue, "80mm Thermal", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(selectedValue, "58mm Thermal", StringComparison.OrdinalIgnoreCase);
        }

        private float GetSelectedPaperWidth()
        {
            string selected = comboBoxprinter.Text?.Trim() ?? string.Empty;
            return string.Equals(selected, "58mm Thermal", StringComparison.OrdinalIgnoreCase) ? 200f : 280f;
        }

        public string ResolvePrinterNameForPrinting()
        {
            try
            {
                string selected = comboBoxprinter.Text?.Trim() ?? string.Empty;
                List<string> installedPrinters = PrinterSettings.InstalledPrinters.Cast<string>().ToList();
                string systemDefaultPrinter = new PrinterSettings().PrinterName;

                if (string.Equals(selected, "Auto", StringComparison.OrdinalIgnoreCase))
                {
                    return systemDefaultPrinter;
                }

                string exactInstalled = installedPrinters
                    .FirstOrDefault(p => string.Equals(p, selected, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(exactInstalled))
                {
                    return exactInstalled;
                }

                if (IsThermalModeSelection(selected))
                {
                    string thermalPrinter = FindBestReceiptPrinter(installedPrinters);
                    if (!string.IsNullOrWhiteSpace(thermalPrinter))
                        return thermalPrinter;

                    if (!string.IsNullOrWhiteSpace(systemDefaultPrinter))
                        return systemDefaultPrinter;
                }

                return systemDefaultPrinter;
            }
            catch (Exception ex)
            {
                LogError("ResolvePrinterNameForPrinting", ex);
                return new PrinterSettings().PrinterName;
            }
        }

        private string FindBestReceiptPrinter(List<string> installedPrinters)
        {
            try
            {
                if (installedPrinters == null || installedPrinters.Count == 0)
                    return null;

                string[] preferredKeywords =
                {
            "thermal", "receipt", "pos", "epson", "tm-t", "xp-", "rp", "80", "58"
        };

                List<string> filtered = installedPrinters
                    .Where(p => !IsVirtualPrinter(p))
                    .ToList();

                string keywordMatch = filtered.FirstOrDefault(p =>
                    preferredKeywords.Any(k => p.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0));

                if (!string.IsNullOrWhiteSpace(keywordMatch))
                    return keywordMatch;

                // If only one real non-virtual printer exists, use it
                if (filtered.Count == 1)
                    return filtered[0];

                return null;
            }
            catch (Exception ex)
            {
                LogError("FindBestReceiptPrinter", ex);
                return null;
            }
        }

        private bool IsVirtualPrinter(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName))
                return true;

            string[] virtualKeywords =
            {
        "pdf", "xps", "onenote", "fax"
    };

            return virtualKeywords.Any(k => printerName.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
        }
        #endregion

        #region PRINTING
        private void PrintThermalBill(string transNo)
        {
            if (string.IsNullOrWhiteSpace(transNo))
                return;

            try
            {
                string resolvedPrinterName = ResolvePrinterNameForPrinting();

                if (string.IsNullOrWhiteSpace(resolvedPrinterName))
                {
                    MessageBox.Show("No valid printer was found for receipt printing.",
                        stitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                using (PrintDocument pd = new PrintDocument())
                {
                    pd.PrinterSettings.PrinterName = resolvedPrinterName;

                    if (!pd.PrinterSettings.IsValid)
                    {
                        MessageBox.Show("Selected printer is not valid or not available:\n" + resolvedPrinterName,
                            stitle,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    pd.PrintPage += (s, ev) =>
                    {
                        Graphics g = ev.Graphics;
                        float paperWidth = GetSelectedPaperWidth();

                        using (Font fRegular = new Font("Courier New", 9))
                        using (Font fBold = new Font("Courier New", 10, FontStyle.Bold))
                        using (StringFormat sCenter = new StringFormat { Alignment = StringAlignment.Center })
                        using (StringFormat sRight = new StringFormat { Alignment = StringAlignment.Far })
                        {
                            float y = 10;
                            decimal grandTotal = 0m;

                            g.DrawString(lblSname.Text, fBold, Brushes.Black, new RectangleF(0, y, paperWidth, 20), sCenter);
                            y += 20;
                            g.DrawString("TRANS: " + transNo, fRegular, Brushes.Black, 5, y);
                            y += 15;
                            g.DrawString(new string('-', 30), fRegular, Brushes.Black, 5, y);
                            y += 15;

                            using (SQLiteConnection cn = new SQLiteConnection(DBConnection.MyConnection()))
                            {
                                cn.Open();

                                using (SQLiteCommand cmd = new SQLiteCommand(@"
                            SELECT c.qty, c.price, c.total, p.pdesc
                            FROM tblCart1 c
                            INNER JOIN TblProduct1 p ON c.pcode = p.pcode
                            WHERE c.transno = @t
                            ORDER BY c.id", cn))
                                {
                                    cmd.Parameters.AddWithValue("@t", transNo);

                                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                                    {
                                        while (dr.Read())
                                        {
                                            string description = dr["pdesc"]?.ToString() ?? string.Empty;
                                            decimal qty = dr["qty"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["qty"]);
                                            decimal price = dr["price"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["price"]);
                                            decimal lineTotal = dr["total"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["total"]);

                                            grandTotal += lineTotal;

                                            g.DrawString(description, fRegular, Brushes.Black, 5, y);
                                            y += 15;
                                            g.DrawString($"{qty:N0} x {price:N2}", fRegular, Brushes.Black, 15, y);
                                            g.DrawString(lineTotal.ToString("N2"), fRegular, Brushes.Black, paperWidth - 5, y, sRight);
                                            y += 18;
                                        }
                                    }
                                }
                            }

                            y += 10;
                            g.DrawString("TOTAL: " + grandTotal.ToString("N2"), fBold, Brushes.Black, paperWidth - 5, y, sRight);
                        }
                    };

                    pd.Print();
                }
            }
            catch (Exception ex)
            {
                LogError("PrintThermalBill", ex);
                MessageBox.Show("Receipt printing failed: " + ex.Message,
                    stitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public void KickDrawer()
        {
            try
            {
                string resolvedPrinterName = ResolvePrinterNameForPrinting();

                if (string.IsNullOrWhiteSpace(resolvedPrinterName))
                    return;

                byte[] code = new byte[] { 27, 112, 0, 25, 250 };
                RawPrinterHelper.SendBytesToPrinter(resolvedPrinterName, code);
            }
            catch (Exception ex)
            {
                LogError("KickDrawer", ex);
            }
        }
        #endregion

        #region UTILS
        private void timer1_Tick(object sender, EventArgs e)
        {
            label2.Text = DateTime.Now.ToString("hh:mm:ss tt");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LoadStoreInfo()
        {
            try
            {
                using (SQLiteConnection cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    cn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM tblStore LIMIT 1", cn))
                    {
                        using (SQLiteDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                lblName.Text = dr["store"].ToString();
                                lblSname.Text = dr["store"].ToString();
                                lblAddress.Text = dr["address"].ToString();
                                lblPhone.Text = dr["phone"].ToString();
                            }
                            else
                            {
                                lblName.Text = "ELITE POS";
                                lblSname.Text = "ELITE POS";
                                lblAddress.Text = "No Address Set";
                                lblPhone.Text = "000-0000";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("LoadStoreInfo", ex);
                lblSname.Text = "ELITE POS";
            }
        }

        private void LogError(string ctx, Exception ex)
        {
            try
            {
                string dir = Path.GetDirectoryName(logFile);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.AppendAllText(logFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{ctx}] {ex.Message}{Environment.NewLine}");
            }
            catch { }
        }

        private void frmPOS_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (dataGridView1.Rows.Count > 0 && isTransactionStarted)
            {
                DialogResult dr = MessageBox.Show("There is an active transaction in the cart.\n\n" +
                    "Click YES to CANCEL and clear the cart before exiting.\n" +
                    "Click NO to exit and KEEP the items (orphaned).\n" +
                    "Click CANCEL to return to POS.",
                    stitle, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (dr == DialogResult.Yes)
                {
                    try
                    {
                        using (SQLiteConnection cn = new SQLiteConnection(DBConnection.MyConnection()))
                        {
                            cn.Open();
                            using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM tblCart1 WHERE transno=@t AND status='Pending'", cn))
                            {
                                cmd.Parameters.AddWithValue("@t", lblTransno.Text);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex) { LogError("ClosingCleanup", ex); }
                }
                else if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            try
            {
                CleanupActiveScanner();

                if (timer1 != null)
                {
                    timer1.Stop();
                    timer1.Enabled = false;
                    timer1.Tick -= timer1_Tick;
                    timer1.Dispose();
                }

                if (_label6ResetTimer != null)
                {
                    _label6ResetTimer.Stop();
                    _label6ResetTimer.Tick -= label6ResetTimer_Tick;
                    _label6ResetTimer.Dispose();
                    _label6ResetTimer = null;
                }

                f = null;

                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();
            }
            catch (Exception ex)
            {
                LogError("MemoryDisposal", ex);
            }
        }

        private void comboBoxprinter_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Properties.Settings.Default.DefaultPrinter = comboBoxprinter.Text;
                Properties.Settings.Default.Save();
            }
            catch { }
        }
        #endregion

        private void btnReprint_Click_1(object sender, EventArgs e)
        {
            btnReprint_Click(sender, e);
        }

        private void comboBoxprinter_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            comboBoxprinter_SelectedIndexChanged(sender, e);
        }
    }

    public class RawPrinterHelper
    {
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)] public string pDatatype;
        }

        public static bool SendBytesToPrinter(string szPrinterName, byte[] pBytes)
        {
            IntPtr hPrinter = new IntPtr(0);
            DOCINFOA di = new DOCINFOA();
            bool bSuccess = false;
            di.pDocName = "POS Receipt";
            di.pDatatype = "RAW";

            if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    if (StartPagePrinter(hPrinter))
                    {
                        IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(pBytes.Length);
                        Marshal.Copy(pBytes, 0, pUnmanagedBytes, pBytes.Length);
                        bSuccess = WritePrinter(hPrinter, pUnmanagedBytes, pBytes.Length, out _);
                        EndPagePrinter(hPrinter);
                        Marshal.FreeCoTaskMem(pUnmanagedBytes);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }

            return bSuccess;
        }
    }
}