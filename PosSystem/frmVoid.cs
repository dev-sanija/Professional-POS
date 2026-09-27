using System;
using System.Data.SQLite;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;

namespace PosSystem
{
    public partial class frmVoid : Form
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

        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        // ===== Public properties set by frmCancelDetails =====
        public string CancelAction { get; set; }
        public int CancelQty { get; set; }
        public string CancelReason { get; set; }
        public string ProductCode { get; set; }
        public string CartId { get; set; }
        public decimal Price { get; set; }
        public string CancelledBy { get; set; }
        public frmSoldItems SoldItemForm { get; set; }

        // This matches the call from frmCancelDetails: voidForm.VoidBy = ...
        public string VoidBy { get; set; }

        public bool Approved { get; private set; } = false;
        public bool SaveSuccess { get; private set; } = false;
        private bool _isProcessing = false;

        public frmVoid()
        {
            InitializeComponent();
        }

        private void frmVoid_Load(object sender, EventArgs e) { }

        private void pictureBox2_Click(object sender, EventArgs e) => this.Dispose();

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (_isProcessing) return;
            if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPass.Text))
            {
                MessageBox.Show("Please enter administrator credentials.", "Void Authorization", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _isProcessing = true;
            btnSave.Enabled = false;

            try
            {
                using (SQLiteConnection cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    cn.Open();
                    using (SQLiteTransaction tx = cn.BeginTransaction())
                    {
                        // 1. Authenticate
                        string authUser = AuthenticateUser(cn, tx);
                        if (authUser == null)
                        {
                            tx.Rollback();
                            MessageBox.Show("Invalid credentials or unauthorized user!", "Access Denied",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            txtPass.Clear();
                            txtPass.Focus();
                            _isProcessing = false;
                            btnSave.Enabled = true;
                            return;
                        }

                        Approved = true;

                        // 2. Database Updates
                        SaveCancelOrder(cn, tx, authUser);
                        if (CancelAction?.Trim().ToUpper() == "YES")
                        {
                            UpdateProductQty(cn, tx, ProductCode, CancelQty);
                        }
                        UpdateCartQty(cn, tx, CartId, CancelQty);

                        tx.Commit();
                    }
                }

                SaveSuccess = true;
                MessageBox.Show("Transaction has been successfully voided.", "Void Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (SoldItemForm != null)
                    await SoldItemForm.LoadSoldItemsAsync();

                this.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Critical Database Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isProcessing = false;
                btnSave.Enabled = true;
            }
        }

        private string AuthenticateUser(SQLiteConnection cn, SQLiteTransaction tx)
        {
            // 1. Get the user's salt first
            string salt = "";
            using (SQLiteCommand cmSalt = new SQLiteCommand("SELECT salt FROM tblUser WHERE UPPER(TRIM(username)) = UPPER(TRIM(@u))", cn, tx))
            {
                cmSalt.Parameters.AddWithValue("@u", txtUser.Text.Trim());
                object s = cmSalt.ExecuteScalar();
                if (s == null || s == DBNull.Value) return null;
                salt = s.ToString();
            }

            // 2. Hash the input using the found salt
            string hashedPass = DBConnection.GetHash(txtPass.Text.Trim(), salt);

            // 3. Authenticate with the role check
            string sql = @"SELECT username FROM tblUser 
                   WHERE UPPER(TRIM(username)) = UPPER(TRIM(@u)) 
                   AND password = @p 
                   AND (role = 'Administrator' OR role = 'System Administrator')
                   AND (isactive = 1 OR UPPER(isactive) = 'TRUE')";

            using (SQLiteCommand cm = new SQLiteCommand(sql, cn, tx))
            {
                cm.Parameters.AddWithValue("@u", txtUser.Text.Trim());
                cm.Parameters.AddWithValue("@p", hashedPass);

                object result = cm.ExecuteScalar();
                if (result == null) return null;

                if (!result.ToString().Equals(this.VoidBy, StringComparison.OrdinalIgnoreCase))
                    return null;

                return result.ToString();
            }
        }

        private void SaveCancelOrder(SQLiteConnection cn, SQLiteTransaction tx, string adminUser)
        {
            using (SQLiteCommand cm = new SQLiteCommand(
                @"INSERT INTO tblCancel 
                  (transno, pcode, price, qty, sdate, voidby, cancelledby, reason, action) 
                  VALUES 
                  (@transno, @pcode, @price, @qty, @sdate, @voidby, @cancelledby, @reason, @action)",
                cn, tx))
            {
                cm.Parameters.AddWithValue("@transno", CartId);
                cm.Parameters.AddWithValue("@pcode", ProductCode);
                cm.Parameters.AddWithValue("@price", Price);
                cm.Parameters.AddWithValue("@qty", CancelQty);
                cm.Parameters.AddWithValue("@sdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cm.Parameters.AddWithValue("@voidby", adminUser);
                cm.Parameters.AddWithValue("@cancelledby", CancelledBy);
                cm.Parameters.AddWithValue("@reason", CancelReason);
                cm.Parameters.AddWithValue("@action", CancelAction);
                cm.ExecuteNonQuery();
            }
        }

        private void UpdateProductQty(SQLiteConnection cn, SQLiteTransaction tx, string pcode, int qty)
        {
            using (SQLiteCommand cm = new SQLiteCommand(
                "UPDATE TblProduct1 SET qty = qty + @qty WHERE pcode = @pcode", cn, tx))
            {
                cm.Parameters.AddWithValue("@qty", qty);
                cm.Parameters.AddWithValue("@pcode", pcode);
                cm.ExecuteNonQuery();
            }
        }

        private void UpdateCartQty(SQLiteConnection cn, SQLiteTransaction tx, string cartId, int qtyToVoid)
        {
            using (SQLiteCommand cm = new SQLiteCommand(
                "UPDATE tblCart1 SET qty = qty - @qty, total = (qty - @qty) * price WHERE id = @id",
                cn, tx))
            {
                cm.Parameters.AddWithValue("@qty", qtyToVoid);
                cm.Parameters.AddWithValue("@id", cartId);
                cm.ExecuteNonQuery();
            }

            using (SQLiteCommand cm = new SQLiteCommand(
                "DELETE FROM tblCart1 WHERE id = @id AND qty <= 0",
                cn, tx))
            {
                cm.Parameters.AddWithValue("@id", cartId);
                cm.ExecuteNonQuery();
            }
        }

        #region Junk Designer Stubs
        private void txtUser_TextChanged(object sender, EventArgs e) { }
        private void txtPass_TextChanged(object sender, EventArgs e) { }
        #endregion
    }
}