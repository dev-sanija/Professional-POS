using System;
using System.Data;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PosSystem
{
    public static class UserRepositoryV2
    {
        // ===============================
        // CREATE OR RESTORE ACCOUNT
        // ===============================
        public static async Task<bool> CreateOrRestoreAccountAsync(string username, string password, string role, string name)
        {
            using var cn = new SQLiteConnection(DBConnection.MyConnection());
            await cn.OpenAsync();

            // Check if user exists
            using (var cmd = new SQLiteCommand("SELECT isactive FROM tblUser WHERE username=@user", cn))
            {
                cmd.Parameters.AddWithValue("@user", username);
                using var dr = await cmd.ExecuteReaderAsync();
                if (await dr.ReadAsync())
                {
                    bool isActive = Convert.ToInt32(dr["isactive"]) == 1;

                    if (isActive) return false; // Active user exists

                    // Restore inactive user
                    dr.Close();
                    string salt = Guid.NewGuid().ToString("N");
                    using var upd = new SQLiteCommand(@"
                        UPDATE tblUser
                        SET isactive=1, password=@pass, salt=@salt, role=@role, name=@name
                        WHERE username=@user
                    ", cn);
                    upd.Parameters.AddWithValue("@salt", salt);
                    upd.Parameters.AddWithValue("@pass", DBConnection.GetHash(password, salt));
                    upd.Parameters.AddWithValue("@role", role);
                    upd.Parameters.AddWithValue("@name", name);
                    upd.Parameters.AddWithValue("@user", username);
                    await upd.ExecuteNonQueryAsync();
                    return true;
                }
            }

            // Insert new user
            string newSalt = Guid.NewGuid().ToString("N");
            using var ins = new SQLiteCommand(@"
                INSERT INTO tblUser (username, password, salt, role, name, isactive)
                VALUES (@user, @pass, @salt, @role, @name, 1)
            ", cn);
            ins.Parameters.AddWithValue("@user", username);
            ins.Parameters.AddWithValue("@salt", newSalt);
            ins.Parameters.AddWithValue("@pass", DBConnection.GetHash(password, newSalt));
            ins.Parameters.AddWithValue("@role", role);
            ins.Parameters.AddWithValue("@name", name);
            await ins.ExecuteNonQueryAsync();

            return true;
        }

        // ===============================
        // CHANGE PASSWORD
        // ===============================
        public static async Task<bool> ChangePasswordAsync(string username, string oldPass, string newPass)
        {
            using var cn = new SQLiteConnection(DBConnection.MyConnection());
            await cn.OpenAsync();

            using var cmd = new SQLiteCommand("SELECT password, salt FROM tblUser WHERE username=@user AND isactive=1", cn);
            cmd.Parameters.AddWithValue("@user", username);

            using var dr = await cmd.ExecuteReaderAsync();
            if (await dr.ReadAsync())
            {
                string dbHash = dr["password"].ToString();
                string salt = dr["salt"].ToString();

                if (DBConnection.GetHash(oldPass, salt) != dbHash) return false;

                dr.Close();
                string newSalt = Guid.NewGuid().ToString("N");
                using var upd = new SQLiteCommand("UPDATE tblUser SET password=@pass, salt=@salt WHERE username=@user", cn);
                upd.Parameters.AddWithValue("@pass", DBConnection.GetHash(newPass, newSalt));
                upd.Parameters.AddWithValue("@salt", newSalt);
                upd.Parameters.AddWithValue("@user", username);
                await upd.ExecuteNonQueryAsync();
                return true;
            }

            return false;
        }

        // ===============================
        // GET USER STATUS
        // ===============================
        public static async Task<bool> GetUserStatusAsync(string username)
        {
            using var cn = new SQLiteConnection(DBConnection.MyConnection());
            await cn.OpenAsync();

            using var cmd = new SQLiteCommand("SELECT isactive FROM tblUser WHERE username=@user", cn);
            cmd.Parameters.AddWithValue("@user", username);
            object result = await cmd.ExecuteScalarAsync();

            return result != null && Convert.ToInt32(result) == 1;
        }

        // ===============================
        // UPDATE USER STATUS
        // ===============================
        public static async Task<bool> UpdateStatusAsync(string username, bool status)
        {
            using var cn = new SQLiteConnection(DBConnection.MyConnection());
            await cn.OpenAsync();

            using var cmd = new SQLiteCommand("UPDATE tblUser SET isactive=@status WHERE username=@user", cn);
            cmd.Parameters.AddWithValue("@status", status ? 1 : 0);
            cmd.Parameters.AddWithValue("@user", username);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        // ===============================
        // DELETE USER (soft-delete by setting isactive=0)
        // ===============================
        public static async Task<bool> DeleteUserAsync(string username)
        {
            using var cn = new SQLiteConnection(DBConnection.MyConnection());
            await cn.OpenAsync();

            using var cmd = new SQLiteCommand("UPDATE tblUser SET isactive=0 WHERE username=@user", cn);
            cmd.Parameters.AddWithValue("@user", username);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        // ===============================
        // LOAD ALL USERS (for DataGridView)
        // ===============================
        public static async Task<DataTable> LoadAllUsersAsync()
        {
            var dt = new DataTable();
            using var cn = new SQLiteConnection(DBConnection.MyConnection());
            await cn.OpenAsync();

            using var cmd = new SQLiteCommand("SELECT username, name, role, isactive FROM tblUser", cn);
            using var dr = await cmd.ExecuteReaderAsync();
            dt.Load(dr);

            return dt;
        }
    }
}