using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosSystem
{
    public partial class frmStockin : Form
    {
        // --- DRAGGABLE LOGIC (WinAPI) ---
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private void panelTitle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        // -------------------------------

        private readonly string stitle = "POS System Management";
        private readonly string connStr;

        private readonly List<VendorLookupItem> _vendors = new List<VendorLookupItem>();
        private bool _isUpdatingVendorText;

        private int _entryPage = 0;
        private int _historyPage = 0;
        private const int PageSize = 50;
        private int _entryTotalRecords = 0;
        private int _historyTotalRecords = 0;

        public frmStockin(string connectionString)
        {
            InitializeComponent();
            connStr = connectionString;

            SetupEnterpriseUI();

            if (!string.IsNullOrWhiteSpace(connStr))
            {
                LoadVendor();
            }
        }

        private void SetupEnterpriseUI()
        {
            try
            {
                panel1.MouseDown += panelTitle_MouseDown;
                label2.MouseDown += panelTitle_MouseDown;

                dt1.Format = DateTimePickerFormat.Custom;
                dt1.CustomFormat = "yyyy-MM-dd HH:mm";

                txtAddress.ReadOnly = true;
                txtPerson.ReadOnly = true;
                txtAddress.TabStop = false;
                txtPerson.TabStop = false;

                lblVendorID.Visible = true;
                lblVendorID.Text = "Vendor Id : 0";

                cbVendor.DropDownStyle = ComboBoxStyle.DropDown;

                pnlstockentrygridprevious.Cursor = Cursors.Hand;
                pnlstockentrygridnext.Cursor = Cursors.Hand;
                pnlstockhistoryprevious.Cursor = Cursors.Hand;
                pnlstockhistorynext.Cursor = Cursors.Hand;

                pnlstockentrygridprevious.Click += pnlstockentrygridprevious_Click;
                pnlstockentrygridnext.Click += pnlstockentrygridnext_Click;
                pnlstockhistoryprevious.Click += pnlstockhistoryprevious_Click;
                pnlstockhistorynext.Click += pnlstockhistorynext_Click;

                pnlstockentrygridprevious.Paint += pnlstockentrygridprevious_Paint;
                pnlstockentrygridnext.Paint += pnlstockentrygridnext_Paint;
                pnlstockhistoryprevious.Paint += pnlstockhistoryprevious_Paint;
                pnlstockhistorynext.Paint += pnlstockhistorynext_Paint;

                ConfigureGridVisuals();
            }
            catch (Exception ex)
            {
                TrySilentLog(ex, "frmStockin_SetupEnterpriseUI");
            }
        }

        private void ConfigureGridVisuals()
        {
            ConfigureGrid(dataGridView2);
            ConfigureGrid(dataGridView1);
        }

        private void ConfigureGrid(DataGridView grid)
        {
            if (grid == null) return;

            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AllowUserToResizeColumns = false;
            grid.MultiSelect = false;
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 30;
            grid.RowTemplate.Height = 26;

            try
            {
                typeof(DataGridView)
                    .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    ?.SetValue(grid, true, null);
            }
            catch { }

            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void frmStockin_Load(object sender, EventArgs e)
        {
            dt1.Value = DateTime.Now;
            date1.Value = DateTime.Today.AddDays(-30);
            date2.Value = DateTime.Today;

            GenerateReferenceNo();
            _entryPage = 0;
            _historyPage = 0;
            LoadStockIn();
            LoadStockHistory();
        }

        public void GenerateReferenceNo()
        {
            try
            {
                string sdate = dt1.Value.ToString("yyyyMMdd");
                int nextNumber = 1001;

                using (var cn = new SQLiteConnection(connStr))
                {
                    cn.Open();
                    string query = "SELECT refno FROM tblStockIn WHERE refno LIKE @sdate ORDER BY refno DESC LIMIT 1";
                    using (var cmd = new SQLiteCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@sdate", sdate + "%");
                        var lastRef = cmd.ExecuteScalar()?.ToString();

                        if (!string.IsNullOrEmpty(lastRef) && lastRef.Length >= 12)
                        {
                            string suffix = lastRef.Substring(8);
                            if (int.TryParse(suffix, out int lastCount))
                                nextNumber = lastCount + 1;
                        }
                    }
                }

                txtRefNo.Text = sdate + nextNumber;
                _entryPage = 0;
                LoadStockIn();
            }
            catch (Exception ex)
            {
                TrySilentLog(ex, "frmStockin_GenerateReferenceNo");
                MessageBox.Show("Ref Error: " + ex.Message, stitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void LoadStockIn()
        {
            dataGridView2.Rows.Clear();

            try
            {
                using (var cn = new SQLiteConnection(connStr))
                {
                    cn.Open();

                    using (var countCmd = new SQLiteCommand(@"
                        SELECT COUNT(*)
                        FROM tblStockIn
                        WHERE refno = @refno
                          AND UPPER(TRIM(IFNULL(status,''))) = 'PENDING';", cn))
                    {
                        countCmd.Parameters.AddWithValue("@refno", txtRefNo.Text.Trim());
                        object result = countCmd.ExecuteScalar();
                        _entryTotalRecords = result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
                    }

                    string query = @"
                        SELECT
                            s.id,
                            s.refno,
                            s.pcode,
                            p.pdesc,
                            IFNULL(s.action, 'ADD') AS action,
                            CASE
                                WHEN UPPER(TRIM(IFNULL(s.action,''))) IN ('REMOVE', 'REMOVE FROM INVENTORY')
                                    THEN -ABS(IFNULL(s.qty, 0))
                                ELSE ABS(IFNULL(s.qty, 0))
                            END AS display_qty,
                            SUBSTR(IFNULL(s.sdate,''), 1, 10) AS sdate,
                            IFNULL(s.stime, '') AS stime,
                            IFNULL(s.stockinby, '') AS stockinby,
                            IFNULL(v.vendor, '') AS vendor
                        FROM tblStockIn s
                        INNER JOIN TblProduct1 p ON s.pcode = p.pcode
                        LEFT JOIN tblVendor v ON s.vendorid = v.id
                        WHERE s.refno = @refno
                          AND UPPER(TRIM(IFNULL(s.status,''))) = 'PENDING'
                        ORDER BY s.id DESC
                        LIMIT @limit OFFSET @offset;";

                    using (var cmd = new SQLiteCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@refno", txtRefNo.Text.Trim());
                        cmd.Parameters.AddWithValue("@limit", PageSize);
                        cmd.Parameters.AddWithValue("@offset", _entryPage * PageSize);

                        using (var dr = cmd.ExecuteReader())
                        {
                            int i = _entryPage * PageSize;
                            while (dr.Read())
                            {
                                i++;
                                dataGridView2.Rows.Add(
                                    i,
                                    dr["id"],
                                    dr["refno"],
                                    dr["pcode"],
                                    dr["pdesc"],
                                    dr["action"],
                                    dr["display_qty"],
                                    dr["sdate"],
                                    dr["stime"],
                                    dr["stockinby"],
                                    dr["vendor"]
                                );
                            }
                        }
                    }
                }

                pnlstockentrygridprevious.Invalidate();
                pnlstockentrygridnext.Invalidate();
            }
            catch (Exception ex)
            {
                TrySilentLog(ex, "frmStockin_LoadStockIn");
                MessageBox.Show("Load Error: " + ex.Message, stitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (dataGridView2.Rows.Count == 0)
            {
                MessageBox.Show("No items in the list to save.", stitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to save this stock entry?", stitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var stockList = new List<StockInModel>();

            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Cells[1].Value == null ||
                    row.Cells[3].Value == null ||
                    row.Cells[6].Value == null ||
                    row.Cells[6].Value == DBNull.Value)
                    continue;

                int qty;
                if (!int.TryParse(row.Cells[6].Value.ToString(), out qty))
                    continue;

                stockList.Add(new StockInModel
                {
                    ID = row.Cells[1].Value.ToString(),
                    PCode = row.Cells[3].Value.ToString(),
                    Qty = Math.Abs(qty),
                    Action = Convert.ToString(row.Cells[5].Value),
                    RefNo = Convert.ToString(row.Cells[2].Value),
                    SDate = Convert.ToString(row.Cells[7].Value),
                    STime = Convert.ToString(row.Cells[8].Value),
                    StockInBy = Convert.ToString(row.Cells[9].Value)
                });
            }

            if (stockList.Count == 0)
            {
                MessageBox.Show("No valid items found to save.", stitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnSave.Enabled = false;
            UseWaitCursor = true;

            try
            {
                string currentVendorId = GetCurrentVendorId();
                bool applyVendor = !string.IsNullOrWhiteSpace(currentVendorId) && currentVendorId != "0";

                string currentStockInBy = txtBy.Text.Trim();
                bool applyStockInBy = !string.IsNullOrWhiteSpace(currentStockInBy);

                string selectedDate = dt1.Value.ToString("yyyy-MM-dd");
                string selectedTime = dt1.Value.ToString("HH:mm");

                await Task.Run(() =>
                {
                    using (var cn = new SQLiteConnection(connStr))
                    {
                        cn.Open();

                        using (var tran = cn.BeginTransaction())
                        {
                            try
                            {
                                foreach (var item in stockList)
                                {
                                    bool isRemove = IsRemoveAction(item.Action);
                                    int delta = isRemove ? -item.Qty : item.Qty;

                                    using (var stockCmd = new SQLiteCommand("SELECT IFNULL(qty,0) FROM TblProduct1 WHERE pcode = @pcode", cn, tran))
                                    {
                                        stockCmd.Parameters.AddWithValue("@pcode", item.PCode);
                                        object result = stockCmd.ExecuteScalar();
                                        int currentQty = result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);

                                        int newQty = currentQty + delta;
                                        if (newQty < 0)
                                            throw new InvalidOperationException("Action results in negative stock for product " + item.PCode);

                                        using (var upd = new SQLiteCommand("UPDATE TblProduct1 SET qty = @qty WHERE pcode = @pcode", cn, tran))
                                        {
                                            upd.Parameters.AddWithValue("@qty", newQty);
                                            upd.Parameters.AddWithValue("@pcode", item.PCode);
                                            upd.ExecuteNonQuery();
                                        }
                                    }

                                    using (var cmd = new SQLiteCommand(@"
                                        UPDATE tblStockIn
                                        SET
                                            status = 'Done',
                                            vendorid = CASE
                                                WHEN @applyVendor = 1 AND (vendorid IS NULL OR vendorid = 0)
                                                    THEN @vid
                                                ELSE vendorid
                                            END,
                                            stockinby = CASE
                                                WHEN @applyBy = 1 AND (stockinby IS NULL OR TRIM(stockinby) = '')
                                                    THEN @by
                                                ELSE stockinby
                                            END,
                                            sdate = CASE
                                                WHEN sdate IS NULL OR TRIM(sdate) = ''
                                                    THEN @sdate
                                                ELSE sdate
                                            END,
                                            stime = CASE
                                                WHEN stime IS NULL OR TRIM(stime) = ''
                                                    THEN @stime
                                                ELSE stime
                                            END,
                                            action = CASE
                                                WHEN action IS NULL OR TRIM(action) = ''
                                                    THEN @action
                                                ELSE action
                                            END,
                                            remarks = IFNULL(remarks, '')
                                        WHERE id = @id;", cn, tran))
                                    {
                                        cmd.Parameters.AddWithValue("@applyVendor", applyVendor ? 1 : 0);
                                        cmd.Parameters.AddWithValue("@vid", applyVendor ? (object)currentVendorId : DBNull.Value);
                                        cmd.Parameters.AddWithValue("@applyBy", applyStockInBy ? 1 : 0);
                                        cmd.Parameters.AddWithValue("@by", applyStockInBy ? (object)currentStockInBy : DBNull.Value);
                                        cmd.Parameters.AddWithValue("@sdate", selectedDate);
                                        cmd.Parameters.AddWithValue("@stime", selectedTime);
                                        cmd.Parameters.AddWithValue("@action", string.IsNullOrWhiteSpace(item.Action) ? "ADD" : item.Action.Trim());
                                        cmd.Parameters.AddWithValue("@id", item.ID);
                                        cmd.ExecuteNonQuery();
                                    }
                                }

                                tran.Commit();
                            }
                            catch
                            {
                                try { tran.Rollback(); } catch { }
                                throw;
                            }
                        }
                    }
                });

                MessageBox.Show("Stock records updated successfully.", stitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                Clear();
                _entryPage = 0;
                _historyPage = 0;
                LoadStockIn();
                LoadStockHistory();
            }
            catch (Exception ex)
            {
                TrySilentLog(ex, "frmStockin_btnSave_Click");
                MessageBox.Show("Save Error: " + ex.Message, stitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
                btnSave.Enabled = true;
            }
        }

        public void LoadStockHistory()
        {
            dataGridView1.Rows.Clear();

            try
            {
                using (var cn = new SQLiteConnection(connStr))
                {
                    cn.Open();

                    using (var countCmd = new SQLiteCommand(@"
                        SELECT COUNT(*)
                        FROM vwStockMovementHistory
                        WHERE DATE(sdate) BETWEEN @d1 AND @d2;", cn))
                    {
                        countCmd.Parameters.AddWithValue("@d1", date1.Value.ToString("yyyy-MM-dd"));
                        countCmd.Parameters.AddWithValue("@d2", date2.Value.ToString("yyyy-MM-dd"));
                        object result = countCmd.ExecuteScalar();
                        _historyTotalRecords = result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
                    }

                    string query = @"
                        SELECT
                            id,
                            refno,
                            pcode,
                            pdesc,
                            action,
                            CASE
                                WHEN UPPER(TRIM(IFNULL(action,''))) IN ('REMOVE', 'REMOVE FROM INVENTORY')
                                    THEN -ABS(IFNULL(qty, 0))
                                ELSE ABS(IFNULL(qty, 0))
                            END AS display_qty,
                            sdate,
                            [user],
                            vendor,
                            remarks
                        FROM vwStockMovementHistory
                        WHERE DATE(sdate) BETWEEN @d1 AND @d2
                        ORDER BY DATE(sdate) DESC, id DESC
                        LIMIT @limit OFFSET @offset;";

                    using (var cmd = new SQLiteCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@d1", date1.Value.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@d2", date2.Value.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@limit", PageSize);
                        cmd.Parameters.AddWithValue("@offset", _historyPage * PageSize);

                        using (var dr = cmd.ExecuteReader())
                        {
                            int i = _historyPage * PageSize;
                            while (dr.Read())
                            {
                                i++;
                                dataGridView1.Rows.Add(
                                    i,
                                    dr["id"],
                                    dr["refno"],
                                    dr["pcode"],
                                    dr["pdesc"],
                                    dr["action"],
                                    dr["display_qty"],
                                    dr["sdate"],
                                    dr["user"],
                                    dr["vendor"],
                                    dr["remarks"]
                                );
                            }
                        }
                    }
                }

                pnlstockhistoryprevious.Invalidate();
                pnlstockhistorynext.Invalidate();
            }
            catch (Exception ex)
            {
                TrySilentLog(ex, "frmStockin_LoadStockHistory");
                MessageBox.Show("History Error: " + ex.Message, stitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void load_Click(object sender, EventArgs e)
        {
            _historyPage = 0;
            LoadStockHistory();
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dataGridView2.Columns[e.ColumnIndex].Name;

            if (colName == "Delete")
            {
                if (MessageBox.Show("Remove this item from pending list?", stitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                try
                {
                    using (var cn = new SQLiteConnection(connStr))
                    {
                        cn.Open();
                        using (var cmd = new SQLiteCommand("DELETE FROM tblStockIn WHERE id = @id AND UPPER(TRIM(IFNULL(status,''))) = 'PENDING'", cn))
                        {
                            cmd.Parameters.AddWithValue("@id", dataGridView2.Rows[e.RowIndex].Cells[1].Value);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    LoadStockIn();
                    LoadStockHistory();
                }
                catch (Exception ex)
                {
                    TrySilentLog(ex, "frmStockin_dataGridView2_CellContentClick_Delete");
                    MessageBox.Show("Delete Error: " + ex.Message, stitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void LoadVendor()
        {
            cbVendor.Items.Clear();
            _vendors.Clear();

            try
            {
                using (var cn = new SQLiteConnection(connStr))
                {
                    cn.Open();
                    using (var cmd = new SQLiteCommand(@"
                        SELECT id, vendor, address, contactperson
                        FROM tblVendor
                        ORDER BY vendor;", cn))
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var item = new VendorLookupItem
                            {
                                Id = Convert.ToString(dr["id"]),
                                Vendor = Convert.ToString(dr["vendor"]),
                                Address = Convert.ToString(dr["address"]),
                                ContactPerson = Convert.ToString(dr["contactperson"])
                            };

                            _vendors.Add(item);
                            cbVendor.Items.Add(item.Vendor);
                        }
                    }
                }

                var auto = new AutoCompleteStringCollection();
                auto.AddRange(_vendors.Select(v => v.Vendor).Distinct().ToArray());
                cbVendor.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cbVendor.AutoCompleteSource = AutoCompleteSource.CustomSource;
                cbVendor.AutoCompleteCustomSource = auto;
            }
            catch (Exception ex)
            {
                TrySilentLog(ex, "frmStockin_LoadVendor");
                MessageBox.Show("Vendor Load Error: " + ex.Message, stitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cbVendor_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingVendorText) return;

            try
            {
                string input = (cbVendor.Text ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(input))
                {
                    lblVendorID.Text = "Vendor Id : 0";
                    txtAddress.Clear();
                    txtPerson.Clear();
                    return;
                }

                VendorLookupItem match = _vendors.FirstOrDefault(v =>
                    string.Equals(v.Vendor, input, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(v.Id, input, StringComparison.OrdinalIgnoreCase));

                if (match == null)
                {
                    match = _vendors.FirstOrDefault(v =>
                        v.Vendor.StartsWith(input, StringComparison.OrdinalIgnoreCase) ||
                        v.Id.StartsWith(input, StringComparison.OrdinalIgnoreCase));
                }

                if (match != null)
                {
                    lblVendorID.Text = "Vendor Id : " + match.Id;
                    txtAddress.Text = match.Address ?? string.Empty;
                    txtPerson.Text = match.ContactPerson ?? string.Empty;

                    if (!string.Equals(cbVendor.Text, match.Vendor, StringComparison.Ordinal))
                    {
                        _isUpdatingVendorText = true;
                        cbVendor.Text = match.Vendor;
                        cbVendor.SelectionStart = cbVendor.Text.Length;
                        cbVendor.SelectionLength = 0;
                        _isUpdatingVendorText = false;
                    }
                }
                else
                {
                    lblVendorID.Text = "Vendor Id : 0";
                    txtAddress.Clear();
                    txtPerson.Clear();
                }
            }
            catch (Exception ex)
            {
                TrySilentLog(ex, "frmStockin_cbVendor_TextChanged");
            }
        }

        private string GetCurrentVendorId()
        {
            string raw = lblVendorID.Text ?? string.Empty;
            const string prefix = "Vendor Id :";
            if (raw.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return raw.Substring(prefix.Length).Trim();

            return raw.Trim();
        }

        private void Clear()
        {
            txtBy.Clear();
            txtAddress.Clear();
            txtPerson.Clear();
            cbVendor.Text = string.Empty;
            lblVendorID.Text = "Vendor Id : 0";
            dt1.Value = DateTime.Now;
            GenerateReferenceNo();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var frm = new frmSearchProductStokin(this);
            frm.ShowDialog();
            LoadStockIn();
            LoadStockHistory();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GenerateReferenceNo();
        }

        private void pictureBox1_Click(object sender, EventArgs e) => this.Dispose();

        private void cbVendor_KeyPress(object sender, KeyPressEventArgs e)
        {
            // allow typing because user wants combo-box search by vendor name or vendor id
            e.Handled = false;
        }

        private void txtRefNo_TextChanged(object sender, EventArgs e)
        {
            _entryPage = 0;
            LoadStockIn();
        }

        private void dt1_ValueChanged(object sender, EventArgs e)
        {
            string selectedPrefix = dt1.Value.ToString("yyyyMMdd");

            if (string.IsNullOrWhiteSpace(txtRefNo.Text) || !txtRefNo.Text.StartsWith(selectedPrefix))
            {
                GenerateReferenceNo();
            }
        }

        private void date1_ValueChanged(object sender, EventArgs e)
        {
            _historyPage = 0;
            LoadStockHistory();
        }

        private void date2_ValueChanged(object sender, EventArgs e)
        {
            _historyPage = 0;
            LoadStockHistory();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void cbVendor_SelectedIndexChanged(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void textBox2_TextChanged(object sender, EventArgs e) { }
        private void lblVendorID_Click(object sender, EventArgs e) { }

        private void pnlstockentrygridprevious_Click(object sender, EventArgs e)
        {
            if (_entryPage <= 0) return;
            _entryPage--;
            LoadStockIn();
        }

        private void pnlstockentrygridnext_Click(object sender, EventArgs e)
        {
            if ((_entryPage + 1) * PageSize >= _entryTotalRecords) return;
            _entryPage++;
            LoadStockIn();
        }

        private void pnlstockhistoryprevious_Click(object sender, EventArgs e)
        {
            if (_historyPage <= 0) return;
            _historyPage--;
            LoadStockHistory();
        }

        private void pnlstockhistorynext_Click(object sender, EventArgs e)
        {
            if ((_historyPage + 1) * PageSize >= _historyTotalRecords) return;
            _historyPage++;
            LoadStockHistory();
        }

        private void pnlstockentrygridprevious_Paint(object sender, PaintEventArgs e)
        {
            DrawPagerButton(e.Graphics, pnlstockentrygridprevious.ClientRectangle, false, _entryPage > 0);
        }

        private void pnlstockentrygridnext_Paint(object sender, PaintEventArgs e)
        {
            DrawPagerButton(e.Graphics, pnlstockentrygridnext.ClientRectangle, true, (_entryPage + 1) * PageSize < _entryTotalRecords);
        }

        private void pnlstockhistoryprevious_Paint(object sender, PaintEventArgs e)
        {
            DrawPagerButton(e.Graphics, pnlstockhistoryprevious.ClientRectangle, false, _historyPage > 0);
        }

        private void pnlstockhistorynext_Paint(object sender, PaintEventArgs e)
        {
            DrawPagerButton(e.Graphics, pnlstockhistorynext.ClientRectangle, true, (_historyPage + 1) * PageSize < _historyTotalRecords);
        }

        private void DrawPagerButton(Graphics g, Rectangle bounds, bool next, bool enabled)
        {
            g.Clear(enabled ? Color.FromArgb(20, 158, 132) : Color.Silver);

            using (var brush = new SolidBrush(Color.White))
            {
                Point[] points;

                if (next)
                {
                    points = new[]
                    {
                        new Point(bounds.Left + 8, bounds.Top + 5),
                        new Point(bounds.Left + 17, bounds.Top + bounds.Height / 2),
                        new Point(bounds.Left + 8, bounds.Bottom - 5)
                    };
                }
                else
                {
                    points = new[]
                    {
                        new Point(bounds.Right - 8, bounds.Top + 5),
                        new Point(bounds.Left + 8, bounds.Top + bounds.Height / 2),
                        new Point(bounds.Right - 8, bounds.Bottom - 5)
                    };
                }

                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.FillPolygon(brush, points);
            }
        }

        private bool IsRemoveAction(string action)
        {
            string value = (action ?? string.Empty).Trim();

            if (value.Equals("REMOVE", StringComparison.OrdinalIgnoreCase))
                return true;

            if (value.Equals("Remove from Inventory", StringComparison.OrdinalIgnoreCase))
                return true;

            value = value.ToLowerInvariant();
            return value.Contains("remove") ||
                   value.Contains("deduct") ||
                   value.Contains("decrease") ||
                   value.Contains("less");
        }

        private void TrySilentLog(Exception ex, string location)
        {
            try
            {
                Program.SilentLog(ex, location);
            }
            catch
            {
            }
        }
    }

    public class StockInModel
    {
        public string ID { get; set; }
        public string PCode { get; set; }
        public int Qty { get; set; }
        public string Action { get; set; }
        public string RefNo { get; set; }
        public string SDate { get; set; }
        public string STime { get; set; }
        public string StockInBy { get; set; }
    }

    internal sealed class VendorLookupItem
    {
        public string Id { get; set; }
        public string Vendor { get; set; }
        public string ContactPerson { get; set; }
        public string Address { get; set; }
    }
}