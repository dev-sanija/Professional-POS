using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Runtime.InteropServices; // Added for draggable logic
using System.Windows.Forms;

namespace PosSystem
{
    public partial class frmCategory : Form
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

        private readonly frmCategoryList flist;

        public frmCategory(frmCategoryList frm)
        {
            InitializeComponent();
            flist = frm;
            // ELITE: Enable keyboard shortcuts for the form
            this.KeyPreview = true;
            this.KeyDown += FrmCategory_KeyDown;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        // ============================================================
        // 🔹 CLEAR FORM (Enterprise Optimized)
        // ============================================================
        public void Clear()
        {
            btnSave.Enabled = true;
            btnUpdate.Enabled = false;
            txtcategory.Clear();
            lblId.Text = ""; // FIX: Ensure ID is cleared to reset to "New" mode
            txtcategory.Focus();
        }

        // ============================================================
        // 🔹 SAVE CATEGORY (Enterprise Validation & Security)
        // ============================================================
        private async void btnSave_Click(object sender, EventArgs e)
        {
            // ELITE: Sanitized input check
            string categoryName = txtcategory.Text.Trim();

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                MessageBox.Show("Security Alert: Category name cannot be empty or just spaces.", "Input Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtcategory.Focus();
                return;
            }

            btnSave.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    await cn.OpenAsync();

                    // ENTERPRISE: Optimized Duplicate Check
                    string checkSql = "SELECT COUNT(*) FROM TblCategory WHERE UPPER(category) = @name";
                    using (var cmdCheck = new SQLiteCommand(checkSql, cn))
                    {
                        cmdCheck.Parameters.AddWithValue("@name", categoryName.ToUpper());
                        int count = Convert.ToInt32(await cmdCheck.ExecuteScalarAsync());
                        if (count > 0)
                        {
                            MessageBox.Show($"Entry Conflict: '{categoryName}' already exists in the system registry.", "Duplicate Detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    if (MessageBox.Show("Commit this category to the database?", "Confirm Transaction",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;

                    using (var cm = new SQLiteCommand("INSERT INTO TblCategory (category) VALUES (@category)", cn))
                    {
                        cm.Parameters.AddWithValue("@category", categoryName);
                        await cm.ExecuteNonQueryAsync();
                    }
                }

                MessageBox.Show("New category has been successfully registered.", "Registry Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Clear();
                if (flist != null)
                    await flist.LoadCategoryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Enterprise System Error: " + ex.Message, "Fatal Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnSave.Enabled = true;
            }
        }

        // ============================================================
        // 🔹 UPDATE CATEGORY (Elite Implementation)
        // ============================================================
        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            string categoryName = txtcategory.Text.Trim();

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                MessageBox.Show("Input Warning: Category name is required for updates.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnUpdate.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    await cn.OpenAsync();

                    // ELITE: Cross-check name against other IDs
                    string checkSql = "SELECT COUNT(*) FROM TblCategory WHERE UPPER(category) = @name AND id != @id";
                    using (var cmdCheck = new SQLiteCommand(checkSql, cn))
                    {
                        cmdCheck.Parameters.AddWithValue("@name", categoryName.ToUpper());
                        cmdCheck.Parameters.AddWithValue("@id", lblId.Text);
                        int count = Convert.ToInt32(await cmdCheck.ExecuteScalarAsync());
                        if (count > 0)
                        {
                            MessageBox.Show("Update Conflict: Another record already uses this name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    if (MessageBox.Show("Apply modifications to this category record?", "Update Confirmation",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;

                    using (var cm = new SQLiteCommand("UPDATE TblCategory SET category = @category WHERE id = @id", cn))
                    {
                        cm.Parameters.AddWithValue("@category", categoryName);
                        cm.Parameters.AddWithValue("@id", lblId.Text);
                        await cm.ExecuteNonQueryAsync();
                    }
                }

                MessageBox.Show("Category record synchronized successfully.", "Database Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (flist != null)
                    await flist.LoadCategoryAsync();

                this.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update Error: " + ex.Message, "System Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnUpdate.Enabled = true;
            }
        }

        // ============================================================
        // 🔹 ELITE INTERFACE HELPERS
        // ============================================================
        private void FrmCategory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Dispose();
            if (e.KeyCode == Keys.Enter && btnSave.Enabled) btnSave.PerformClick();
            if (e.KeyCode == Keys.Enter && btnUpdate.Enabled) btnUpdate.PerformClick();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void frmCategory_Load(object sender, EventArgs e)
        {
            txtcategory.Focus();
        }

        // ------------------------------------------------------------
        // 🔹 JUNK LINES (Preserved for Designer Compatibility)
        // ------------------------------------------------------------
        private void txtcategory_TextChanged(object sender, EventArgs e)
        {
            // ELITE: Auto-Capitalization UX placeholder
        }
    }
}