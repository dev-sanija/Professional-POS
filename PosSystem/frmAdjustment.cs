using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Drawing;

namespace PosSystem
{
    public partial class frmAdjustment : Form
    {
        #region Private Members & Win32 Elite UI
        private Form1 _f;
        public string suser;
        private CancellationTokenSource searchCTS;

        // Pagination Members
        private int currentPage = 0;
        private const int pageSize = 50;
        private int totalRecords = 0;

        [DllImport("user32.dll")]
        private static extern bool LockWindowUpdate(IntPtr hWndLock);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        // This is actually the connection string from DBConnection.MyConnection()
        private readonly string _dbPath = DBConnection.MyConnection();
        #endregion

        public frmAdjustment()
        {
            InitializeComponent();
            SetupEliteUI();
            ConfigureGridSizing();
            SetupPaginationButtons();
        }

        public frmAdjustment(Form1 frm) : this()
        {
            _f = frm;
            suser = frm?._user ?? "Admin";
        }

        private void SetupEliteUI()
        {
            txtSearch.TextChanged += TxtSearch_TextChanged;
            panel1.MouseDown += panel1_MouseDown;

            typeof(DataGridView).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(dataGridView1, true, null);
        }

        private void ConfigureGridSizing()
        {
            if (dataGridView1 == null) return;

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView1.ColumnHeadersHeight = 30;

            dataGridView1.RowTemplate.Height = 26;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;

            if (dataGridView1.Columns["Column1"] != null)
            {
                dataGridView1.Columns["Column1"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridView1.Columns["Column1"].Width = 35;
                dataGridView1.Columns["Column1"].Resizable = DataGridViewTriState.False;
            }

            if (dataGridView1.Columns["Column2"] != null)
            {
                dataGridView1.Columns["Column2"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns["Column2"].Resizable = DataGridViewTriState.False;
            }

            if (dataGridView1.Columns["Column3"] != null)
            {
                dataGridView1.Columns["Column3"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns["Column3"].Resizable = DataGridViewTriState.False;
            }

            if (dataGridView1.Columns["Column4"] != null)
            {
                dataGridView1.Columns["Column4"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView1.Columns["Column4"].MinimumWidth = 150;
                dataGridView1.Columns["Column4"].Resizable = DataGridViewTriState.False;
            }

            if (dataGridView1.Columns["Column5"] != null)
            {
                dataGridView1.Columns["Column5"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns["Column5"].Resizable = DataGridViewTriState.False;
            }

            if (dataGridView1.Columns["Column6"] != null)
            {
                dataGridView1.Columns["Column6"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns["Column6"].Resizable = DataGridViewTriState.False;
            }

            if (dataGridView1.Columns["Column7"] != null)
            {
                dataGridView1.Columns["Column7"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridView1.Columns["Column7"].Width = 70;
                dataGridView1.Columns["Column7"].Resizable = DataGridViewTriState.False;
                dataGridView1.Columns["Column7"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (dataGridView1.Columns["Colum8"] != null)
            {
                dataGridView1.Columns["Colum8"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridView1.Columns["Colum8"].Width = 85;
                dataGridView1.Columns["Colum8"].Resizable = DataGridViewTriState.False;
                dataGridView1.Columns["Colum8"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            if (dataGridView1.Columns["colSelect"] != null)
            {
                dataGridView1.Columns["colSelect"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridView1.Columns["colSelect"].Width = 85;
                dataGridView1.Columns["colSelect"].Resizable = DataGridViewTriState.False;
            }
        }

        private void SetupPaginationButtons()
        {
            btnPrePage.Size = new Size(20, 20);
            btnNextPage.Size = new Size(20, 20);
            btnPrePage.Cursor = Cursors.Hand;
            btnNextPage.Cursor = Cursors.Hand;

            btnPrePage.Click += async (s, e) =>
            {
                if (currentPage > 0)
                {
                    currentPage--;
                    await LoadRecordsAsync();
                }
            };

            btnNextPage.Click += async (s, e) =>
            {
                if ((currentPage + 1) * pageSize < totalRecords)
                {
                    currentPage++;
                    await LoadRecordsAsync();
                }
            };

            btnPrePage.Paint += (s, e) => DrawArrow(e.Graphics, false, currentPage > 0);
            btnNextPage.Paint += (s, e) => DrawArrow(e.Graphics, true, (currentPage + 1) * pageSize < totalRecords);
        }

        private void DrawArrow(Graphics g, bool right, bool enabled)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (Brush b = new SolidBrush(enabled ? Color.FromArgb(20, 158, 132) : Color.LightGray))
            {
                Point[] points = right
                    ? new Point[] { new Point(6, 4), new Point(14, 10), new Point(6, 16) }
                    : new Point[] { new Point(14, 4), new Point(6, 10), new Point(14, 16) };

                g.FillPolygon(b, points);
            }
        }

        private async void frmAdjustment_Load(object sender, EventArgs e)
        {
            txtUser.Text = string.IsNullOrWhiteSpace(suser) ? (_f?._user ?? "Admin") : suser;
            referenceNo();
            await LoadRecordsAsync(CancellationToken.None);
        }

        public void referenceNo()
        {
            txtRef.Text = $"ADJ-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper()}";
        }

        #region High-Performance Pagination & Auto-Search
        private async void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (searchCTS != null)
                {
                    searchCTS.Cancel();
                    searchCTS.Dispose();
                }

                searchCTS = new CancellationTokenSource();
                CancellationToken token = searchCTS.Token;

                await Task.Delay(400, token);
                currentPage = 0;
                await LoadRecordsAsync(token);
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "frmAdjustment_TxtSearch_TextChanged");
            }
        }

        public async Task LoadRecordsAsync(CancellationToken token = default(CancellationToken))
        {
            if (IsDisposed || !IsHandleCreated) return;

            try
            {
                if (dataGridView1 != null && dataGridView1.IsHandleCreated)
                    LockWindowUpdate(dataGridView1.Handle);

                using (var cn = new SQLiteConnection(_dbPath))
                {
                    await cn.OpenAsync(token);

                    string searchText = $"%{txtSearch.Text.Trim()}%";
                    int offset = currentPage * pageSize;

                    using (var cmdCount = new SQLiteCommand(@"
    SELECT COUNT(*)
    FROM TblProduct1 p
    LEFT JOIN BrandTbl b ON p.bid = b.id
    LEFT JOIN TblCategory c ON p.cid = c.id
    WHERE p.pcode LIKE @search
       OR IFNULL(p.barcode,'') LIKE @search
       OR p.pdesc LIKE @search
       OR IFNULL(b.brand,'') LIKE @search
       OR IFNULL(c.category,'') LIKE @search", cn))
                    {
                        cmdCount.Parameters.AddWithValue("@search", searchText);
                        object result = await cmdCount.ExecuteScalarAsync(token);
                        totalRecords = result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
                    }

                    string query = @"
    SELECT 
        p.pcode,
        p.barcode,
        p.pdesc,
        IFNULL(b.brand,'') as brand,
        IFNULL(c.category,'') as category,
        p.price,
        p.qty
    FROM TblProduct1 p
    LEFT JOIN BrandTbl b ON p.bid = b.id
    LEFT JOIN TblCategory c ON p.cid = c.id
    WHERE p.pcode LIKE @search
       OR IFNULL(p.barcode,'') LIKE @search
       OR p.pdesc LIKE @search
       OR IFNULL(b.brand,'') LIKE @search
       OR IFNULL(c.category,'') LIKE @search
    ORDER BY p.pdesc
    LIMIT @limit OFFSET @offset";

                    using (var cmd = new SQLiteCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@search", searchText);
                        cmd.Parameters.AddWithValue("@limit", pageSize);
                        cmd.Parameters.AddWithValue("@offset", offset);

                        using (var reader = await cmd.ExecuteReaderAsync(token))
                        {
                            dataGridView1.Rows.Clear();
                            int i = offset;

                            while (await reader.ReadAsync(token))
                            {
                                dataGridView1.Rows.Add(
                                    ++i,
                                    reader["pcode"],
                                    reader["barcode"],
                                    reader["pdesc"],
                                    reader["brand"],
                                    reader["category"],
                                    reader["price"],
                                    reader["qty"]
                                );
                            }
                        }
                    }
                }
                ConfigureGridSizing();
                btnPrePage.Invalidate();
                btnNextPage.Invalidate();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "frmAdjustment_LoadRecordsAsync");
            }
            finally
            {
                try
                {
                    LockWindowUpdate(IntPtr.Zero);
                }
                catch
                {
                }
            }
        }
        #endregion

        #region Transaction-Safe Save Logic
        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cbCommands.Text))
            {
                MessageBox.Show(@"Select Action Type.", @"POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPcode.Text))
            {
                MessageBox.Show(@"Select a product first.", @"POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int adjQty;
            if (!int.TryParse(txtQty.Text.Trim(), out adjQty) || adjQty <= 0)
            {
                MessageBox.Show(@"Enter a valid positive quantity.", @"POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isRemoveAction = IsRemoveAction(cbCommands.Text);

            btnSave.Enabled = false;
            UseWaitCursor = true;

            try
            {
                using (var cn = new SQLiteConnection(_dbPath))
                {
                    await cn.OpenAsync();

                    using (var tran = cn.BeginTransaction())
                    {
                        int currentQty;

                        using (var get = new SQLiteCommand("SELECT qty FROM TblProduct1 WHERE pcode=@p", cn, tran))
                        {
                            get.Parameters.AddWithValue("@p", txtPcode.Text.Trim());
                            object result = await get.ExecuteScalarAsync();

                            if (result == null || result == DBNull.Value)
                            {
                                try { tran.Rollback(); } catch { }
                                MessageBox.Show(@"Product not found.", @"POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            currentQty = Convert.ToInt32(result);
                        }

                        int newQty = isRemoveAction ? (currentQty - adjQty) : (currentQty + adjQty);

                        if (newQty < 0)
                        {
                            try { tran.Rollback(); } catch { }
                            MessageBox.Show(@"Action results in negative stock.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            return;
                        }

                        using (var upd = new SQLiteCommand("UPDATE TblProduct1 SET qty=@q WHERE pcode=@p", cn, tran))
                        {
                            upd.Parameters.AddWithValue("@q", newQty);
                            upd.Parameters.AddWithValue("@p", txtPcode.Text.Trim());
                            await upd.ExecuteNonQueryAsync();
                        }

                        int recordQty = isRemoveAction ? -adjQty : adjQty;
                        bool historySaved = await TryInsertAdjustmentHistoryAsync(cn, tran, recordQty);

                        tran.Commit();

                        MessageBox.Show(
                            historySaved ? @"Adjustment saved successfully." : @"Adjustment applied successfully.",
                            @"POS Elite",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }

                ClearFields();
                await LoadRecordsAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "frmAdjustment_btnSave_Click");
                MessageBox.Show(@"Error: " + ex.Message, @"POS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
                btnSave.Enabled = true;
            }
        }

        private bool IsRemoveAction(string actionText)
        {
            string value = (actionText ?? string.Empty).Trim();

            if (value.Equals("Remove from Inventory", StringComparison.OrdinalIgnoreCase))
                return true;

            string lowered = value.ToLowerInvariant();
            return lowered.Contains("remove") ||
                   lowered.Contains("deduct") ||
                   lowered.Contains("decrease") ||
                   lowered.Contains("less");
        }

        private async Task<bool> TryInsertAdjustmentHistoryAsync(SQLiteConnection cn, SQLiteTransaction tran, int recordQty)
        {
            try
            {
                if (!await TableExistsAsync(cn, tran, "tblAdjustment"))
                    return false;

                using (var ins = new SQLiteCommand(@"
                    INSERT INTO tblAdjustment
                    (referenceno, pcode, qty, action, remarks, sdate, [user])
                    VALUES (@ref, @p, @qty, @act, @rem, @date, @u)", cn, tran))
                {
                    ins.Parameters.AddWithValue("@ref", txtRef.Text.Trim());
                    ins.Parameters.AddWithValue("@p", txtPcode.Text.Trim());
                    ins.Parameters.AddWithValue("@qty", recordQty);
                    ins.Parameters.AddWithValue("@act", cbCommands.Text.Trim());
                    ins.Parameters.AddWithValue("@rem", txtRemarks.Text.Trim());
                    ins.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    ins.Parameters.AddWithValue("@u", string.IsNullOrWhiteSpace(suser) ? txtUser.Text.Trim() : suser);

                    await ins.ExecuteNonQueryAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "frmAdjustment_TryInsertAdjustmentHistoryAsync");
                return false;
            }
        }

        private async Task<bool> TableExistsAsync(SQLiteConnection cn, SQLiteTransaction tran, string tableName)
        {
            using (var cmd = new SQLiteCommand(
                "SELECT name FROM sqlite_master WHERE type='table' AND name=@name LIMIT 1;", cn, tran))
            {
                cmd.Parameters.AddWithValue("@name", tableName);
                object result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value;
            }
        }
        #endregion

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dataGridView1.Columns[e.ColumnIndex].Name == "colSelect")
            {
                txtPcode.Text = dataGridView1.Rows[e.RowIndex].Cells[1].Value?.ToString();
                txtdesc.Text = dataGridView1.Rows[e.RowIndex].Cells[3].Value?.ToString();
                txtQty.Focus();
                txtQty.SelectAll();
            }
        }

        private string GetSelectedProductCode()
        {
            if (!string.IsNullOrWhiteSpace(txtPcode.Text))
                return txtPcode.Text.Trim();

            if (dataGridView1.CurrentRow != null && !dataGridView1.CurrentRow.IsNewRow)
                return dataGridView1.CurrentRow.Cells[1].Value?.ToString()?.Trim();

            if (dataGridView1.SelectedRows.Count > 0 && !dataGridView1.SelectedRows[0].IsNewRow)
                return dataGridView1.SelectedRows[0].Cells[1].Value?.ToString()?.Trim();

            return string.Empty;
        }

        private void ClearFields()
        {
            referenceNo();
            txtQty.Clear();
            txtPcode.Clear();
            txtdesc.Clear();
            txtRemarks.Clear();
            cbCommands.SelectedIndex = -1;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                if (searchCTS != null)
                {
                    searchCTS.Cancel();
                    searchCTS.Dispose();
                    searchCTS = null;
                }
            }
            catch
            {
            }

            base.OnFormClosed(e);
        }

        #region Junk Designer Stubs
        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e) { }
        private void txtUser_TextChanged(object sender, EventArgs e) { }
        private void txtRemarks_TextChanged(object sender, EventArgs e) { }
        private void cbCommands_SelectedIndexChanged(object sender, EventArgs e) { }
        private void txtRef_TextChanged(object sender, EventArgs e) { }
        private void txtPcode_TextChanged(object sender, EventArgs e) { }
        private void txtdesc_TextChanged(object sender, EventArgs e) { }
        private void txtQty_TextChanged(object sender, EventArgs e) { }
        private void txtSearch_Click(object sender, EventArgs e) { }
        #endregion

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            string pcode = GetSelectedProductCode();
            if (string.IsNullOrWhiteSpace(pcode))
            {
                MessageBox.Show("Please select a product row first.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnUpdate.Enabled = false;
            UseWaitCursor = true;

            try
            {
                frmProduct frm = new frmProduct(this);
                bool loaded = await frm.LoadProductForUpdateAsync(pcode);

                if (!loaded)
                {
                    frm.Dispose();
                    return;
                }

                frm.ShowDialog();
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "frmAdjustment_btnUpdate_Click");
                MessageBox.Show("Unable to open product update form. " + ex.Message, "POS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
                btnUpdate.Enabled = true;
            }
        }
    }
}