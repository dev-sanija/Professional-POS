using System;
using System.Data.SQLite;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PosSystem
{
    public partial class frmSearchProductStokin : Form
    {
        #region Win32 API for Draggable Form
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        #endregion

        private readonly string stitle = "POS System";
        private readonly frmStockin _stockInForm;
        private readonly Timer _searchTimer;

        public frmSearchProductStokin(frmStockin flist)
        {
            InitializeComponent();
            _stockInForm = flist;

            panel1.MouseDown += panel1_MouseDown;
            pictureBox2.Click += pictureBox2_Click;
            this.Shown += frmSearchProductStokin_Shown;

            _searchTimer = new Timer();
            _searchTimer.Interval = 250;
            _searchTimer.Tick += SearchTimer_Tick;
        }

        private void frmSearchProductStokin_Shown(object sender, EventArgs e)
        {
            txtSearch.Clear();
            LoadProduct();
            txtSearch.Focus();
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            LoadProduct();
        }

        private void frmSearchProductStokin_Load(object sender, EventArgs e)
        {
        }

        private void pictureBox1_Click(object sender, EventArgs e) => this.Dispose();
        private void pictureBox2_Click(object sender, EventArgs e) => this.Dispose();

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (colName == "colSelect" || colName == "Select")
            {
                AddProductToStockIn(e.RowIndex);
            }
        }

        private void AddProductToStockIn(int rowIndex)
        {
            if (dataGridView1.Rows.Count <= rowIndex) return;

            string pcode = dataGridView1.Rows[rowIndex].Cells["Column2"].Value?.ToString();
            string pdesc = dataGridView1.Rows[rowIndex].Cells["Column4"].Value?.ToString();

            if (string.IsNullOrWhiteSpace(pcode))
            {
                MessageBox.Show("Invalid product selected.", stitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_stockInForm.txtRefNo.Text))
            {
                MessageBox.Show("Please generate a Reference No first!", stitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int qty;
            string action;
            string remarks;

            if (!TryGetMovementDetails(pcode, pdesc, out qty, out action, out remarks))
                return;

            try
            {
                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    cn.Open();

                    using (var transaction = cn.BeginTransaction())
                    {
                        string vendorIdText = GetPlainVendorId(_stockInForm.lblVendorID.Text);

                        string insertSql = @"
                            INSERT INTO tblStockIn
                            (refno, pcode, qty, sdate, stime, stockinby, vendorid, action, remarks, status)
                            VALUES
                            (@ref, @pcode, @qty, @sdate, @stime, @by, @vrid, @action, @remarks, 'Pending');";

                        using (var cmd = new SQLiteCommand(insertSql, cn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ref", _stockInForm.txtRefNo.Text.Trim());
                            cmd.Parameters.AddWithValue("@pcode", pcode);
                            cmd.Parameters.AddWithValue("@qty", qty);

                            // keep date + time from stock entry form
                            cmd.Parameters.AddWithValue("@sdate", _stockInForm.dt1.Value.ToString("yyyy-MM-dd"));
                            cmd.Parameters.AddWithValue("@stime", _stockInForm.dt1.Value.ToString("HH:mm"));

                            // allow incomplete record details for now
                            if (string.IsNullOrWhiteSpace(_stockInForm.txtBy.Text))
                                cmd.Parameters.AddWithValue("@by", DBNull.Value);
                            else
                                cmd.Parameters.AddWithValue("@by", _stockInForm.txtBy.Text.Trim());

                            if (string.IsNullOrWhiteSpace(vendorIdText) || vendorIdText == "0")
                                cmd.Parameters.AddWithValue("@vrid", DBNull.Value);
                            else
                                cmd.Parameters.AddWithValue("@vrid", vendorIdText);

                            cmd.Parameters.AddWithValue("@action", action);
                            cmd.Parameters.AddWithValue("@remarks", remarks ?? string.Empty);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }

                _stockInForm.LoadStockIn();

                txtSearch.Clear();
                LoadProduct();
                txtSearch.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error: " + ex.Message, stitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool TryGetMovementDetails(string pcode, string pdesc, out int qty, out string action, out string remarks)
        {
            qty = 0;
            action = "ADD";
            remarks = string.Empty;

            using (Form dlg = new Form())
            using (Label lblTitle = new Label())
            using (Label lblPcode = new Label())
            using (Label lblDesc = new Label())
            using (Label lblQty = new Label())
            using (Label lblAction = new Label())
            using (Label lblRemarks = new Label())
            using (NumericUpDown numQty = new NumericUpDown())
            using (ComboBox cbAction = new ComboBox())
            using (TextBox txtRemarks = new TextBox())
            using (Button btnOk = new Button())
            using (Button btnCancel = new Button())
            {
                dlg.Text = "Add Movement Record";
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.ShowInTaskbar = false;
                dlg.ClientSize = new Size(430, 250);
                dlg.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

                lblTitle.Text = "Movement Details";
                lblTitle.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
                lblTitle.AutoSize = true;
                lblTitle.Location = new Point(15, 15);

                lblPcode.Text = "PCODE : " + (pcode ?? string.Empty);
                lblPcode.AutoSize = true;
                lblPcode.Location = new Point(18, 50);

                lblDesc.Text = "DESCRIPTION : " + (pdesc ?? string.Empty);
                lblDesc.AutoEllipsis = true;
                lblDesc.Size = new Size(390, 20);
                lblDesc.Location = new Point(18, 72);

                lblQty.Text = "Qty";
                lblQty.AutoSize = true;
                lblQty.Location = new Point(18, 108);

                numQty.Minimum = 1;
                numQty.Maximum = 1000000;
                numQty.Value = 1;
                numQty.Location = new Point(110, 104);
                numQty.Size = new Size(130, 23);

                lblAction.Text = "Action";
                lblAction.AutoSize = true;
                lblAction.Location = new Point(18, 140);

                cbAction.DropDownStyle = ComboBoxStyle.DropDownList;
                cbAction.Items.AddRange(new object[]
                {
                    "ADD",
                    "REMOVE"
                });
                cbAction.SelectedIndex = 0;
                cbAction.Location = new Point(110, 136);
                cbAction.Size = new Size(130, 23);

                lblRemarks.Text = "Remarks";
                lblRemarks.AutoSize = true;
                lblRemarks.Location = new Point(18, 172);

                txtRemarks.Location = new Point(110, 168);
                txtRemarks.Size = new Size(298, 23);

                btnOk.Text = "OK";
                btnOk.DialogResult = DialogResult.OK;
                btnOk.Location = new Point(252, 208);
                btnOk.Size = new Size(75, 28);

                btnCancel.Text = "Cancel";
                btnCancel.DialogResult = DialogResult.Cancel;
                btnCancel.Location = new Point(333, 208);
                btnCancel.Size = new Size(75, 28);

                dlg.Controls.Add(lblTitle);
                dlg.Controls.Add(lblPcode);
                dlg.Controls.Add(lblDesc);
                dlg.Controls.Add(lblQty);
                dlg.Controls.Add(numQty);
                dlg.Controls.Add(lblAction);
                dlg.Controls.Add(cbAction);
                dlg.Controls.Add(lblRemarks);
                dlg.Controls.Add(txtRemarks);
                dlg.Controls.Add(btnOk);
                dlg.Controls.Add(btnCancel);

                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return false;

                qty = Convert.ToInt32(numQty.Value);
                action = cbAction.Text.Trim();
                remarks = txtRemarks.Text.Trim();

                return true;
            }
        }

        private string GetPlainVendorId(string rawLabelText)
        {
            string value = rawLabelText ?? string.Empty;
            const string prefix = "Vendor Id :";

            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                value = value.Substring(prefix.Length).Trim();

            return value.Trim();
        }

        public void LoadProduct()
        {
            try
            {
                string searchVal = string.IsNullOrWhiteSpace(txtSearch.Text)
                    ? "%"
                    : "%" + txtSearch.Text.Trim() + "%";

                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    cn.Open();

                    string sql = @"
WITH LastMove AS
(
    SELECT
        x.pcode,
        MAX(x.moved_at) AS moved_at
    FROM
    (
        SELECT
            s.pcode,
            TRIM(IFNULL(s.sdate,'') || ' ' || IFNULL(s.stime,'')) AS moved_at
        FROM tblStockIn s
        WHERE UPPER(TRIM(IFNULL(s.status,''))) = 'DONE'

        UNION ALL

        SELECT
            a.pcode,
            IFNULL(a.sdate,'') AS moved_at
        FROM tblAdjustment a
    ) x
    WHERE TRIM(IFNULL(x.pcode,'')) <> ''
    GROUP BY x.pcode
)
SELECT
    p.pcode,
    p.pdesc,
    p.qty,
    IFNULL(b.brand, '') AS brand,
    IFNULL(c.category, '') AS category,
    CASE
        WHEN LENGTH(IFNULL(m.moved_at,'')) >= 16 THEN SUBSTR(m.moved_at, 12, 5)
        ELSE ''
    END AS last_time
FROM TblProduct1 p
LEFT JOIN BrandTbl b ON p.bid = b.id
LEFT JOIN TblCategory c ON p.cid = c.id
LEFT JOIN LastMove m ON m.pcode = p.pcode
WHERE p.isactive = 1
  AND
  (
      p.pcode LIKE @search
      OR p.pdesc LIKE @search
      OR IFNULL(b.brand,'') LIKE @search
      OR IFNULL(c.category,'') LIKE @search
  )
ORDER BY p.pdesc
LIMIT 100;";

                    using (var cmd = new SQLiteCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("@search", searchVal);

                        using (var dr = cmd.ExecuteReader())
                        {
                            dataGridView1.Rows.Clear();
                            int i = 0;

                            while (dr.Read())
                            {
                                i++;
                                dataGridView1.Rows.Add(
                                    i,
                                    dr["pcode"].ToString(),
                                    dr["pdesc"].ToString(),
                                    dr["qty"].ToString(),
                                    dr["brand"].ToString(),
                                    dr["category"].ToString(),
                                    dr["last_time"].ToString()
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Load Error: " + ex.Message, stitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (dataGridView1.Rows.Count > 0)
                {
                    AddProductToStockIn(0);
                }

                e.SuppressKeyPress = true;
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void txtSearch_Click(object sender, EventArgs e) { }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                if (_searchTimer != null)
                {
                    _searchTimer.Stop();
                    _searchTimer.Dispose();
                }
            }
            catch
            {
            }

            base.OnFormClosed(e);
        }
    }
}