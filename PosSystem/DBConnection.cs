using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace PosSystem
{
    public static class DBConnection
    {
        private static readonly string dbDirectory = Path.Combine(Application.StartupPath, "DATABASE");
        private static readonly string dbPath = Path.Combine(dbDirectory, "PosDB.db");

        // ULTRA PERFORMANCE CONNECTION STRING
        private static readonly string con = $"Data Source={dbPath};Version=3;Journal Mode=WAL;Pooling=True;Cache=Shared;Busy Timeout=5000;Page Size=32768;Synchronous=Normal;";
        private static bool _initialized = false;
        private static readonly object _lock = new object();

        // Keep same public field name for compatibility with old code
        public static SQLiteConnection cn = new SQLiteConnection(con);

        public static string MyConnection() => con;

        // SECURE HASH
        public static string GetHash(string password, string salt = "")
        {
            password = password ?? string.Empty;
            salt = salt ?? string.Empty;
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password + salt));
                StringBuilder sb = new StringBuilder(bytes.Length * 2);
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        // Update the method signature in DBConnection.cs
        public static void CloseAll(bool isShuttingDown = false)
        {
            try
            {
                if (cn != null)
                {
                    try { if (cn.State != ConnectionState.Closed) cn.Close(); } catch { }
                    try { cn.Dispose(); } catch { }
                }
                try { SQLiteConnection.ClearAllPools(); } catch { }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // ONLY recreate if we are not shutting down the app
                if (!isShuttingDown)
                {
                    cn = new SQLiteConnection(con);
                }
            }
            catch (Exception ex)
            {
                LogSilent(ex, "DBConnection_CloseAll");
                if (!isShuttingDown)
                {
                    try { cn = new SQLiteConnection(con); } catch { }
                }
            }
        }

        public static void InitializeDatabase()
        {
            if (_initialized) return;
            lock (_lock)
            {
                if (_initialized) return;
                try
                {
                    if (!Directory.Exists(dbDirectory))
                        Directory.CreateDirectory(dbDirectory);
                    if (!File.Exists(dbPath))
                        SQLiteConnection.CreateFile(dbPath);

                    using (var initCn = new SQLiteConnection(MyConnection()))
                    {
                        initCn.Open();
                        ApplyPragmas(initCn);
                        CreateCoreSchema(initCn);

                        // ===============================
                        // VERSIONING AND MIGRATION SYSTEM
                        // ===============================
                        EnsureVersioning(initCn);
                        MigrateDatabase(initCn);

                        // FIX SCHEMA & SEED DATA
                        FixSchema(initCn);
                        SeedData(initCn);

                        // ===============================
                        // INVENTORY AUTOMATION
                        // ===============================
                        using (var tr = initCn.BeginTransaction())
                        {
                            try
                            {
                                ExecuteNonQuery(initCn, "DROP VIEW IF EXISTS vwCriticalItems;", tr);
                                ExecuteNonQuery(initCn, @"
                                    CREATE VIEW IF NOT EXISTS vwCriticalItems AS
                                    SELECT p.pcode, p.barcode, p.pdesc, b.brand, c.category, p.qty, p.reorder
                                    FROM TblProduct1 p
                                    LEFT JOIN BrandTbl b ON p.bid = b.id
                                    LEFT JOIN TblCategory c ON p.cid = c.id
                                    WHERE p.qty <= p.reorder AND p.isactive = 1;", tr);

                                ExecuteNonQuery(initCn, "DROP TRIGGER IF EXISTS trg_after_sale;", tr);
                                ExecuteNonQuery(initCn, @"
                                    CREATE TRIGGER trg_after_sale AFTER UPDATE ON tblCart1
                                    FOR EACH ROW WHEN NEW.status = 'Sold' AND OLD.status = 'Pending'
                                    BEGIN
                                        UPDATE TblProduct1 SET qty = qty - NEW.qty WHERE pcode = NEW.pcode;
                                    END;", tr);
                                tr.Commit();
                            }
                            catch
                            {
                                try { tr.Rollback(); } catch { }
                                throw;
                            }
                        }
                    }
                    _initialized = true;
                }
                catch (Exception ex)
                {
                    LogSilent(ex, "DBConnection_InitializeDatabase");
                    MessageBox.Show("Critical Database Error:\n" + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ===============================
        // VERSIONING METHODS
        // ===============================

        private static void EnsureVersioning(SQLiteConnection cn)
        {
            ExecuteNonQuery(cn, "CREATE TABLE IF NOT EXISTS tblDBVersion (version INTEGER);");
            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM tblDBVersion;", cn))
            {
                object result = cmd.ExecuteScalar();
                if (Convert.ToInt32(result) == 0)
                {
                    ExecuteNonQuery(cn, "INSERT INTO tblDBVersion (version) VALUES (1);");
                }
            }
        }

        private static int GetDBVersion(SQLiteConnection cn)
        {
            using (var cmd = new SQLiteCommand("SELECT version FROM tblDBVersion LIMIT 1;", cn))
            {
                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value) return 1;
                return Convert.ToInt32(result);
            }
        }

        private static void SetDBVersion(SQLiteConnection cn, int version)
        {
            using (var cmd = new SQLiteCommand("UPDATE tblDBVersion SET version=@version;", cn))
            {
                cmd.Parameters.AddWithValue("@version", version);
                cmd.ExecuteNonQuery();
            }
        }

        private static bool TableExists(SQLiteConnection cn, string table)
        {
            using (var cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name=@t;", cn))
            {
                cmd.Parameters.AddWithValue("@t", table);
                return cmd.ExecuteScalar() != null;
            }
        }

        private static void MigrateDatabase(SQLiteConnection cn)
        {
            int dbVersion = GetDBVersion(cn);

            if (dbVersion < 2)
            {
                if (TableExists(cn, "tblUser") && !ColumnExists(cn, "tblUser", "role"))
                    ExecuteNonQuery(cn, "ALTER TABLE tblUser ADD COLUMN role TEXT;");
                SetDBVersion(cn, 2);
                dbVersion = 2;
            }

            if (dbVersion < 3)
            {
                if (TableExists(cn, "tblCancel") && !ColumnExists(cn, "tblCancel", "total"))
                    ExecuteNonQuery(cn, "ALTER TABLE tblCancel ADD COLUMN total REAL DEFAULT 0;");
                SetDBVersion(cn, 3);
                dbVersion = 3;
            }

            if (dbVersion < 4)
            {
                using (var tr = cn.BeginTransaction())
                {
                    try
                    {
                        ExecuteNonQuery(cn, "DROP VIEW IF EXISTS vwSoldItems;", tr);
                        ExecuteNonQuery(cn, @"
                            CREATE VIEW IF NOT EXISTS vwSoldItems AS
                            SELECT c.id, c.transno, c.pcode, p.pdesc, c.price, c.qty, c.disc, c.total, c.sdate, c.status, c.[user]
                            FROM tblCart1 c
                            INNER JOIN TblProduct1 p ON c.pcode = p.pcode;", tr);
                        tr.Commit();
                        SetDBVersion(cn, 4);
                        dbVersion = 4;
                    }
                    catch
                    {
                        try { tr.Rollback(); } catch { }
                        throw;
                    }
                }
            }

            if (dbVersion < 5)
            {
                if (TableExists(cn, "tblUser") && ColumnExists(cn, "tblUser", "isactive"))
                    ExecuteNonQuery(cn, "UPDATE tblUser SET isactive = 1 WHERE isactive IS NULL;");
                if (TableExists(cn, "TblProduct1") && ColumnExists(cn, "TblProduct1", "isactive"))
                    ExecuteNonQuery(cn, "UPDATE TblProduct1 SET isactive = 1 WHERE isactive IS NULL;");
                SetDBVersion(cn, 5);
                dbVersion = 5;
            }

            if (dbVersion < 6)
            {
                using (var tr = cn.BeginTransaction())
                {
                    try
                    {
                        ExecuteNonQuery(cn, "DROP VIEW IF EXISTS vwSoldItems;", tr);
                        ExecuteNonQuery(cn, @"
                            CREATE VIEW vwSoldItems AS
                            SELECT c.id, c.transno, c.pcode, p.pdesc, c.price, c.qty, c.disc, c.total, c.sdate, c.status, c.[user] as cashier
                            FROM tblCart1 c
                            INNER JOIN TblProduct1 p ON c.pcode = p.pcode;", tr);
                        ExecuteNonQuery(cn, "CREATE INDEX IF NOT EXISTS idx_cart_sdate ON tblCart1(sdate);", tr);
                        tr.Commit();
                        SetDBVersion(cn, 6);
                        dbVersion = 6;
                    }
                    catch
                    {
                        try { tr.Rollback(); } catch { }
                        throw;
                    }
                }
            }

            // v7 - cart discount compatibility for old DBs
            if (dbVersion < 7)
            {
                using (var tr = cn.BeginTransaction())
                {
                    try
                    {
                        if (TableExists(cn, "tblCart1") && !ColumnExists(cn, "tblCart1", "discount_per_unit"))
                            ExecuteNonQuery(cn, "ALTER TABLE tblCart1 ADD COLUMN discount_per_unit REAL DEFAULT 0;", tr);
                        if (TableExists(cn, "tblCart1") && ColumnExists(cn, "tblCart1", "discount_per_unit"))
                            ExecuteNonQuery(cn, "UPDATE tblCart1 SET discount_per_unit = 0 WHERE discount_per_unit IS NULL;", tr);
                        tr.Commit();
                        SetDBVersion(cn, 7);
                        dbVersion = 7;
                    }
                    catch
                    {
                        try { tr.Rollback(); } catch { }
                        throw;
                    }
                }
            }

            // v8 - normalize tblStore for old/new structure compatibility
            if (dbVersion < 8)
            {
                using (var tr = cn.BeginTransaction())
                {
                    try
                    {
                        if (TableExists(cn, "tblStore"))
                        {
                            if (!ColumnExists(cn, "tblStore", "store"))
                                ExecuteNonQuery(cn, "ALTER TABLE tblStore ADD COLUMN store TEXT;", tr);
                            if (!ColumnExists(cn, "tblStore", "address"))
                                ExecuteNonQuery(cn, "ALTER TABLE tblStore ADD COLUMN address TEXT;", tr);
                            if (!ColumnExists(cn, "tblStore", "phone"))
                                ExecuteNonQuery(cn, "ALTER TABLE tblStore ADD COLUMN phone TEXT;", tr);

                            if (ColumnExists(cn, "tblStore", "storename") && ColumnExists(cn, "tblStore", "store"))
                            {
                                ExecuteNonQuery(cn, @"
                                    UPDATE tblStore SET store = storename
                                    WHERE (store IS NULL OR TRIM(store) = '') AND storename IS NOT NULL AND TRIM(storename) <> '';", tr);
                            }
                            if (ColumnExists(cn, "tblStore", "phone"))
                                ExecuteNonQuery(cn, "UPDATE tblStore SET phone = '' WHERE phone IS NULL;", tr);
                        }
                        tr.Commit();
                        SetDBVersion(cn, 8);
                        dbVersion = 8;
                    }
                    catch
                    {
                        try { tr.Rollback(); } catch { }
                        throw;
                    }
                }
            }

            // v9 - normalize admin role strings for old installs
            if (dbVersion < 9)
            {
                using (var tr = cn.BeginTransaction())
                {
                    try
                    {
                        if (TableExists(cn, "tblUser") && ColumnExists(cn, "tblUser", "role"))
                        {
                            ExecuteNonQuery(cn, @"
                                UPDATE tblUser SET role = 'Administrator'
                                WHERE UPPER(TRIM(IFNULL(role,''))) IN ('ADMIN', 'SYSTEM ADMINISTRATOR');", tr);
                        }
                        tr.Commit();
                        SetDBVersion(cn, 9);
                        dbVersion = 9;
                    }
                    catch
                    {
                        try { tr.Rollback(); } catch { }
                        throw;
                    }
                }
            }

            // v10 - tblCart1 user/cashier compatibility bridge + final vwSoldItems rebuild
            if (dbVersion < 10)
            {
                using (var tr = cn.BeginTransaction())
                {
                    try
                    {
                        if (TableExists(cn, "tblCart1"))
                        {
                            if (!ColumnExists(cn, "tblCart1", "user"))
                                ExecuteNonQuery(cn, "ALTER TABLE tblCart1 ADD COLUMN [user] TEXT;", tr);
                            if (!ColumnExists(cn, "tblCart1", "cashier"))
                                ExecuteNonQuery(cn, "ALTER TABLE tblCart1 ADD COLUMN cashier TEXT;", tr);

                            if (ColumnExists(cn, "tblCart1", "user") && ColumnExists(cn, "tblCart1", "cashier"))
                            {
                                ExecuteNonQuery(cn, @"
                                    UPDATE tblCart1 SET [user] = cashier
                                    WHERE ([user] IS NULL OR TRIM([user]) = '') AND cashier IS NOT NULL AND TRIM(cashier) <> '';", tr);
                                ExecuteNonQuery(cn, @"
                                    UPDATE tblCart1 SET cashier = [user]
                                    WHERE (cashier IS NULL OR TRIM(cashier) = '') AND [user] IS NOT NULL AND TRIM([user]) <> '';", tr);
                            }

                            ExecuteNonQuery(cn, "DROP TRIGGER IF EXISTS trg_tblCart1_sync_user_cashier_ai;", tr);
                            ExecuteNonQuery(cn, "DROP TRIGGER IF EXISTS trg_tblCart1_sync_user_cashier_au;", tr);

                            ExecuteNonQuery(cn, @"
                                CREATE TRIGGER trg_tblCart1_sync_user_cashier_ai AFTER INSERT ON tblCart1
                                FOR EACH ROW
                                BEGIN
                                    UPDATE tblCart1 SET
                                    [user] = CASE WHEN (NEW.[user] IS NULL OR TRIM(NEW.[user]) = '') AND NEW.cashier IS NOT NULL AND TRIM(NEW.cashier) <> '' THEN NEW.cashier ELSE [user] END,
                                    cashier = CASE WHEN (NEW.cashier IS NULL OR TRIM(NEW.cashier) = '') AND NEW.[user] IS NOT NULL AND TRIM(NEW.[user]) <> '' THEN NEW.[user] ELSE cashier END
                                    WHERE id = NEW.id;
                                END;", tr);

                            ExecuteNonQuery(cn, @"
                                CREATE TRIGGER trg_tblCart1_sync_user_cashier_au AFTER UPDATE ON tblCart1
                                FOR EACH ROW
                                BEGIN
                                    UPDATE tblCart1 SET
                                    [user] = CASE WHEN (NEW.[user] IS NULL OR TRIM(NEW.[user]) = '') AND NEW.cashier IS NOT NULL AND TRIM(NEW.cashier) <> '' THEN NEW.cashier ELSE [user] END,
                                    cashier = CASE WHEN (NEW.cashier IS NULL OR TRIM(NEW.cashier) = '') AND NEW.[user] IS NOT NULL AND TRIM(NEW.[user]) <> '' THEN NEW.[user] ELSE cashier END
                                    WHERE id = NEW.id;
                                END;", tr);

                            ExecuteNonQuery(cn, "DROP VIEW IF EXISTS vwSoldItems;", tr);
                            ExecuteNonQuery(cn, @"
                                CREATE VIEW vwSoldItems AS
                                SELECT c.id, c.transno, c.pcode, p.pdesc, c.price, c.qty, c.disc, c.total, c.sdate, c.status, c.[user] AS [user], c.cashier AS cashier,
                                IFNULL(
                                    c.discount_per_unit,
                                    CASE WHEN IFNULL(c.qty, 0) > 0 THEN IFNULL(c.disc, 0) * 1.0 / c.qty ELSE 0 END
                                ) AS discount_per_unit
                                FROM tblCart1 c
                                INNER JOIN TblProduct1 p ON c.pcode = p.pcode;", tr);

                            ExecuteNonQuery(cn, "CREATE INDEX IF NOT EXISTS idx_cart_sdate ON tblCart1(sdate);", tr);
                        }
                        tr.Commit();
                        SetDBVersion(cn, 10);
                    }
                    catch
                    {
                        try { tr.Rollback(); } catch { }
                        throw;
                    }
                }
            }

            // v11 - stock movement record compatibility (record-only fields, no product table changes)
            if (dbVersion < 11)
            {
                using (var tr = cn.BeginTransaction())
                {
                    try
                    {
                        if (TableExists(cn, "tblStockIn"))
                        {
                            if (!ColumnExists(cn, "tblStockIn", "stime"))
                                ExecuteNonQuery(cn, "ALTER TABLE tblStockIn ADD COLUMN stime TEXT;", tr);

                            if (!ColumnExists(cn, "tblStockIn", "action"))
                                ExecuteNonQuery(cn, "ALTER TABLE tblStockIn ADD COLUMN action TEXT DEFAULT 'ADD';", tr);

                            if (!ColumnExists(cn, "tblStockIn", "remarks"))
                                ExecuteNonQuery(cn, "ALTER TABLE tblStockIn ADD COLUMN remarks TEXT;", tr);

                            if (ColumnExists(cn, "tblStockIn", "action"))
                            {
                                ExecuteNonQuery(cn, @"
                        UPDATE tblStockIn
                        SET action = 'ADD'
                        WHERE action IS NULL OR TRIM(action) = '';", tr);
                            }

                            if (ColumnExists(cn, "tblStockIn", "remarks"))
                            {
                                ExecuteNonQuery(cn, @"
                        UPDATE tblStockIn
                        SET remarks = ''
                        WHERE remarks IS NULL;", tr);
                            }

                            if (ColumnExists(cn, "tblStockIn", "stime"))
                            {
                                ExecuteNonQuery(cn, @"
                        UPDATE tblStockIn
                        SET stime = CASE
                            WHEN stime IS NULL OR TRIM(stime) = '' THEN ''
                            ELSE stime
                        END;", tr);
                            }

                            ExecuteNonQuery(cn, "CREATE INDEX IF NOT EXISTS idx_tblStockIn_sdate_status ON tblStockIn(sdate, status);", tr);
                            ExecuteNonQuery(cn, "CREATE INDEX IF NOT EXISTS idx_tblStockIn_refno ON tblStockIn(refno);", tr);
                        }

                        ExecuteNonQuery(cn, @"
                CREATE TABLE IF NOT EXISTS tblAdjustment (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    referenceno TEXT,
                    pcode TEXT,
                    qty INTEGER,
                    action TEXT,
                    remarks TEXT,
                    sdate TEXT,
                    [user] TEXT
                );", tr);

                        ExecuteNonQuery(cn, "CREATE INDEX IF NOT EXISTS idx_tblAdjustment_sdate ON tblAdjustment(sdate);", tr);
                        ExecuteNonQuery(cn, "CREATE INDEX IF NOT EXISTS idx_tblAdjustment_pcode ON tblAdjustment(pcode);", tr);

                        ExecuteNonQuery(cn, "DROP VIEW IF EXISTS vwStockMovementHistory;", tr);
                        ExecuteNonQuery(cn, @"
                CREATE VIEW vwStockMovementHistory AS
                SELECT
                    s.id AS id,
                    s.refno AS refno,
                    s.pcode AS pcode,
                    p.pdesc AS pdesc,
                    IFNULL(s.action, 'ADD') AS action,
                    IFNULL(s.qty, 0) AS qty,
                    SUBSTR(IFNULL(s.sdate,''), 1, 10) AS sdate,
                    IFNULL(s.stime, '') AS stime,
                    IFNULL(s.stockinby, '') AS [user],
                    IFNULL(v.vendor, '') AS vendor,
                    IFNULL(s.remarks, '') AS remarks,
                    'tblStockIn' AS source_table
                FROM tblStockIn s
                LEFT JOIN TblProduct1 p ON p.pcode = s.pcode
                LEFT JOIN tblVendor v ON v.id = s.vendorid
                WHERE UPPER(TRIM(IFNULL(s.status,''))) = 'DONE'

                UNION ALL

                SELECT
                    a.id AS id,
                    a.referenceno AS refno,
                    a.pcode AS pcode,
                    p.pdesc AS pdesc,
                    IFNULL(a.action, '') AS action,
                    IFNULL(a.qty, 0) AS qty,
                    SUBSTR(IFNULL(a.sdate,''), 1, 10) AS sdate,
                    CASE
                        WHEN LENGTH(IFNULL(a.sdate,'')) >= 16 THEN SUBSTR(a.sdate, 12, 5)
                        ELSE ''
                    END AS stime,
                    IFNULL(a.[user], '') AS [user],
                    '' AS vendor,
                    IFNULL(a.remarks, '') AS remarks,
                    'tblAdjustment' AS source_table
                FROM tblAdjustment a
                LEFT JOIN TblProduct1 p ON p.pcode = a.pcode;", tr);

                        tr.Commit();
                        SetDBVersion(cn, 11);
                        dbVersion = 11;
                    }
                    catch
                    {
                        try { tr.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        // ===============================
        // EXISTING FIX SCHEMA & HELPERS // ===============================

        private static void FixSchema(SQLiteConnection cn)
        {
            string[,] updates = {
                { "TblProduct1", "cost_price", "REAL DEFAULT 0" },
                { "TblProduct1", "tax_rate", "REAL DEFAULT 0" },
                { "TblProduct1", "sid", "INTEGER DEFAULT 0" },
                { "TblProduct1", "isactive", "INTEGER DEFAULT 1" },
                { "tblUser", "isactive", "INTEGER DEFAULT 1" }
            };

            for (int i = 0; i < updates.GetLength(0); i++)
            {
                string tableName = updates[i, 0];
                string columnName = updates[i, 1];
                string columnDefinition = updates[i, 2];

                if (TableExists(cn, tableName) && !ColumnExists(cn, tableName, columnName))
                {
                    using (var cmd = new SQLiteCommand($"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition};", cn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private static bool ColumnExists(SQLiteConnection cn, string tableName, string columnName)
        {
            using (var cmd = new SQLiteCommand($"PRAGMA table_info([{tableName}]);", cn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string currentColumn = reader["name"]?.ToString();
                    if (string.Equals(currentColumn, columnName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }

        private static void SeedData(SQLiteConnection cn)
        {
            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM tblVat;", cn))
            {
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    ExecuteNonQuery(cn, "INSERT INTO tblVat (vat) VALUES (0);");
            }

            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM tblUser;", cn))
            {
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                {
                    string salt = Guid.NewGuid().ToString("N");
                    using (var ins = new SQLiteCommand("INSERT INTO tblUser (username,password,salt,role,name,isactive) VALUES ('admin',@p,@s,'Administrator','System Admin',1)", cn))
                    {
                        ins.Parameters.AddWithValue("@s", salt);
                        ins.Parameters.AddWithValue("@p", GetHash("admin123", salt));
                        ins.ExecuteNonQuery();
                    }
                }
            }

            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM tblStore;", cn))
            {
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    ExecuteNonQuery(cn, "INSERT INTO tblStore (store,address,phone) VALUES ('Default Store','Default Address','0000000000');");
            }
        }

        // DASHBOARD
        public static double DailySales()
        {
            try
            {
                using (var cn = new SQLiteConnection(con))
                {
                    cn.Open();
                    using (var cm = new SQLiteCommand("SELECT IFNULL(SUM(total),0) FROM tblTransaction WHERE DATE(sdate)=DATE('now','localtime') AND status='Sold';", cn))
                    {
                        return Convert.ToDouble(cm.ExecuteScalar() ?? 0);
                    }
                }
            }
            catch (Exception ex)
            {
                LogSilent(ex, "DBConnection_DailySales");
                return 0;
            }
        }

        public static double ProductLine()
        {
            try
            {
                using (var cn = new SQLiteConnection(con))
                {
                    cn.Open();
                    using (var cm = new SQLiteCommand("SELECT COUNT(*) FROM TblProduct1 WHERE isactive=1;", cn))
                    {
                        return Convert.ToDouble(cm.ExecuteScalar() ?? 0);
                    }
                }
            }
            catch (Exception ex)
            {
                LogSilent(ex, "DBConnection_ProductLine");
                return 0;
            }
        }

        public static double StockOnHand()
        {
            try
            {
                using (var cn = new SQLiteConnection(con))
                {
                    cn.Open();
                    using (var cm = new SQLiteCommand("SELECT IFNULL(SUM(qty),0) FROM TblProduct1 WHERE isactive=1;", cn))
                    {
                        return Convert.ToDouble(cm.ExecuteScalar() ?? 0);
                    }
                }
            }
            catch (Exception ex)
            {
                LogSilent(ex, "DBConnection_StockOnHand");
                return 0;
            }
        }

        public static double CriticalItems()
        {
            try
            {
                using (var cn = new SQLiteConnection(con))
                {
                    cn.Open();
                    using (var cm = new SQLiteCommand("SELECT COUNT(*) FROM vwCriticalItems;", cn))
                    {
                        return Convert.ToDouble(cm.ExecuteScalar() ?? 0);
                    }
                }
            }
            catch (Exception ex)
            {
                LogSilent(ex, "DBConnection_CriticalItems");
                return 0;
            }
        }

        private static void ApplyPragmas(SQLiteConnection cn)
        {
            ExecuteNonQuery(cn, "PRAGMA foreign_keys = ON;");
            ExecuteNonQuery(cn, "PRAGMA synchronous = NORMAL;");
            ExecuteNonQuery(cn, "PRAGMA journal_mode = WAL;");
            ExecuteNonQuery(cn, "PRAGMA temp_store = MEMORY;");
            ExecuteNonQuery(cn, "PRAGMA cache_size = -20000;");
        }

        private static void CreateCoreSchema(SQLiteConnection cn)
        {
            string script = @"
                CREATE TABLE IF NOT EXISTS BrandTbl (id INTEGER PRIMARY KEY AUTOINCREMENT, brand TEXT NOT NULL);
                CREATE TABLE IF NOT EXISTS TblCategory (id INTEGER PRIMARY KEY AUTOINCREMENT, category TEXT NOT NULL);
                CREATE TABLE IF NOT EXISTS tblStore (store TEXT, address TEXT, phone TEXT);
                CREATE TABLE IF NOT EXISTS tblVat (id INTEGER PRIMARY KEY AUTOINCREMENT, vat REAL DEFAULT 0);
                CREATE TABLE IF NOT EXISTS tblUser (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT UNIQUE,
                    password TEXT,
                    salt TEXT,
                    role TEXT,
                    name TEXT,
                    isactive INTEGER DEFAULT 1
                );
                CREATE TABLE IF NOT EXISTS tblVendor (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    vendor TEXT,
                    address TEXT,
                    contactperson TEXT,
                    telephone TEXT,
                    email TEXT,
                    fax TEXT
                );
                CREATE TABLE IF NOT EXISTS tblStockIn (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                refno TEXT,
                pcode TEXT,
                qty INTEGER,
                cost REAL,
                sdate TEXT,
                stime TEXT,
                stockinby TEXT,
                vendorid INTEGER,
                action TEXT DEFAULT 'ADD',
                remarks TEXT,
                status TEXT DEFAULT 'Pending'
                );
                CREATE TABLE IF NOT EXISTS tblAdjustment (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                referenceno TEXT,
                pcode TEXT,
                qty INTEGER,
                action TEXT,
                remarks TEXT,
                sdate TEXT,
                [user] TEXT
                );
                CREATE TABLE IF NOT EXISTS TblProduct1 (
                    pcode TEXT PRIMARY KEY,
                    barcode TEXT,
                    pdesc TEXT NOT NULL,
                    bid INTEGER,
                    cid INTEGER,
                    price REAL DEFAULT 0,
                    cost_price REAL DEFAULT 0,
                    tax_rate REAL DEFAULT 0,
                    sid INTEGER DEFAULT 0,
                    qty INTEGER DEFAULT 0,
                    reorder INTEGER DEFAULT 0,
                    isactive INTEGER DEFAULT 1,
                    FOREIGN KEY (bid) REFERENCES BrandTbl(id),
                    FOREIGN KEY (cid) REFERENCES TblCategory(id)
                );
                CREATE TABLE IF NOT EXISTS tblTransaction (
                    transno TEXT PRIMARY KEY,
                    sdate TEXT,
                    subtotal REAL DEFAULT 0,
                    discount REAL DEFAULT 0,
                    vat REAL DEFAULT 0,
                    total REAL DEFAULT 0,
                    payment_type TEXT,
                    cash_tendered REAL DEFAULT 0,
                    cash_change REAL DEFAULT 0,
                    user_id TEXT,
                    status TEXT DEFAULT 'Sold'
                );
                CREATE TABLE IF NOT EXISTS tblCart1 (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    transno TEXT,
                    pcode TEXT,
                    price REAL,
                    qty INTEGER,
                    sdate TEXT,
                    status TEXT DEFAULT 'Pending',
                    disc REAL DEFAULT 0,
                    total REAL DEFAULT 0,
                    [user] TEXT,
                    FOREIGN KEY (pcode) REFERENCES TblProduct1(pcode),
                    FOREIGN KEY (transno) REFERENCES tblTransaction(transno) ON DELETE CASCADE
                );
                CREATE TABLE IF NOT EXISTS tblCancel (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    transno TEXT,
                    pcode TEXT,
                    price REAL,
                    qty INTEGER,
                    total REAL,
                    sdate TEXT,
                    voidby TEXT,
                    cancelledby TEXT,
                    reason TEXT,
                    action TEXT
                );
                CREATE INDEX IF NOT EXISTS idx_product_barcode ON TblProduct1(barcode);
                CREATE INDEX IF NOT EXISTS idx_cart_transno ON tblCart1(transno);";

            using (var cm = new SQLiteCommand(script, cn))
            {
                cm.ExecuteNonQuery();
            }
        }

        private static void ExecuteNonQuery(SQLiteConnection cn, string sql, SQLiteTransaction tr = null)
        {
            using (var cmd = new SQLiteCommand(sql, cn, tr))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static void LogSilent(Exception ex, string location)
        {
            try
            {
                Program.SilentLog(ex, location);
            }
            catch
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine(location + ": " + ex.Message);
                }
                catch { }
            }
        }
    }
}