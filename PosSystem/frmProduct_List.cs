using System;
using System.Data;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tulpep.NotificationWindow;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices; // Required for Draggable Logic

namespace PosSystem
{
    public partial class frmProduct_List : Form
    {
        // --- DRAGGABLE LOGIC (WinAPI) ---
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        // -------------------------------

        private CancellationTokenSource searchCts; // For debounce

        public frmProduct_List()
        {
            InitializeComponent();

            // 💎 UX Elite: Smooth scrolling & live search flicker-free
            dataGridView1.DoubleBuffered(true);
        }

        #region Product CRUD & Load Logic

        public async Task LoadRecordsAsync()
        {
            dataGridView1.Rows.Clear();
            int i = 0;

            try
            {
                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    await cn.OpenAsync();

                    string query =
                        @"SELECT p.pcode, p.barcode, p.pdesc, b.brand, c.category, p.price, p.reorder
                          FROM TblProduct1 AS p
                          INNER JOIN BrandTbl AS b ON b.id = p.bid
                          INNER JOIN TblCategory AS c ON c.id = p.cid
                          WHERE p.pdesc LIKE @search OR p.barcode LIKE @search";

                    using (var cmd = new SQLiteCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@search", "%" + txtSearch.Text + "%");

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                i++;
                                dataGridView1.Rows.Add(
                                    i,
                                    reader["pcode"].ToString(),
                                    reader["barcode"].ToString(),
                                    reader["pdesc"].ToString(),
                                    reader["brand"].ToString(),
                                    reader["category"].ToString(),
                                    reader["price"].ToString(),
                                    reader["reorder"].ToString()
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "frmProduct_List");
            }
        }

        // ✅ Synchronous wrapper to fix CS1061
        public void LoadRecords()
        {
            LoadRecordsAsync().Wait();
        }

        #endregion

        #region Events

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // ✅ Debounce: cancel previous search if user is still typing
            searchCts?.Cancel();
            searchCts = new CancellationTokenSource();
            var token = searchCts.Token;

            try
            {
                await Task.Delay(200, token); // 200ms delay

                if (!token.IsCancellationRequested)
                    await LoadRecordsAsync();
            }
            catch (TaskCanceledException)
            {
                // Ignore, user is still typing
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            frmProduct frm = new frmProduct(this)
            {
                btnSave = { Enabled = true },
                btnUpdate = { Enabled = false }
            };
            frm.LocalBrand();
            frm.LocalCategory();
            frm.ShowDialog();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private async void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            string pcode = dataGridView1.Rows[e.RowIndex].Cells["pcode"].Value.ToString();

            if (colName == "Edit")
            {
                frmProduct frm = new frmProduct(this)
                {
                    btnSave = { Enabled = false },
                    btnUpdate = { Enabled = true },
                    TxtPcode = { Text = pcode },
                    txtBarcode = { Text = dataGridView1.Rows[e.RowIndex].Cells["barcode"].Value.ToString() },
                    txtPdesc = { Text = dataGridView1.Rows[e.RowIndex].Cells["pdesc"].Value.ToString() },
                    comboBox1 = { Text = dataGridView1.Rows[e.RowIndex].Cells["brand"].Value.ToString() },
                    comboBox2 = { Text = dataGridView1.Rows[e.RowIndex].Cells["category"].Value.ToString() },
                    txtPrice = { Text = dataGridView1.Rows[e.RowIndex].Cells["price"].Value.ToString() },
                    txtReOrder = { Text = dataGridView1.Rows[e.RowIndex].Cells["reorder"].Value.ToString() }
                };

                frm.ShowDialog();
            }
            else if (colName == "Delete")
            {
                if (MessageBox.Show("Are you sure you want to delete this product?", "Delete Product",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                        {
                            await cn.OpenAsync();
                            using (var cmd = new SQLiteCommand("DELETE FROM TblProduct1 WHERE pcode = @pcode", cn))
                            {
                                cmd.Parameters.AddWithValue("@pcode", pcode);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        MessageBox.Show("Item Removed Successfully", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadRecordsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // ✅ Fixed panel paint CS1061
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // intentionally left empty
        }

        #endregion
    }

    #region DataGridView Helper

    public static class DataGridViewExtensions
    {
        public static void DoubleBuffered(this DataGridView dgv, bool setting)
        {
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.SetProperty,
                null, dgv, new object[] { setting });
        }
    }

    #endregion
}