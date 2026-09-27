using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace PosSystem
{
    public partial class frmBrand : Form
    {
        private readonly string _connectionString = DBConnection.MyConnection();
        private readonly frmBrandList _frmlist;

        public frmBrand(frmBrandList flist)
        {
            InitializeComponent();
            _frmlist = flist;

            this.StartPosition = FormStartPosition.CenterParent;
            this.KeyPreview = true;
            this.KeyDown += FrmBrand_KeyDown;
        }

        private void FrmBrand_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Dispose();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                if (button1.Enabled)
                    button1_Click(sender, e);
                else if (button2.Enabled)
                    button2_Click(sender, e);
            }
        }

        private void Clear()
        {
            txtBrand.Clear();
            lblId.Text = "";
            txtBrand.Focus();

            button1.Enabled = true;
            button2.Enabled = false;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string brandName = txtBrand.Text.Trim();

            if (string.IsNullOrWhiteSpace(brandName))
            {
                MessageBox.Show("Brand name is required to proceed.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBrand.Focus();
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;
                button1.Enabled = false;
                button2.Enabled = false;

                if (MessageBox.Show(
                    $"Are you sure you want to save '{brandName}'?",
                    "Confirm Save",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                using (var cn = new SQLiteConnection(_connectionString))
                {
                    await cn.OpenAsync();

                    using (var transaction = cn.BeginTransaction())
                    {
                        try
                        {
                            if (await IsDuplicateAsync(cn, transaction, brandName))
                            {
                                MessageBox.Show(
                                    $"Brand '{brandName}' already exists in the system.",
                                    "Duplicate Conflict",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Stop);
                                try { transaction.Rollback(); } catch { }
                                txtBrand.Focus();
                                txtBrand.SelectAll();
                                return;
                            }

                            using (var cm = new SQLiteCommand(
                                "INSERT INTO BrandTbl (brand) VALUES (@brand)", cn, transaction))
                            {
                                cm.Parameters.AddWithValue("@brand", brandName);
                                await cm.ExecuteNonQueryAsync();
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            try { transaction.Rollback(); } catch { }
                            throw;
                        }
                    }
                }

                MessageBox.Show(
                    "New brand has been successfully registered.",
                    "Registry Updated",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                if (_frmlist != null)
                    await _frmlist.LoadRecordsAsync();

                Clear();
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "frmBrand_Save");
                MessageBox.Show(
                    "System Error while saving: " + ex.Message,
                    "Enterprise Database Failure",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                button1.Enabled = true;
                button2.Enabled = false;
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            string brandName = txtBrand.Text.Trim();
            string id = lblId.Text.Trim();

            if (string.IsNullOrWhiteSpace(brandName) || string.IsNullOrWhiteSpace(id))
            {
                MessageBox.Show(
                    "Target brand ID or name is missing. Please reload the record.",
                    "System Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;
                button1.Enabled = false;
                button2.Enabled = false;

                if (MessageBox.Show(
                    $"Update current record to '{brandName}'?",
                    "Confirm Update",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                using (var cn = new SQLiteConnection(_connectionString))
                {
                    await cn.OpenAsync();

                    using (var transaction = cn.BeginTransaction())
                    {
                        try
                        {
                            if (await IsDuplicateAsync(cn, transaction, brandName, id))
                            {
                                MessageBox.Show(
                                    $"The name '{brandName}' is already assigned to another brand.",
                                    "Conflict Detected",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Stop);
                                try { transaction.Rollback(); } catch { }
                                txtBrand.Focus();
                                txtBrand.SelectAll();
                                return;
                            }

                            int affected;
                            using (var cm = new SQLiteCommand(
                                "UPDATE BrandTbl SET brand=@brand WHERE id=@id", cn, transaction))
                            {
                                cm.Parameters.AddWithValue("@brand", brandName);
                                cm.Parameters.AddWithValue("@id", id);
                                affected = await cm.ExecuteNonQueryAsync();
                            }

                            if (affected <= 0)
                            {
                                try { transaction.Rollback(); } catch { }
                                MessageBox.Show(
                                    "Brand record not found or already changed.",
                                    "Update Cancelled",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                                return;
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            try { transaction.Rollback(); } catch { }
                            throw;
                        }
                    }
                }

                MessageBox.Show(
                    "Record modified successfully.",
                    "Update Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                if (_frmlist != null)
                    await _frmlist.LoadRecordsAsync();

                this.Dispose();
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "frmBrand_Update");
                MessageBox.Show(
                    "System Error while updating: " + ex.Message,
                    "Enterprise Update Failure",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                button1.Enabled = false;
                button2.Enabled = true;
            }
        }

        private async Task<bool> IsDuplicateAsync(SQLiteConnection cn, SQLiteTransaction transaction, string brandName, string id = "")
        {
            using (var cm = new SQLiteCommand(
                "SELECT COUNT(*) FROM BrandTbl WHERE LOWER(TRIM(brand)) = LOWER(TRIM(@brand))" +
                (string.IsNullOrWhiteSpace(id) ? "" : " AND id <> @id"), cn, transaction))
            {
                cm.Parameters.AddWithValue("@brand", brandName);

                if (!string.IsNullOrWhiteSpace(id))
                    cm.Parameters.AddWithValue("@id", id);

                object result = await cm.ExecuteScalarAsync();
                int count = result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
                return count > 0;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e) => this.Dispose();
        private void pictureBox1_Click(object sender, EventArgs e) => this.Dispose();
        private void frmBrand_Load(object sender, EventArgs e) => txtBrand.Focus();
        private void button3_Click(object sender, EventArgs e) => Clear();

        private void txtBrand_TextChanged(object sender, EventArgs e)
        {
        }
    }
}