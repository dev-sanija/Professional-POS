using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PosSystem
{
    public partial class frmCancelDetails : Form
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

        private readonly frmSoldItems f;
        private decimal unitPrice = 0;
        private bool _isSaving = false;

        public frmCancelDetails(frmSoldItems frm, decimal soldPrice = 0)
        {
            InitializeComponent();
            f = frm;
            unitPrice = soldPrice;
            KeyPreview = true;

            txtCancelQty.TextChanged += TxtCancelQty_TextChanged;

            txtID.KeyDown += TxtID_KeyDown;
            txtPcode.KeyDown += TxtPcode_KeyDown;
            txtTransno.KeyDown += TxtTransno_KeyDown;
        }

        private void frmCancelDetails_Load(object sender, EventArgs e)
        {
            cbAction.DropDownStyle = ComboBoxStyle.DropDownList;
            txtID.ReadOnly = false;
            txtPcode.ReadOnly = false;
            txtTransno.ReadOnly = false;

            // ENSURE THESE ARE NOT GRAYED OUT
            txtVoidBy.Enabled = true;
            txtCancelled.Enabled = true;
            txtCancelled.ReadOnly = false;
            txtVoidBy.ReadOnly = false;


            cbAction.Focus();
        }


        private void pictureBox1_Click(object sender, EventArgs e) => Dispose();
        private void cbAction_KeyPress(object sender, KeyPressEventArgs e) => e.Handled = true;
        private void TxtCancelQty_TextChanged(object sender, EventArgs e) => PerformCalculation();

        private void PerformCalculation()
        {
            try
            {
                if (decimal.TryParse(txtCancelQty.Text, out decimal qty))
                    txtTotal.Text = (unitPrice * qty).ToString("N2");
                else
                    txtTotal.Text = "0.00";
            }
            catch
            {
                txtTotal.Text = "0.00";
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (_isSaving) return;
            _isSaving = true;

            btnSave.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                if (string.IsNullOrWhiteSpace(cbAction.Text) ||
                    string.IsNullOrWhiteSpace(txtCancelQty.Text) ||
                    string.IsNullOrWhiteSpace(txtReason.Text) ||
                    string.IsNullOrWhiteSpace(txtVoidBy.Text) ||
                    string.IsNullOrWhiteSpace(txtCancelled.Text))
                {
                    MessageBox.Show("Please fill all cancellation fields.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ResetSaveState();
                    return;
                }

                using (SQLiteConnection con = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    con.Open();

                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT role FROM tblUser WHERE UPPER(username) = @user AND (isactive = 'True' OR isactive = 1)", con))
                    {
                        cmd.Parameters.AddWithValue("@user", txtCancelled.Text.Trim().ToUpper());
                        object roleObj = cmd.ExecuteScalar();
                        if (roleObj == null)
                        {
                            MessageBox.Show("The user in 'Cancelled By' is not a valid active user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ResetSaveState();
                            return;
                        }
                    }

                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT role FROM tblUser WHERE UPPER(username) = @user AND (isactive = 'True' OR isactive = 1)", con))
                    {
                        cmd.Parameters.AddWithValue("@user", txtVoidBy.Text.Trim().ToUpper());
                        object roleObj = cmd.ExecuteScalar();

                        if (roleObj == null)
                        {
                            MessageBox.Show("The Admin in 'Voided By' does not exist.", "Invalid Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ResetSaveState();
                            return;
                        }

                        string userRole = roleObj.ToString().Trim();
                        bool isAdmin = userRole.Equals("System Administrator", StringComparison.OrdinalIgnoreCase) ||
                                       userRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase);

                        if (!isAdmin)
                        {
                            MessageBox.Show($"Access Denied: {txtVoidBy.Text} is a '{userRole}', not an Admin!", "Security", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            ResetSaveState();
                            return;
                        }
                    }
                }

                if (!int.TryParse(txtQty.Text, out int currentQty) ||
                    !int.TryParse(txtCancelQty.Text, out int cancelQty))
                {
                    MessageBox.Show("Invalid quantity format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ResetSaveState();
                    return;
                }

                if (cancelQty <= 0 || cancelQty > currentQty)
                {
                    MessageBox.Show("Cancel quantity must be greater than 0 and not exceed current quantity.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ResetSaveState();
                    return;
                }

                if (MessageBox.Show("Proceed with this cancellation?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    ResetSaveState();
                    return;
                }

                bool saveSuccess = false;

                using (frmVoid voidForm = new frmVoid())
                {
                    voidForm.CancelAction = cbAction.Text.Trim();
                    voidForm.CancelQty = cancelQty;
                    voidForm.CancelReason = txtReason.Text.Trim();
                    voidForm.ProductCode = txtPcode.Text;
                    voidForm.CartId = txtID.Text;
                    voidForm.Price = unitPrice;
                    voidForm.CancelledBy = txtCancelled.Text;

                    // Assigning to the property in frmVoid
                    voidForm.VoidBy = txtVoidBy.Text.Trim();

                    voidForm.SoldItemForm = f;

                    voidForm.ShowDialog();
                    saveSuccess = voidForm.SaveSuccess;

                    if (!saveSuccess)
                    {
                        ResetSaveState();
                        return;
                    }
                }

                if (f != null)
                    await f.LoadSoldItemsAsync();

                var reportData = new
                {
                    ID = txtID.Text,
                    PCode = txtPcode.Text,
                    Desc = txtDesc.Text,
                    Qty = txtQty.Text,
                    CancelQty = txtCancelQty.Text,
                    Price = txtPrice.Text,
                    Total = txtTotal.Text,
                    Action = cbAction.Text,
                    Reason = txtReason.Text,
                    VoidBy = txtVoidBy.Text,
                    DateNow = DateTime.Now
                };

                await Task.Run(() => ExportCancelDetailsToPDF(reportData));

                this.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Critical Error: " + ex.Message);
                ResetSaveState();
            }
            finally
            {
                _isSaving = false;
                btnSave.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        // Add this helper method to handle UI resets cleanly
        private void ResetSaveState()
        {
            Cursor = Cursors.Default;
            btnSave.Enabled = true;
            _isSaving = false;
        }

        private void frmCancelDetails_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Dispose();
            else if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnSave_Click(sender, e);
            }
        }

        #region AutoFill

        private void TxtID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                AutoFill("id", txtID.Text.Trim());
            }
        }

        private void TxtPcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                AutoFill("pcode", txtPcode.Text.Trim());
            }
        }

        private void TxtTransno_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                AutoFill("transno", txtTransno.Text.Trim());
            }
        }

        private void AutoFill(string column, string value)
        {
            using (SQLiteConnection con = new SQLiteConnection(DBConnection.MyConnection()))
            {
                con.Open();
                using (SQLiteCommand cmd = new SQLiteCommand($"SELECT * FROM vwSoldItems WHERE {column}=@val LIMIT 1", con))
                {
                    cmd.Parameters.AddWithValue("@val", value);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtID.Text = reader["id"].ToString();
                            txtPcode.Text = reader["pcode"].ToString();
                            txtDesc.Text = reader["pdesc"].ToString();
                            txtTransno.Text = reader["transno"].ToString();
                            txtPrice.Text = reader["price"].ToString();
                            txtQty.Text = reader["qty"].ToString();
                            txtDiscount.Text = reader["disc"].ToString();
                            txtTotal.Text = reader["total"].ToString();

                            decimal.TryParse(reader["price"].ToString(), out unitPrice);
                            PerformCalculation();
                        }
                        else
                        {
                            MessageBox.Show("No sold record found.");
                            ClearSoldFields();
                        }
                    }
                }
            }
        }

        private void ClearSoldFields()
        {
            txtID.Clear();
            txtPcode.Clear();
            txtDesc.Clear();
            txtTransno.Clear();
            txtPrice.Clear();
            txtQty.Clear();
            txtDiscount.Clear();
            txtTotal.Text = "0.00";
        }

        #endregion

        #region DATABASE UPDATE
        private void UpdateDatabaseCancellation(int cancelQty)
        {
            using (var con = new SQLiteConnection(DBConnection.MyConnection()))
            {
                con.Open();
                using (var transaction = con.BeginTransaction())
                {
                    try
                    {
                        // 1. Log to tblCancel (Existing logic)
                        using (var cmd = new SQLiteCommand(@"
                    INSERT INTO tblCancel 
                    (cart_id, pcode, qty, price, total, action, reason, cancelled_by, void_by, date_cancelled) 
                    VALUES (@cart,@pcode,@qty,@price,@total,@action,@reason,@cancelled,@voidby,@date)", con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@cart", txtID.Text);
                            cmd.Parameters.AddWithValue("@pcode", txtPcode.Text);
                            cmd.Parameters.AddWithValue("@qty", cancelQty);
                            cmd.Parameters.AddWithValue("@price", unitPrice);
                            cmd.Parameters.AddWithValue("@total", decimal.Parse(txtTotal.Text));
                            cmd.Parameters.AddWithValue("@action", cbAction.Text);
                            cmd.Parameters.AddWithValue("@reason", txtReason.Text);
                            cmd.Parameters.AddWithValue("@cancelled", txtCancelled.Text);
                            cmd.Parameters.AddWithValue("@voidby", txtVoidBy.Text);
                            cmd.Parameters.AddWithValue("@date", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Update tblCart1 (Reduce the quantity or remove if zero)
                        using (var cmd = new SQLiteCommand(@"
                    UPDATE tblCart1 
                    SET qty = qty - @cancelQty, 
                        total = (qty - @cancelQty) * price 
                    WHERE id = @id", con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@cancelQty", cancelQty);
                            cmd.Parameters.AddWithValue("@id", txtID.Text);
                            cmd.ExecuteNonQuery();

                            // Cleanup: Delete row if quantity becomes 0
                            using (var delCmd = new SQLiteCommand("DELETE FROM tblCart1 WHERE qty <= 0 AND id = @id", con, transaction))
                            {
                                delCmd.Parameters.AddWithValue("@id", txtID.Text);
                                delCmd.ExecuteNonQuery();
                            }
                        }

                        // 3. Restore Stock to TblProduct1 (If Action is YES)
                        if (cbAction.Text == "Yes")
                        {
                            using (var cmd = new SQLiteCommand(@"
                        UPDATE TblProduct1 
                        SET qty = qty + @qty 
                        WHERE pcode = @pcode", con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@qty", cancelQty);
                                cmd.Parameters.AddWithValue("@pcode", txtPcode.Text);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Database Sync Failed: " + ex.Message);
                    }
                }
            }
        }
        #endregion

        #region Export PDF
        private void ExportCancelDetailsToPDF(dynamic d)
        {
            try
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "POS_Cancellations");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string path = Path.Combine(folder, $"Void_{d.ID}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

                Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(doc, new FileStream(path, FileMode.Create));
                doc.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var standardFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                doc.Add(new Paragraph("VOIDED TRANSACTION REPORT", titleFont));
                doc.Add(new Paragraph($"Date generated: {d.DateNow:f}", standardFont));
                doc.Add(new Paragraph("---------------------------------------------------------"));
                doc.Add(new Paragraph($"Cart Item ID (tblCart1): {d.ID}", standardFont));
                doc.Add(new Paragraph($"Product: [{d.PCode}] {d.Desc}", standardFont));
                doc.Add(new Paragraph($"Original Sold Qty: {d.Qty}", standardFont));
                doc.Add(new Paragraph($"Voided Qty: {d.CancelQty}", standardFont));
                doc.Add(new Paragraph($"Unit Price: {d.Price}", standardFont));
                doc.Add(new Paragraph($"Refund Amount: {d.Total}", standardFont));
                doc.Add(new Paragraph($"Action (Restored Stock?): {d.Action}", standardFont));
                doc.Add(new Paragraph($"Reason: {d.Reason}", standardFont));
                doc.Add(new Paragraph($"Authorized By: {d.VoidBy}", standardFont));

                doc.Close();
                MessageBox.Show($"Void Record Exported:\n{path}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("PDF Export Failed: " + ex.Message);
            }
        }
        #endregion

        #region Junk Designer Stubs
        private void txtID_TextChanged(object sender, EventArgs e) { }
        private void txtPcode_TextChanged(object sender, EventArgs e) { }
        private void txtDesc_TextChanged(object sender, EventArgs e) { }
        private void txtTransno_TextChanged(object sender, EventArgs e) { }
        private void txtPrice_TextChanged(object sender, EventArgs e) { }
        private void txtQty_TextChanged(object sender, EventArgs e) { }
        private void txtDiscount_TextChanged(object sender, EventArgs e) { }
        private void txtTotal_TextChanged(object sender, EventArgs e) { }
        private void txtVoidBy_TextChanged(object sender, EventArgs e) { }
        private void txtCancelled_TextChanged(object sender, EventArgs e) { }
        private void cbAction_SelectedIndexChanged(object sender, EventArgs e) { }
        private void txtReason_TextChanged(object sender, EventArgs e) { }
        private void txtCancelQty_TextChanged_1(object sender, EventArgs e) { PerformCalculation(); }
        #endregion
    }
}