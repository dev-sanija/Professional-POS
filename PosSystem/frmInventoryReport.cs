using Microsoft.Reporting.WinForms;
using System;
using System.Data;
using System.Data.SQLite;
using System.Runtime.InteropServices; // Added for draggable logic
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosSystem
{
    public partial class frmInventoryReport : Form
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

        public frmInventoryReport()
        {
            InitializeComponent();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void frmInventoryReport_Load(object sender, EventArgs e) { }
        private void reportViewer1_Load(object sender, EventArgs e) { }

        // ================================
        // PUBLIC REPORT METHODS
        // ================================

        public async void LoadSoldItems(string sql, string param)
        {
            DataTable dt = await LoadDataTableAsync(sql);
            SetReport("Bill\\ReportSold.rdlc", dt,
                      new ReportParameter("pDate", param));
        }

        public async void LoadTopSelling(string sql, string param, string header)
        {
            DataTable dt = await LoadDataTableAsync(sql);
            SetReport("Bill\\ReportTop.rdlc", dt,
                      new ReportParameter("pDate", param),
                      new ReportParameter("pHeader", header));
        }

        public async void LoadReport()
        {
            string query = @"
                SELECT p.pcode, p.barcode, p.pdesc, b.brand, c.category, p.price, p.qty, p.reorder
                FROM TblProduct1 AS p
                INNER JOIN BrandTbl AS b ON p.bid = b.id
                INNER JOIN TblCategory AS c ON p.cid = c.id";

            DataTable dt = await LoadDataTableAsync(query);
            SetReport("Bill\\Report3.rdlc", dt);
        }

        public async void LoadStockReport(string sql, string param)
        {
            DataTable dt = await LoadDataTableAsync(sql);
            SetReport("Bill\\ReportStock.rdlc", dt,
                      new ReportParameter("pDate", param));
        }

        public async void LoadCancelReport(string sql, string param)
        {
            DataTable dt = await LoadDataTableAsync(sql);
            SetReport("Bill\\ReportCancel.rdlc", dt,
                      new ReportParameter("pDate", param));
        }

        // ================================
        // PRIVATE HELPERS
        // ================================

        private async Task<DataTable> LoadDataTableAsync(string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                using (var da = new SQLiteDataAdapter(sql, cn))
                {
                    cn.Open();
                    dt.Clear();
                    da.FillSchema(dt, SchemaType.Source);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return dt;
        }

        private void SetReport(string rdlcPath, DataTable table, params ReportParameter[] parameters)
        {
            try
            {
                reportViewer1.LocalReport.ReportPath = Application.StartupPath + "\\" + rdlcPath;
                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", table));

                if (parameters != null)
                    reportViewer1.LocalReport.SetParameters(parameters);

                reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
                reportViewer1.ZoomMode = ZoomMode.Percent;
                reportViewer1.ZoomPercent = 100;
                reportViewer1.RefreshReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}