using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace PosSystem
{
    public partial class frmRecentTransactions : Form
    {
        public string SelectedTransNo { get; private set; }

        public frmRecentTransactions()
        {
            InitializeComponent();
            LoadRecentTransactions();
        }

        private void LoadRecentTransactions()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("transno", typeof(string));
                dt.Columns.Add("sale_time_display", typeof(string));
                dt.Columns.Add("cashier", typeof(string));
                dt.Columns.Add("grand_total", typeof(decimal));
                dt.Columns.Add("sort_id", typeof(long));

                using (SQLiteConnection cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    cn.Open();

                    string sql = @"
                        SELECT 
                            c.transno,
                            MAX(c.sdate) AS sale_time_raw,
                            COALESCE(
                                MAX(CASE WHEN TRIM(IFNULL(c.cashier,'')) <> '' THEN c.cashier END),
                                MAX(CASE WHEN TRIM(IFNULL(c.[user],'')) <> '' THEN c.[user] END),
                                ''
                            ) AS cashier,
                            ROUND(SUM(IFNULL(c.total, 0)), 2) AS grand_total,
                            MAX(c.id) AS sort_id
                        FROM tblCart1 c
                        WHERE c.status = 'Sold'
                          AND SUBSTR(IFNULL(c.sdate,''), 1, 10) = DATE('now', 'localtime')
                        GROUP BY c.transno
                        ORDER BY sort_id DESC;";

                    using (SQLiteCommand cmd = new SQLiteCommand(sql, cn))
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string rawTime = dr["sale_time_raw"]?.ToString() ?? "";
                            string formattedTime = FormatSaleTime(rawTime);

                            DataRow row = dt.NewRow();
                            row["transno"] = dr["transno"]?.ToString() ?? "";
                            row["sale_time_display"] = formattedTime;
                            row["cashier"] = dr["cashier"]?.ToString() ?? "";
                            row["grand_total"] = dr["grand_total"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["grand_total"]);
                            row["sort_id"] = dr["sort_id"] == DBNull.Value ? 0L : Convert.ToInt64(dr["sort_id"]);
                            dt.Rows.Add(row);
                        }
                    }
                }

                dgv.DataSource = dt;

                if (dgv.Rows.Count > 0)
                    dgv.Rows[0].Selected = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load recent transactions: " + ex.Message,
                    "Reprint",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private string FormatSaleTime(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "";

            if (DateTime.TryParse(raw, out DateTime parsed))
                return parsed.ToString("yyyy-MM-dd hh:mm tt");

            return raw;
        }

        private void btnReprint_Click(object sender, EventArgs e)
        {
            ConfirmSelection();
        }

        private void dgv_DoubleClick(object sender, EventArgs e)
        {
            ConfirmSelection();
        }

        private void dgv_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                ConfirmSelection();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ConfirmSelection()
        {
            if (dgv.CurrentRow == null)
            {
                MessageBox.Show("Please select a transaction first.",
                    "Reprint",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            string transNo = dgv.CurrentRow.Cells["colTransNo"].Value?.ToString();
            if (string.IsNullOrWhiteSpace(transNo))
            {
                MessageBox.Show("Invalid transaction selected.",
                    "Reprint",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            SelectedTransNo = transNo;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}