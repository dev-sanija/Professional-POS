using System;
using System.Data;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace PosSystem
{
    public partial class frmBrandList : Form
    {
        private readonly string _connectionString = DBConnection.MyConnection();
        private CancellationTokenSource searchTokenSource;

        public frmBrandList()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            _ = LoadRecordsAsync();
        }

        public async Task LoadRecordsAsync(string search = "")
        {
            if (IsDisposed) return;

            try
            {
                dataGridView1.SuspendLayout();
                int i = 0;
                dataGridView1.Rows.Clear();

                using (SQLiteConnection cn = new SQLiteConnection(_connectionString))
                {
                    await cn.OpenAsync();

                    string sql = "SELECT id, brand FROM BrandTbl WHERE brand LIKE @search ORDER BY brand ASC";
                    using (SQLiteCommand cm = new SQLiteCommand(sql, cn))
                    {
                        cm.Parameters.AddWithValue("@search", $"%{search}%");

                        using (var dr = await cm.ExecuteReaderAsync())
                        {
                            while (await dr.ReadAsync())
                            {
                                i++;
                                dataGridView1.Rows.Add(
                                    i,
                                    dr["id"].ToString(),
                                    dr["brand"].ToString()
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "frmBrandList_LoadRecordsAsync");
            }
            finally
            {
                try
                {
                    dataGridView1.ResumeLayout();
                }
                catch { }
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            frmBrand frm = new frmBrand(this);
            frm.button1.Enabled = true;
            frm.button2.Enabled = false;
            frm.ShowDialog();
        }

        private async void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            string brandId = dataGridView1.Rows[e.RowIndex].Cells[1].Value?.ToString() ?? "";
            string brandName = dataGridView1.Rows[e.RowIndex].Cells[2].Value?.ToString() ?? "";

            try
            {
                if (colName == "Edit")
                {
                    frmBrand frm = new frmBrand(this);
                    frm.lblId.Text = brandId;
                    frm.txtBrand.Text = brandName;
                    frm.button1.Enabled = false;
                    frm.button2.Enabled = true;
                    frm.ShowDialog();
                }
                else if (colName == "Delete")
                {
                    if (string.IsNullOrWhiteSpace(brandId))
                    {
                        MessageBox.Show("Invalid brand record selected.", "Warning",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (MessageBox.Show(
                        $"Are you sure you want to permanently delete brand '{brandName}'?",
                        "Confirm Deletion",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        DeleteBrandResult result = await DeleteBrandAsync(brandId);

                        if (result == DeleteBrandResult.InUse)
                        {
                            MessageBox.Show(
                                $"Brand '{brandName}' cannot be deleted because it is linked to existing product records.",
                                "Delete Blocked",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                            return;
                        }

                        if (result == DeleteBrandResult.NotFound)
                        {
                            MessageBox.Show(
                                "Brand record was not found or may already have been removed.",
                                "Information",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            await LoadRecordsAsync(txtSearch.Text.Trim());
                            return;
                        }

                        await LoadRecordsAsync(txtSearch.Text.Trim());

                        MessageBox.Show(
                            $"Brand record for '{brandName}' has been deleted successfully.",
                            "Action Successful",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "frmBrandList_CellContentClick");
                MessageBox.Show(
                    $"Workflow Error:\n{ex.Message}",
                    "System Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private enum DeleteBrandResult
        {
            Deleted = 1,
            InUse = 2,
            NotFound = 3
        }

        private async Task<DeleteBrandResult> DeleteBrandAsync(string brandId)
        {
            using (SQLiteConnection cn = new SQLiteConnection(_connectionString))
            {
                await cn.OpenAsync();

                using (SQLiteCommand pragma = new SQLiteCommand("PRAGMA foreign_keys = ON;", cn))
                {
                    await pragma.ExecuteNonQueryAsync();
                }

                using (var transaction = cn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand check = new SQLiteCommand(
                            "SELECT COUNT(*) FROM TblProduct1 WHERE bid = @id;", cn, transaction))
                        {
                            check.Parameters.AddWithValue("@id", brandId);
                            object linkedResult = await check.ExecuteScalarAsync();
                            int linkedCount = linkedResult == null || linkedResult == DBNull.Value
                                ? 0
                                : Convert.ToInt32(linkedResult);

                            if (linkedCount > 0)
                            {
                                transaction.Rollback();
                                return DeleteBrandResult.InUse;
                            }
                        }

                        using (SQLiteCommand cm = new SQLiteCommand(
                            "DELETE FROM BrandTbl WHERE id = @id;", cn, transaction))
                        {
                            cm.Parameters.AddWithValue("@id", brandId);
                            int affected = await cm.ExecuteNonQueryAsync();

                            transaction.Commit();

                            return affected > 0
                                ? DeleteBrandResult.Deleted
                                : DeleteBrandResult.NotFound;
                        }
                    }
                    catch
                    {
                        try { transaction.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (searchTokenSource != null)
            {
                searchTokenSource.Cancel();
                searchTokenSource.Dispose();
            }

            searchTokenSource = new CancellationTokenSource();
            CancellationToken token = searchTokenSource.Token;

            try
            {
                await Task.Delay(350, token);

                if (!token.IsCancellationRequested && !IsDisposed)
                {
                    await LoadRecordsAsync(txtSearch.Text.Trim());
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "frmBrandList_txtSearch_TextChanged");
            }
        }

        private void txtSearch_TextChanged_1(object sender, EventArgs e)
        {
            txtSearch_TextChanged(sender, e);
        }

        private void txtSearch_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
        {
            frmBrand frm = new frmBrand(this);
            frm.button1.Enabled = true;
            frm.button2.Enabled = false;
            frm.Owner = this;
            frm.ShowDialog();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                if (searchTokenSource != null)
                {
                    searchTokenSource.Cancel();
                    searchTokenSource.Dispose();
                    searchTokenSource = null;
                }
            }
            catch { }

            base.OnFormClosed(e);
        }
    }
}