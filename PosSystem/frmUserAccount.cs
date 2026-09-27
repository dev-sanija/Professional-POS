using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosSystem
{
    public partial class frmUserAccount : Form
    {
        private readonly Form1 _parent;

        private static readonly Bitmap StatusActiveDot;
        private static readonly Bitmap StatusInactiveDot;

        static frmUserAccount()
        {
            StatusActiveDot = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(StatusActiveDot))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.FillEllipse(Brushes.LimeGreen, 2, 2, 12, 12);
            }

            StatusInactiveDot = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(StatusInactiveDot))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.FillEllipse(Brushes.Crimson, 2, 2, 12, 12);
            }
        }

        public frmUserAccount(Form1 f)
        {
            InitializeComponent();
            _parent = f;
            this.AcceptButton = button1;

            this.Load += frmUserAccount_Load;
            this.dataGridViewActive.CellFormatting += dataGridViewActive_CellFormatting;

            this.dataGridViewActive.AutoGenerateColumns = false;
            ConfigureActiveDataGridView();

            txtU.Enabled = true;
            txtU.ReadOnly = false;
        }

        private void ConfigureActiveDataGridView()
        {
            dataGridViewActive.RowHeadersVisible = false;
            dataGridViewActive.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewActive.ReadOnly = true;
            dataGridViewActive.AllowUserToAddRows = false;
            dataGridViewActive.AllowUserToResizeRows = false;
            dataGridViewActive.AllowUserToResizeColumns = false;
            dataGridViewActive.MultiSelect = false;

            if (dataGridViewActive.Columns["colName"] != null) dataGridViewActive.Columns["colName"].DataPropertyName = "name";
            if (dataGridViewActive.Columns["colUser"] != null) dataGridViewActive.Columns["colUser"].DataPropertyName = "username";
            if (dataGridViewActive.Columns["colRole"] != null) dataGridViewActive.Columns["colRole"].DataPropertyName = "role";
            if (dataGridViewActive.Columns["isactive"] != null)
            {
                dataGridViewActive.Columns["isactive"].DataPropertyName = "isactive";
                dataGridViewActive.Columns["isactive"].Visible = false;
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e) => this.Dispose();

        private async void frmUserAccount_Load(object sender, EventArgs e)
        {
            await LoadUserListAsync();
        }

        #region UI Event Handlers

        private async void button1_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text.Trim().ToLower();
            string password = txtPassword.Text;
            string role = cbRole.Text.Trim();
            string name = txtName.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                Notify("Please fill all required fields.", "Error", MessageBoxIcon.Warning);
                return;
            }

            if (password != txtRePassword.Text)
            {
                Notify("Passwords do not match.", "Error", MessageBoxIcon.Warning);
                return;
            }

            SetBusyState(true, button1);
            try
            {
                bool success = await UserRepositoryV2.CreateOrRestoreAccountAsync(username, password, role, name);
                if (success)
                {
                    Notify("Account successfully saved/restored.", "Success", MessageBoxIcon.Information);
                    ClearNewAccountFields();
                    await LoadUserListAsync();
                }
                else
                {
                    Notify("This username is currently active in the system.", "Duplicate", MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                HandleError("Creation Fault", ex);
            }
            finally
            {
                SetBusyState(false, button1);
            }
        }

        private async void btnSav_Click(object sender, EventArgs e)
        {
            string username = txtU.Text.Trim().ToLower();
            string oldPass = txtOld.Text;
            string newPass = txtNew.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(oldPass) || string.IsNullOrEmpty(newPass))
            {
                Notify("Please enter Username, Old Password and New Password.", "Warning", MessageBoxIcon.Warning);
                return;
            }

            if (newPass != txtRePass.Text)
            {
                Notify("New passwords do not match.", "Warning", MessageBoxIcon.Warning);
                return;
            }

            SetBusyState(true, btnSav);
            try
            {
                bool success = await UserRepositoryV2.ChangePasswordAsync(username, oldPass, newPass);
                if (success)
                {
                    Notify("Credentials updated successfully.", "Security", MessageBoxIcon.Information);
                    txtU.Clear(); txtOld.Clear(); txtNew.Clear(); txtRePass.Clear();
                }
                else
                {
                    Notify("Invalid username or old password.", "Failed", MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                HandleError("Security Fault", ex);
            }
            finally
            {
                SetBusyState(false, btnSav);
            }
        }

        private async void txtuser2_TextChanged(object sender, EventArgs e)
        {
            string username = txtuser2.Text.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(username)) { checkBox1.Checked = false; return; }

            try
            {
                bool isActive = await UserRepositoryV2.GetUserStatusAsync(username);
                checkBox1.Checked = isActive;
            }
            catch { /* Silent fail */ }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            string username = txtuser2.Text.Trim().ToLower();
            bool status = checkBox1.Checked;

            if (string.IsNullOrEmpty(username)) return;

            SetBusyState(true, button2);
            try
            {
                bool updated = await UserRepositoryV2.UpdateStatusAsync(username, status);
                if (updated)
                {
                    Notify("User status updated.", "Success", MessageBoxIcon.Information);
                    await LoadUserListAsync();
                }
                else
                    Notify("User not found.", "Warning", MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                HandleError("Update Fault", ex);
            }
            finally
            {
                SetBusyState(false, button2);
            }
        }

        private void dataGridViewActive_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dataGridViewActive.Columns[e.ColumnIndex].Name == "StatusDot")
            {
                var cellValue = dataGridViewActive.Rows[e.RowIndex].Cells["isactive"].Value;
                if (cellValue != null && cellValue != DBNull.Value)
                {
                    bool isActive = Convert.ToInt32(cellValue) == 1;
                    e.Value = isActive ? StatusActiveDot : StatusInactiveDot;
                }
            }

            if (dataGridViewActive.Columns[e.ColumnIndex].Name == "Revoke")
            {
                var cellValue = dataGridViewActive.Rows[e.RowIndex].Cells["isactive"].Value;
                if (cellValue != null && cellValue != DBNull.Value)
                {
                    bool isActive = Convert.ToInt32(cellValue) == 1;
                    e.Value = isActive ? "Deactivate" : "Activate";
                }
            }
        }

        private async void dataGridViewActive_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dataGridViewActive.Columns[e.ColumnIndex].Name;

            if (colName == "Revoke")
            {
                var selectedRow = dataGridViewActive.Rows[e.RowIndex];
                string targetUser = selectedRow.Cells["colUser"].Value.ToString();
                bool currentStatus = Convert.ToInt32(selectedRow.Cells["isactive"].Value) == 1;

                string currentUser = "";
                try { currentUser = _parent.Controls.Find("lblUser", true)[0].Text; } catch { currentUser = "admin"; }

                if (currentStatus && targetUser.ToLower() == currentUser.ToLower())
                {
                    Notify("You cannot deactivate your own account.", "Denied", MessageBoxIcon.Stop);
                    return;
                }

                string action = currentStatus ? "deactivate" : "activate";

                if (MessageBox.Show($"Are you sure you want to {action} account: {targetUser}?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    bool success = await UserRepositoryV2.UpdateStatusAsync(targetUser, !currentStatus);
                    if (success) await LoadUserListAsync();
                }
            }
        }

        public async Task LoadUserListAsync()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                DataTable dt = new DataTable();
                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    await cn.OpenAsync();
                    using (var cm = new SQLiteCommand("SELECT name, username, role, isactive FROM tblUser", cn))
                    using (var dr = await cm.ExecuteReaderAsync())
                    {
                        dt.Load(dr);
                    }
                }
                dataGridViewActive.DataSource = dt;
                dataGridViewActive.ClearSelection();
            }
            catch (Exception ex) { HandleError("Load Users", ex); }
            finally { Cursor = Cursors.Default; }
        }

        #endregion

        #region Fix for Designer Errors

        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void btnSave_Click(object sender, EventArgs e) => ClearNewAccountFields();
        private void textBox2_TextChanged(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void tabPage2_Click_1(object sender, EventArgs e) { }
        private void txtOld_TextChanged(object sender, EventArgs e) { }
        private void frmUserAccount_Resize(object sender, EventArgs e) { }

        #endregion

        #region Helpers

        private async Task<bool> ValidateNewUserAsync(string user, string pass, string confirm, string role)
        {
            if (string.IsNullOrWhiteSpace(user)) { Notify("Username required.", "Error", MessageBoxIcon.Warning); return false; }
            if (string.IsNullOrWhiteSpace(role)) { Notify("Select a role.", "Error", MessageBoxIcon.Warning); return false; }
            if (pass.Length < 4) { Notify("Password too short (Min 4).", "Error", MessageBoxIcon.Warning); return false; }
            if (pass != confirm) { Notify("Passwords mismatch.", "Error", MessageBoxIcon.Warning); return false; }
            return true;
        }

        private void SetBusyState(bool isBusy, Button btn) => btn.Enabled = !isBusy;
        private void Notify(string msg, string title, MessageBoxIcon icon) => MessageBox.Show(msg, title, MessageBoxButtons.OK, icon);
        private void HandleError(string context, Exception ex) => MessageBox.Show($"{context}: {ex.Message}", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        private void ClearNewAccountFields()
        {
            txtUser.Clear(); txtPassword.Clear(); txtRePassword.Clear(); txtName.Clear();
            cbRole.SelectedIndex = -1;
            txtUser.Focus();
        }

        #endregion

        private async void btndeluser_Click(object sender, EventArgs e)
        {
            string username = txtuser2.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(username))
            {
                Notify("Please enter a username to delete.", "Warning", MessageBoxIcon.Warning);
                return;
            }

            string currentUser = "";
            try { currentUser = txtU.Text.Trim().ToLower(); } catch { currentUser = "admin"; }

            if (username == currentUser)
            {
                Notify("Security Restriction: You cannot delete yourself.", "Denied", MessageBoxIcon.Stop);
                return;
            }

            if (username == "admin" || username == "administrator")
            {
                Notify("Default admin account cannot be deleted.", "Denied", MessageBoxIcon.Stop);
                return;
            }

            if (MessageBox.Show($"Are you sure you want to permanently delete user: {username}?",
                "Confirm Permanent Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    bool deleted = await DeleteUserPermanentlyAsync(username);

                    if (deleted)
                    {
                        Notify("User permanently deleted successfully.", "Success", MessageBoxIcon.Information);
                        txtuser2.Clear();
                        checkBox1.Checked = false;
                        await LoadUserListAsync();
                    }
                    else
                    {
                        Notify("User not found.", "Error", MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    HandleError("Permanent Delete Fault", ex);
                }
            }
        }

        private async Task<bool> DeleteUserPermanentlyAsync(string username)
        {
            using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
            {
                await cn.OpenAsync();

                using (var cm = new SQLiteCommand("DELETE FROM tblUser WHERE LOWER(username) = @username", cn))
                {
                    cm.Parameters.AddWithValue("@username", username.ToLower());
                    int affected = await cm.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }
    }
}