using System;
using System.Data;
using System.Data.SQLite;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosSystem
{
    public partial class frmStoreSetting : Form
    {
        #region Elite UI - Draggable Logic
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
        #endregion

        private readonly string stitle = "PosSystem Elite";
        private readonly string _dbPath = DBConnection.MyConnection();

        public frmStoreSetting()
        {
            InitializeComponent();
            SetupEvents();
        }

        private void SetupEvents()
        {
            // ELITE: Link to the green title bar from your image
            this.panel1.MouseDown += panel1_MouseDown;
            // Link to the Cancel button
            this.button3.Click += (s, e) => this.Dispose();
        }

        private void frmStoreSetting_Load(object sender, EventArgs e)
        {
            // ELITE: Immediately show existing info when form opens
            LoadRecord();
        }

        #region Data Operations (tblStore Profile)

        // ELITE: Fetches the single store profile into the textboxes
        public void LoadRecord()
        {
            try
            {
                using (var cn = new SQLiteConnection(_dbPath))
                {
                    cn.Open();
                    // tblStore is our definitive source [cite: 2026-02-13]
                    using (var cmd = new SQLiteCommand("SELECT * FROM tblStore LIMIT 1", cn))
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            // ELITE: Populating your text fields
                            txtStore.Text = dr["store"].ToString();
                            txtAddress.Text = dr["address"].ToString();
                            txtPhone.Text = dr["phone"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Profile Load Error: " + ex.Message, stitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            // Validation: Don't allow empty store name
            if (string.IsNullOrWhiteSpace(txtStore.Text))
            {
                MessageBox.Show(@"Store name is required for receipts.", stitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (MessageBox.Show(@"Update Store Profile?", stitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                using (var cn = new SQLiteConnection(_dbPath))
                {
                    await cn.OpenAsync();
                    using (var transaction = cn.BeginTransaction())
                    {
                        try
                        {
                            // ELITE: Use DELETE + INSERT to ensure only ONE record ever exists
                            using (var del = new SQLiteCommand("DELETE FROM tblStore", cn, transaction))
                            {
                                await del.ExecuteNonQueryAsync();
                            }

                            string sql = "INSERT INTO tblStore (store, address, phone) VALUES (@store, @address, @phone)";
                            using (var cmd = new SQLiteCommand(sql, cn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@store", txtStore.Text.Trim());
                                cmd.Parameters.AddWithValue("@address", txtAddress.Text.Trim());
                                cmd.Parameters.AddWithValue("@phone", txtPhone.Text.Trim());
                                await cmd.ExecuteNonQueryAsync();
                            }

                            transaction.Commit();
                            MessageBox.Show(@"Store Profile updated successfully!", stitle, MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Refresh current view
                            LoadRecord();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Save Error: " + ex.Message, stitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Junk Designer Stubs (Kept for Designer Stability)
        private void pictureBox2_Click(object sender, EventArgs e) => this.Dispose();
        private void txtStore_TextChanged(object sender, EventArgs e) { }
        private void txtAddress_TextChanged(object sender, EventArgs e) { }
        private void txtPhone_TextChanged(object sender, EventArgs e) { }
        private void button3_Click(object sender, EventArgs e) { }
        private void panelTitle_MouseDown(object sender, MouseEventArgs e) { }
        #endregion
    }
}