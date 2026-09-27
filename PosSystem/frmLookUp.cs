using System;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PosSystem
{
    public partial class frmLookUp : Form
    {
        #region WINAPI FOR DRAGGING
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

        public Action<string, double, string, int> OnProductSelected;
        private CancellationTokenSource _searchCts;

        public string SelectedPCode { get; private set; }
        public string SelectedDescription { get; private set; }
        public decimal SelectedPrice { get; private set; }
        public int SelectedStock { get; private set; }

        public frmLookUp()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.Shown += frmLookUp_Shown;
        }

        public frmLookUp(Action<string, double, string, int> callback) : this()
        {
            OnProductSelected = callback;
        }

        private async void frmLookUp_Shown(object sender, EventArgs e)
        {
            await LoadRecordsAsync(string.Empty, CancellationToken.None);
            txtSearch.Focus();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async Task LoadRecordsAsync(string search, CancellationToken token)
        {
            try
            {
                if (IsDisposed) return;

                dataGridView1.Rows.Clear();

                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    await cn.OpenAsync(token);

                    string query = @"
                        SELECT 
                            p.pcode,
                            p.barcode,
                            p.pdesc,
                            IFNULL(b.brand, '') AS brand,
                            IFNULL(c.category, '') AS category,
                            p.price,
                            p.qty
                        FROM TblProduct1 p
                        LEFT JOIN BrandTbl b ON b.id = p.bid
                        LEFT JOIN TblCategory c ON c.id = p.cid
                        WHERE p.pdesc LIKE @search
                           OR p.barcode LIKE @search
                           OR p.pcode LIKE @search
                        ORDER BY p.pdesc ASC";

                    using (var cm = new SQLiteCommand(query, cn))
                    {
                        cm.Parameters.AddWithValue("@search", "%" + (search ?? string.Empty).Trim() + "%");

                        using (var dr = await cm.ExecuteReaderAsync(token))
                        {
                            int i = 0;

                            while (await dr.ReadAsync(token))
                            {
                                i++;
                                dataGridView1.Rows.Add(
                                    i,
                                    dr["pcode"]?.ToString(),
                                    dr["barcode"]?.ToString(),
                                    dr["pdesc"]?.ToString(),
                                    dr["brand"]?.ToString(),
                                    dr["category"]?.ToString(),
                                    dr["price"]?.ToString(),
                                    dr["qty"]?.ToString()
                                );
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "frmLookUp_LoadRecordsAsync");
            }
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _searchCts?.Cancel();
                _searchCts?.Dispose();
                _searchCts = new CancellationTokenSource();

                await Task.Delay(300, _searchCts.Token);
                await LoadRecordsAsync(txtSearch.Text.Trim(), _searchCts.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "frmLookUp_txtSearch_TextChanged");
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (colName == "colSelect" || colName == "Select")
            {
                SelectProduct(e.RowIndex);
            }
        }

        private void SelectProduct(int rowIndex)
        {
            try
            {
                if (rowIndex < 0 || rowIndex >= dataGridView1.Rows.Count)
                    return;

                SelectedPCode = dataGridView1.Rows[rowIndex].Cells[1].Value?.ToString();
                SelectedDescription = dataGridView1.Rows[rowIndex].Cells[3].Value?.ToString();

                decimal.TryParse(dataGridView1.Rows[rowIndex].Cells[6].Value?.ToString(), out decimal price);
                SelectedPrice = price;

                int.TryParse(dataGridView1.Rows[rowIndex].Cells[7].Value?.ToString(), out int qtyHand);
                SelectedStock = qtyHand;

                OnProductSelected?.Invoke(
                    SelectedPCode,
                    (double)SelectedPrice,
                    DateTime.Now.ToString("yyyyMMddHHmmss"),
                    SelectedStock);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Selection Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmLookUp_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();

            if (e.KeyCode == Keys.Enter && dataGridView1.CurrentRow != null)
            {
                e.Handled = true;
                SelectProduct(dataGridView1.CurrentRow.Index);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                SelectProduct(dataGridView1.CurrentRow.Index);
            }
        }

        private void btnOK_Click_1(object sender, EventArgs e)
        {
            btnOK_Click(sender, e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                _searchCts?.Cancel();
                _searchCts?.Dispose();
                _searchCts = null;
            }
            catch
            {
            }

            base.OnFormClosing(e);
        }

        #region Unused Stubs
        private void txtSearch_Click(object sender, EventArgs e) { }
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void frmLookUp_KeyPress(object sender, KeyPressEventArgs e) { }
        #endregion
    }
}