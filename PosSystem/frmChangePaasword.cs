using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosSystem
{
    public partial class frmChangePaasword : Form
    {
        private readonly Form1 f;
        private readonly string stitle = "POS System";

        public frmChangePaasword(Form1 frm)
        {
            InitializeComponent();
            f = frm;
            KeyPreview = true; // Required for Enter/Esc shortcuts
        }

        private void frmChangePaasword_Load(object sender, EventArgs e)
        {
            txtOld.Focus();
        }

        // ============================================================
        // 🔹 SAVE PASSWORD ASYNC (Enterprise Implementation)
        // ============================================================
        private async void btnSave_Click(object sender, EventArgs e)
        {
            string oldPass = txtOld.Text.Trim();
            string newPass = txtNew.Text.Trim();
            string confirmPass = txtConfirm.Text.Trim();

            // 1. ELITE VALIDATION: Multi-tier check
            if (string.IsNullOrWhiteSpace(oldPass) || string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(confirmPass))
            {
                MessageBox.Show("Security requirement: All password fields are mandatory.", stitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPass != confirmPass)
            {
                MessageBox.Show("Input mismatch: The new password and confirmation do not match.", stitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPass.Length < 4) // Enterprise standard: minimum length
            {
                MessageBox.Show("Security Policy: New password is too short. Use at least 4 characters.", stitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnSave.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                string username = f.lblUserName.Text;

                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    await cn.OpenAsync();

                    // 2. ELITE SECURITY: Fetch Salt and Password together
                    string dbHash = "";
                    string dbSalt = "";

                    using (var checkCmd = new SQLiteCommand("SELECT password, salt FROM tblUser WHERE username = @username", cn))
                    {
                        checkCmd.Parameters.AddWithValue("@username", username);
                        using (var dr = await checkCmd.ExecuteReaderAsync())
                        {
                            if (await dr.ReadAsync())
                            {
                                dbHash = dr["password"].ToString();
                                dbSalt = dr["salt"].ToString();
                            }
                        }
                    }

                    // 3. VERIFY OLD PASSWORD (Using DB Salt)
                    string hashedOldInput = DBConnection.GetHash(oldPass, dbSalt);

                    if (dbHash != hashedOldInput)
                    {
                        MessageBox.Show("Security Alert: The old password you entered is incorrect.", stitle, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        txtOld.Focus();
                        return;
                    }

                    // 4. PREVENT REUSING OLD PASSWORD
                    if (oldPass == newPass)
                    {
                        MessageBox.Show("Policy Restriction: New password cannot be the same as the old password.", stitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // 5. UPDATE WITH NEW HASH (Reuse same salt for stability or generate new for Elite)
                    if (MessageBox.Show("Are you sure you want to update your access credentials?", stitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        using (var updateCmd = new SQLiteCommand("UPDATE tblUser SET password = @password WHERE username = @username", cn))
                        {
                            updateCmd.Parameters.AddWithValue("@password", DBConnection.GetHash(newPass, dbSalt));
                            updateCmd.Parameters.AddWithValue("@username", username);
                            await updateCmd.ExecuteNonQueryAsync();
                        }

                        MessageBox.Show("Transaction Complete: Your password has been updated successfully.", stitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Enterprise Logic Error: " + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnSave.Enabled = true;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e) => Dispose();

        private void frmChangePaasword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Dispose();
            else if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Prevents the system beep
                btnSave_Click(sender, e);
            }
        }

        // --------------------------------------------------------------------------------------
        // 🔹 JUNK LINES (Preserved for Designer Compatibility)
        // --------------------------------------------------------------------------------------
        private void txtOld_Click(object sender, EventArgs e)
        {
            // ELITE: Auto-select text for easier editing
            txtOld.SelectAll();
        }

        private void txtNew_Click(object sender, EventArgs e)
        {
            txtNew.SelectAll();
        }

        private void txtConfirm_Click(object sender, EventArgs e)
        {
            txtConfirm.SelectAll();
        }
    }
}