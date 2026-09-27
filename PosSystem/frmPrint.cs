using Microsoft.Reporting.WinForms;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Runtime.InteropServices; // Added for draggable functionality
using System.Windows.Forms;

namespace PosSystem
{
    public partial class frmPrint : Form
    {
        // --- DRAGGABLE LOGIC (WinAPI) ---
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        // -------------------------------

        private DataTable _barcode;

        public frmPrint(DataTable barcode)
        {
            InitializeComponent();
            _barcode = barcode;
        }

        public frmPrint()
        {
            InitializeComponent();
        }

        private void frmPrint_Load(object sender, EventArgs e)
        {
            try
            {
                // Load barcode table dynamically if not supplied
                if (_barcode == null)
                {
                    using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                    {
                        cn.Open();
                        _barcode = new DataTable();
                        using (var da = new SQLiteDataAdapter("SELECT * FROM tblCart1", cn))
                        {
                            da.FillSchema(_barcode, SchemaType.Source);
                            da.Fill(_barcode);
                        }
                    }
                }

                // Build report path safely
                string reportPath = Path.Combine(Application.StartupPath, "Bill", "ReportBarcode.rdlc");

                // Check if the RDLC file exists
                if (!File.Exists(reportPath))
                {
                    MessageBox.Show("Report file not found at: " + reportPath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Set report path
                reportViewer1.LocalReport.ReportPath = reportPath;

                // Clear existing data sources
                reportViewer1.LocalReport.DataSources.Clear();

                // Add the barcode data table as the report data source
                ReportDataSource rds = new ReportDataSource("DataSet1", _barcode);
                reportViewer1.LocalReport.DataSources.Add(rds);

                // Configure viewer display
                reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
                reportViewer1.ZoomMode = ZoomMode.Percent;
                reportViewer1.ZoomPercent = 100;
                reportViewer1.ShowToolBar = true;

                // Refresh to render the report
                reportViewer1.RefreshReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Printing Error: " + ex.Message, "POS System", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void reportViewer1_Load(object sender, EventArgs e)
        {
            // Not required for V1, safe to leave empty
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}