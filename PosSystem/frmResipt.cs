using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosSystem
{
    public partial class frmResipt : Form
    {
        private readonly Form1 f1;
        private readonly frmPOS fPOS;
        private IList<Stream> _reportStreams;
        private int _currentPageIndex;
        private bool _reportLoaded = false;

        public frmResipt(Form frm)
        {
            InitializeComponent();
            KeyPreview = true;

            if (frm is Form1 form1)
                f1 = form1;
            else if (frm is frmPOS formPOS)
                fPOS = formPOS;
            else
                throw new ArgumentException("Unsupported form type passed to frmResipt.");
        }

        private void frmResipt_Load(object sender, EventArgs e)
        {
            // Keep empty. Do not call RefreshReport() here.
        }

        // Existing live-sale method kept for compatibility
        public async void LoadReport(string pcash, string pchange, string cashierName)
        {
            string transNo = GetCurrentTransactionNo();
            await LoadReportCoreAsync(
                transNo,
                pcash,
                pchange,
                cashierName,
                useDbTransactionValues: false);
        }

        // New method for old completed transaction reprint
        public async void LoadReportByTransactionNo(string transNo)
        {
            await LoadReportCoreAsync(
                transNo,
                null,
                null,
                null,
                useDbTransactionValues: true);
        }

        private async Task LoadReportCoreAsync(
            string transNo,
            string liveCash,
            string liveChange,
            string liveCashierName,
            bool useDbTransactionValues)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(transNo))
                    throw new Exception("Transaction number is empty.");

                string reportFile = Path.Combine(Application.StartupPath, "Bill", "Report1.rdlc");
                if (!File.Exists(reportFile))
                    throw new FileNotFoundException("Report file not found: " + reportFile);

                DataTable dtSold = new DataTable();

                string storeName = string.Empty;
                string storeAddress = string.Empty;
                string storePhone = string.Empty;

                decimal subtotal = 0m;
                decimal discount = 0m;
                decimal vat = 0m;
                decimal total = 0m;
                decimal cashTendered = 0m;
                decimal cashChange = 0m;
                string cashier = string.Empty;

                await Task.Run(() =>
                {
                    using (SQLiteConnection cn = new SQLiteConnection(DBConnection.MyConnection()))
                    {
                        cn.Open();

                        // 1. Store info
                        using (SQLiteCommand cmdStore = new SQLiteCommand("SELECT store, address, phone FROM tblStore LIMIT 1", cn))
                        using (SQLiteDataReader drStore = cmdStore.ExecuteReader())
                        {
                            if (drStore.Read())
                            {
                                storeName = drStore["store"]?.ToString() ?? "";
                                storeAddress = drStore["address"]?.ToString() ?? "";
                                storePhone = drStore["phone"]?.ToString() ?? "";
                            }
                        }

                        // 2. Sold line items
                        string sqlItems = @"
                            SELECT 
                                c.id,
                                c.transno,
                                c.pcode,
                                CAST(c.price AS DOUBLE) AS price,
                                CAST(c.qty AS DOUBLE) AS qty,
                                CAST(c.disc AS DOUBLE) AS disc,
                                CAST(c.total AS DOUBLE) AS total,
                                c.sdate,
                                c.status,
                                p.pdesc
                            FROM tblCart1 c
                            INNER JOIN TblProduct1 p ON p.pcode = c.pcode
                            WHERE c.transno = @transno
                            ORDER BY c.id;";

                        using (SQLiteCommand cmdItems = new SQLiteCommand(sqlItems, cn))
                        {
                            cmdItems.Parameters.AddWithValue("@transno", transNo);
                            using (SQLiteDataAdapter da = new SQLiteDataAdapter(cmdItems))
                            {
                                da.FillSchema(dtSold, SchemaType.Source);
                                da.Fill(dtSold);
                            }
                        }

                        // 3. Transaction summary
                        string sqlTxn = @"
                            SELECT 
                                subtotal,
                                discount,
                                vat,
                                total,
                                cash_tendered,
                                cash_change,
                                user_id
                            FROM tblTransaction
                            WHERE transno = @transno
                            LIMIT 1;";

                        using (SQLiteCommand cmdTxn = new SQLiteCommand(sqlTxn, cn))
                        {
                            cmdTxn.Parameters.AddWithValue("@transno", transNo);

                            using (SQLiteDataReader drTxn = cmdTxn.ExecuteReader())
                            {
                                if (drTxn.Read())
                                {
                                    subtotal = drTxn["subtotal"] == DBNull.Value ? 0m : Convert.ToDecimal(drTxn["subtotal"]);
                                    discount = drTxn["discount"] == DBNull.Value ? 0m : Convert.ToDecimal(drTxn["discount"]);
                                    vat = drTxn["vat"] == DBNull.Value ? 0m : Convert.ToDecimal(drTxn["vat"]);
                                    total = drTxn["total"] == DBNull.Value ? 0m : Convert.ToDecimal(drTxn["total"]);
                                    cashTendered = drTxn["cash_tendered"] == DBNull.Value ? 0m : Convert.ToDecimal(drTxn["cash_tendered"]);
                                    cashChange = drTxn["cash_change"] == DBNull.Value ? 0m : Convert.ToDecimal(drTxn["cash_change"]);
                                    cashier = drTxn["user_id"]?.ToString() ?? "";
                                }
                            }
                        }

                        // 4. Cashier fallback from cart rows if needed
                        if (string.IsNullOrWhiteSpace(cashier))
                        {
                            string sqlCashier = @"
                                SELECT COALESCE(
                                    MAX(CASE WHEN TRIM(IFNULL(c.cashier,'')) <> '' THEN c.cashier END),
                                    MAX(CASE WHEN TRIM(IFNULL(c.[user],'')) <> '' THEN c.[user] END),
                                    ''
                                )
                                FROM tblCart1 c
                                WHERE c.transno = @transno;";

                            using (SQLiteCommand cmdCashier = new SQLiteCommand(sqlCashier, cn))
                            {
                                cmdCashier.Parameters.AddWithValue("@transno", transNo);
                                cashier = cmdCashier.ExecuteScalar()?.ToString() ?? "";
                            }
                        }

                        // 5. Fallback totals if tblTransaction row is missing
                        if (total <= 0m && dtSold.Rows.Count > 0)
                        {
                            decimal computedTotal = 0m;
                            decimal computedDiscount = 0m;

                            foreach (DataRow row in dtSold.Rows)
                            {
                                computedTotal += row["total"] == DBNull.Value ? 0m : Convert.ToDecimal(row["total"]);
                                computedDiscount += row["disc"] == DBNull.Value ? 0m : Convert.ToDecimal(row["disc"]);
                            }

                            total = computedTotal;
                            discount = computedDiscount;
                            subtotal = computedTotal + computedDiscount;
                        }
                    }
                });

                // Live POS compatibility values
                if (!useDbTransactionValues)
                {
                    if (fPOS != null)
                    {
                        if (string.IsNullOrWhiteSpace(storeName)) storeName = fPOS.lblSname.Text ?? "";
                        if (string.IsNullOrWhiteSpace(storeAddress)) storeAddress = fPOS.lblAddress.Text ?? "";
                        if (string.IsNullOrWhiteSpace(storePhone)) storePhone = fPOS.lblPhone.Text ?? "";

                        if (total <= 0m) decimal.TryParse((fPOS.lblTotal.Text ?? "0").Replace(",", ""), out total);
                        if (discount <= 0m) decimal.TryParse((fPOS.lblDiscount.Text ?? "0").Replace(",", ""), out discount);
                    }
                    else if (f1 != null)
                    {
                        if (string.IsNullOrWhiteSpace(storeName)) storeName = f1.lblSname.Text ?? "";
                        if (string.IsNullOrWhiteSpace(storeAddress)) storeAddress = f1.lblAddress.Text ?? "";
                        if (string.IsNullOrWhiteSpace(storePhone)) storePhone = f1.lblPhone.Text ?? "";

                        if (total <= 0m) decimal.TryParse((f1.lblTotal.Text ?? "0").Replace(",", ""), out total);
                        if (discount <= 0m) decimal.TryParse((f1.lblDiscount.Text ?? "0").Replace(",", ""), out discount);
                    }

                    if (!string.IsNullOrWhiteSpace(liveCash)) decimal.TryParse(liveCash.Replace(",", ""), out cashTendered);
                    if (!string.IsNullOrWhiteSpace(liveChange)) decimal.TryParse(liveChange.Replace(",", ""), out cashChange);
                    if (!string.IsNullOrWhiteSpace(liveCashierName)) cashier = liveCashierName;
                }

                reportViewer1.Reset();
                reportViewer1.ProcessingMode = ProcessingMode.Local;
                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.ReportPath = reportFile;

                ReportParameter[] parameters = new ReportParameter[]
                {
                    new ReportParameter("pPhone", storePhone ?? ""),
                    new ReportParameter("pTotal", total.ToString("N2")),
                    new ReportParameter("pCash", cashTendered.ToString("N2")),
                    new ReportParameter("pDiscount", discount.ToString("N2")),
                    new ReportParameter("pChange", cashChange.ToString("N2")),
                    new ReportParameter("pStore", storeName ?? ""),
                    new ReportParameter("pAddress", storeAddress ?? ""),
                    new ReportParameter("pTransaction", "Invoice #: " + transNo),
                    new ReportParameter("pCashier", string.IsNullOrWhiteSpace(cashier) ? "N/A" : cashier)
                };

                reportViewer1.LocalReport.SetParameters(parameters);
                reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", dtSold));

                reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
                reportViewer1.ZoomMode = ZoomMode.Percent;
                reportViewer1.ZoomPercent = 100;
                reportViewer1.RefreshReport();
                _reportLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load receipt: " + ex.Message,
                    "Receipt Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private string GetCurrentTransactionNo()
        {
            if (fPOS != null)
                return fPOS.lblTransno.Text;
            if (f1 != null)
                return f1.lblTransno.Text;

            return string.Empty;
        }

        private void reportViewer1_Load(object sender, EventArgs e)
        {
            // Keep empty
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }

            if (keyData == Keys.Enter)
            {
                if (_reportLoaded)
                {
                    PrintLoadedReportToSelectedPrinter();
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private string ResolvePrinterNameForReceipt()
        {
            try
            {
                if (fPOS != null)
                    return fPOS.ResolvePrinterNameForPrinting();

                return new PrinterSettings().PrinterName;
            }
            catch
            {
                return new PrinterSettings().PrinterName;
            }
        }

        private bool IsVirtualPrinter(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName))
                return true;

            string[] virtualKeywords = { "pdf", "xps", "onenote", "fax" };
            return virtualKeywords.Any(k => printerName.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void PrintLoadedReportToSelectedPrinter()
        {
            try
            {
                string printerName = ResolvePrinterNameForReceipt();
                if (string.IsNullOrWhiteSpace(printerName))
                {
                    MessageBox.Show("No valid printer selected for receipt printing.",
                        "Receipt Print",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                ExportReport(reportViewer1.LocalReport);
                PrintReport(printerName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Receipt printing failed: " + ex.Message,
                    "Receipt Print",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                DisposeReportStreams();
            }
        }

        private void ExportReport(LocalReport report)
        {
            Warning[] warnings;
            _reportStreams = new List<Stream>();

            report.Render(
                "Image",
                null,
                CreateReportStream,
                out warnings);

            foreach (Stream stream in _reportStreams)
                stream.Position = 0;

            _currentPageIndex = 0;
        }

        private Stream CreateReportStream(string name, string fileNameExtension, Encoding encoding, string mimeType, bool willSeek)
        {
            Stream stream = new MemoryStream();
            _reportStreams.Add(stream);
            return stream;
        }

        private void PrintReport(string printerName)
        {
            if (_reportStreams == null || _reportStreams.Count == 0)
                throw new Exception("No receipt pages were prepared for printing.");

            using (PrintDocument printDoc = new PrintDocument())
            {
                printDoc.PrinterSettings.PrinterName = printerName;

                if (!printDoc.PrinterSettings.IsValid)
                    throw new Exception("Selected receipt printer is not available: " + printerName);

                printDoc.PrintPage += PrintDoc_PrintPage;
                _currentPageIndex = 0;
                printDoc.Print();
            }
        }

        private void PrintDoc_PrintPage(object sender, PrintPageEventArgs ev)
        {
            using (Metafile pageImage = new Metafile(_reportStreams[_currentPageIndex]))
            {
                ev.Graphics.DrawImage(pageImage, ev.PageBounds);
            }

            _currentPageIndex++;
            ev.HasMorePages = (_currentPageIndex < _reportStreams.Count);
        }

        private void DisposeReportStreams()
        {
            if (_reportStreams == null) return;

            foreach (Stream stream in _reportStreams)
                stream.Dispose();

            _reportStreams = null;
        }
    }
}