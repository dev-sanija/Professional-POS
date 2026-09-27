using System;
using System.Data;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace PosSystem
{
    public static class DatabaseLoader
    {
        private static readonly Regex SafeTableNameRegex =
            new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        public static DataTable Load(string table)
        {
            try
            {
                string safeTable = GetSafeTableName(table);

                using (SQLiteConnection cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    cn.Open();

                    using (SQLiteDataAdapter da = new SQLiteDataAdapter($"SELECT * FROM [{safeTable}]", cn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "DatabaseLoader_Load");
                throw;
            }
        }

        public static DataTable LoadSchema(string table)
        {
            try
            {
                string safeTable = GetSafeTableName(table);

                using (SQLiteConnection cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    cn.Open();

                    using (SQLiteDataAdapter da = new SQLiteDataAdapter($"SELECT * FROM [{safeTable}] LIMIT 0", cn))
                    {
                        DataTable dt = new DataTable();
                        da.FillSchema(dt, SchemaType.Source);
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "DatabaseLoader_LoadSchema");
                throw;
            }
        }

        private static string GetSafeTableName(string table)
        {
            if (string.IsNullOrWhiteSpace(table))
                throw new ArgumentException("Table name cannot be empty.", nameof(table));

            string trimmed = table.Trim();

            if (!SafeTableNameRegex.IsMatch(trimmed))
                throw new ArgumentException("Invalid table name.", nameof(table));

            return trimmed;
        }
    }
}