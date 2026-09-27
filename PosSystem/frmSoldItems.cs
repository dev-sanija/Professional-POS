using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosSystem
{



    public partial class frmSoldItems : Form
    {

        public frmSoldItems()
        {
            InitializeComponent();
            InitializeDefaults();

            // Attach Print Event
            printDoc.PrintPage += new PrintPageEventHandler(printDoc_PrintPage);

            // 1. Draw Arrows
            panelnextpage.Paint += DrawNextArrow;
            panelprevpage.Paint += DrawPrevArrow;

            // 2. Make them look clickable (Hand Cursor)
            panelnextpage.Cursor = Cursors.Hand;
            panelprevpage.Cursor = Cursors.Hand;

            // 3. Wire Click Events
            panelnextpage.Click += panelnextpage_Click;
            panelprevpage.Click += panelprevpage_Click;
        }


        #region Win32 API
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        #endregion


        private int currentPage = 0;
        private const int pageSize = 100;
        private int _printRowIndex = 0;
        private int _printPageNumber = 0;      

        #region Fields
        private readonly string _dbPath = DBConnection.MyConnection();
        private readonly string _appTitle = "POS System";
        private const string STATUS_SOLD = "Sold";

        public string _user;

        // Printing Objects
        PrintDocument printDoc = new PrintDocument();
        PrintPreviewDialog previewDlg = new PrintPreviewDialog();
        #endregion

        #region Public Properties (Bridges)
        public string suser { get => _user; set => _user = value; }
        public DateTimePicker dt1 => dateTimePicker1;
        public DateTimePicker dt2 => dateTimePicker2;
        #endregion

        

        private void DrawNextArrow(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Point[] arrow = { new Point(10, 5), new Point(25, 15), new Point(10, 25) };
            e.Graphics.FillPolygon(Brushes.DarkGray, arrow);
        }

        private void DrawPrevArrow(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Point[] arrow = { new Point(25, 5), new Point(10, 15), new Point(25, 25) };
            e.Graphics.FillPolygon(Brushes.DarkGray, arrow);
        }

        private void InitializeDefaults()
        {
            // Set range: Start date is 7 days ago, End date is Today
            if (dateTimePicker1 != null) dateTimePicker1.Value = DateTime.Now.AddDays(-7);
            if (dateTimePicker2 != null) dateTimePicker2.Value = DateTime.Now;
            if (lblTotal1 != null) lblTotal1.Text = "0.00";
        }

        private async void frmSoldItems_Load(object sender, EventArgs e)
        {
            try
            {
                await LoadCashierAsync();
                await LoadSoldItemsAsync();
            }
            catch (Exception ex)
            {
                HandleError("Initialization Error", ex);
            }
        }

        private async void panelnextpage_Click(object sender, EventArgs e)
        {
            // If the grid is full, there's likely a next page
            if (dataGridView1.Rows.Count == pageSize)
            {
                currentPage++;
                await LoadSoldItemsAsync();
            }
        }

        private async void panelprevpage_Click(object sender, EventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                await LoadSoldItemsAsync();
            }
        }

        public async Task LoadSoldItemsAsync()
        {
            if (dataGridView1 == null) return;

            dataGridView1.Rows.Clear();
            decimal runningTotal = 0;
            int recordCount = 0;

            string dateFrom = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string dateTo = dateTimePicker2.Value.ToString("yyyy-MM-dd");

            try
            {
                using (var cn = new SQLiteConnection(_dbPath))
                {
                    await cn.OpenAsync();

                    // Grouping by price and disc to show different price/discount points in separate rows
                    string sql = @"
                SELECT c.id, c.transno, c.pcode, p.pdesc, 
                       c.price, SUM(c.qty) as qty, c.disc, SUM(c.total) as total, c.[user] as cashier
                FROM tblCart1 AS c
                LEFT JOIN TblProduct1 AS p ON c.pcode = p.pcode
                WHERE c.status = @status 
                AND date(c.sdate) BETWEEN @date1 AND @date2";

                    bool filterByCashier = cbCashier.Text != "All Cashier" && !string.IsNullOrWhiteSpace(cbCashier.Text);
                    if (filterByCashier) sql += " AND c.[user] = @cashier";

                    sql += $" GROUP BY c.transno, c.pcode, c.price, c.disc ORDER BY c.sdate DESC LIMIT {pageSize} OFFSET {currentPage * pageSize}";

                    using (var cmd = new SQLiteCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("@status", STATUS_SOLD);
                        cmd.Parameters.AddWithValue("@date1", dateFrom);
                        cmd.Parameters.AddWithValue("@date2", dateTo);
                        if (filterByCashier) cmd.Parameters.AddWithValue("@cashier", cbCashier.Text);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                recordCount++;
                                decimal total = reader["total"] != DBNull.Value ? Convert.ToDecimal(reader["total"]) : 0;
                                runningTotal += total;

                                
                                dataGridView1.Rows.Add(
                                    recordCount,                                         // Column1 (#)
                                    reader["id"],                                        // Column2 (ID)
                                    
                                    reader["transno"],                                   // Column4 (INVOICE#)
                                    reader["pcode"],                                     // Column5 (PCODE)
                                    reader["pdesc"] ?? "Unknown Product",                // Column6 (DESCRIPTION)
                                    Convert.ToDecimal(reader["price"]).ToString("#,##0.00"), // Column7 (PRICE)
                                    reader["qty"],                                       // Column8 (QTY)
                                    Convert.ToDecimal(reader["disc"]).ToString("#,##0.00"),  // Column9 (DISCOUNT)
                                    total.ToString("#,##0.00"),                          // Column10 (TOTAL)
                                    null                                                 // Column11 (ColCancel - Image)
                                );
                            }
                        }
                    }
                }
                lblTotal1.Text = runningTotal.ToString("#,##0.00");
            }
            catch (Exception ex)
            {
                HandleError("Load Error", ex);
            }
        }

        private async Task LoadCashierAsync()
        {
            if (cbCashier == null) return;
            cbCashier.Items.Clear();
            cbCashier.Items.Add("All Cashier");
            using (var cn = new SQLiteConnection(_dbPath))
            {
                await cn.OpenAsync();
                using (var cmd = new SQLiteCommand("SELECT username FROM tblUser ORDER BY username ASC", cn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync()) cbCashier.Items.Add(reader["username"].ToString());
                }
            }
            if (cbCashier.Items.Count > 0) cbCashier.SelectedIndex = 0;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            previewDlg.Document = printDoc;
            previewDlg.WindowState = FormWindowState.Maximized;
            previewDlg.ShowDialog();
        }

        private void printDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle margin = e.MarginBounds;

            string storeName = "POS SYSTEM";
            string storeAddress = "";
            string storePhone = "";

            try
            {
                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    cn.Open();
                    using (var cmd = new SQLiteCommand("SELECT IFNULL(store,'POS SYSTEM') AS store, IFNULL(address,'') AS address, IFNULL(phone,'') AS phone FROM tblStore LIMIT 1", cn))
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            storeName = dr["store"]?.ToString() ?? "POS SYSTEM";
                            storeAddress = dr["address"]?.ToString() ?? "";
                            storePhone = dr["phone"]?.ToString() ?? "";
                        }
                    }
                }
            }
            catch
            {
                // Keep defaults if store details fail
            }

            using (Font fStore = new Font("Segoe UI", 16, FontStyle.Bold))
            using (Font fTitle = new Font("Segoe UI", 12, FontStyle.Bold))
            using (Font fSub = new Font("Segoe UI", 9, FontStyle.Regular))
            using (Font fHead = new Font("Segoe UI", 9, FontStyle.Bold))
            using (Font fRow = new Font("Segoe UI", 9, FontStyle.Regular))
            using (Font fFooter = new Font("Segoe UI", 8, FontStyle.Italic))
            using (Pen borderPen = new Pen(Color.Black, 1))
            using (Pen lightPen = new Pen(Color.FromArgb(210, 210, 210), 1))
            using (SolidBrush headerBrush = new SolidBrush(Color.FromArgb(235, 235, 235)))
            using (SolidBrush altRowBrush = new SolidBrush(Color.FromArgb(248, 248, 248)))
            using (StringFormat sfLeft = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
            using (StringFormat sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
            using (StringFormat sfRight = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
            {
                if (_printRowIndex == 0)
                    _printPageNumber = 1;
                else
                    _printPageNumber++;

                int left = margin.Left;
                int top = margin.Top;
                int width = margin.Width;

                int rowHeight = 26;
                int headerHeight = 28;

                int colNo = 45;
                int colDesc = 255;
                int colPrice = 90;
                int colQty = 70;
                int colDiscount = 90;
                int colTotal = width - (colNo + colDesc + colPrice + colQty + colDiscount);

                float y = top;

                // ===== HEADER =====
                g.DrawRectangle(borderPen, left, (int)y, width, 110);

                g.DrawString(storeName, fStore, Brushes.Black, new RectangleF(left, y + 8, width, 25), sfCenter);
                g.DrawString(storeAddress, fSub, Brushes.Black, new RectangleF(left, y + 35, width, 18), sfCenter);
                g.DrawString(
                    string.IsNullOrWhiteSpace(storePhone) ? "" : "Phone: " + storePhone,
                    fSub,
                    Brushes.Black,
                    new RectangleF(left, y + 52, width, 18),
                    sfCenter);

                g.DrawString("DAILY SOLD ITEMS REPORT", fTitle, Brushes.Black, new RectangleF(left, y + 74, width, 20), sfCenter);
                g.DrawString("Printed: " + DateTime.Now.ToString("yyyy-MM-dd hh:mm tt"), fSub, Brushes.Black,
                    new RectangleF(left + 10, y + 92, width - 20, 14), sfLeft);
                g.DrawString("Page: " + _printPageNumber, fSub, Brushes.Black,
                    new RectangleF(left + 10, y + 92, width - 20, 14), sfRight);

                y += 120;

                // ===== FILTER INFO =====
                g.DrawRectangle(borderPen, left, (int)y, width, 48);
                g.DrawString("Period: " + dateTimePicker1.Value.ToString("yyyy-MM-dd") + " to " + dateTimePicker2.Value.ToString("yyyy-MM-dd"),
                    fSub, Brushes.Black, new RectangleF(left + 10, y + 6, width / 2f, 16), sfLeft);
                g.DrawString("Cashier: " + (string.IsNullOrWhiteSpace(cbCashier.Text) ? "All Cashier" : cbCashier.Text),
                    fSub, Brushes.Black, new RectangleF(left + (width / 2f), y + 6, (width / 2f) - 10, 16), sfRight);
                g.DrawString("Printed By: " + (string.IsNullOrWhiteSpace(_user) ? "System" : _user),
                    fSub, Brushes.Black, new RectangleF(left + 10, y + 24, width / 2f, 16), sfLeft);
                g.DrawString("Items On This Print: " + dataGridView1.Rows.Cast<DataGridViewRow>().Count(r => !r.IsNewRow),
                    fSub, Brushes.Black, new RectangleF(left + (width / 2f), y + 24, (width / 2f) - 10, 16), sfRight);

                y += 60;

                // ===== TABLE HEADER =====
                int xNo = left;
                int xDesc = xNo + colNo;
                int xPrice = xDesc + colDesc;
                int xQty = xPrice + colPrice;
                int xDiscount = xQty + colQty;
                int xTotal = xDiscount + colDiscount;

                g.FillRectangle(headerBrush, left, y, width, headerHeight);
                g.DrawRectangle(borderPen, left, y, width, headerHeight);

                g.DrawLine(borderPen, xDesc, y, xDesc, y + headerHeight);
                g.DrawLine(borderPen, xPrice, y, xPrice, y + headerHeight);
                g.DrawLine(borderPen, xQty, y, xQty, y + headerHeight);
                g.DrawLine(borderPen, xDiscount, y, xDiscount, y + headerHeight);
                g.DrawLine(borderPen, xTotal, y, xTotal, y + headerHeight);

                g.DrawString("#", fHead, Brushes.Black, new RectangleF(xNo, y, colNo, headerHeight), sfCenter);
                g.DrawString("Description", fHead, Brushes.Black, new RectangleF(xDesc + 4, y, colDesc - 8, headerHeight), sfLeft);
                g.DrawString("Price", fHead, Brushes.Black, new RectangleF(xPrice, y, colPrice - 6, headerHeight), sfRight);
                g.DrawString("Qty", fHead, Brushes.Black, new RectangleF(xQty, y, colQty - 6, headerHeight), sfRight);
                g.DrawString("Discount", fHead, Brushes.Black, new RectangleF(xDiscount, y, colDiscount - 6, headerHeight), sfRight);
                g.DrawString("Total", fHead, Brushes.Black, new RectangleF(xTotal, y, colTotal - 6, headerHeight), sfRight);

                y += headerHeight;

                // ===== TABLE BODY =====
                int printedRowNo = _printRowIndex + 1;

                while (_printRowIndex < dataGridView1.Rows.Count)
                {
                    DataGridViewRow row = dataGridView1.Rows[_printRowIndex];
                    if (row.IsNewRow)
                    {
                        _printRowIndex++;
                        continue;
                    }

                    if (y + rowHeight + 80 > margin.Bottom)
                    {
                        e.HasMorePages = true;
                        return;
                    }

                    if ((_printRowIndex % 2) == 1)
                        g.FillRectangle(altRowBrush, left, y, width, rowHeight);

                    g.DrawRectangle(lightPen, left, y, width, rowHeight);
                    g.DrawLine(lightPen, xDesc, y, xDesc, y + rowHeight);
                    g.DrawLine(lightPen, xPrice, y, xPrice, y + rowHeight);
                    g.DrawLine(lightPen, xQty, y, xQty, y + rowHeight);
                    g.DrawLine(lightPen, xDiscount, y, xDiscount, y + rowHeight);
                    g.DrawLine(lightPen, xTotal, y, xTotal, y + rowHeight);

                    string desc = row.Cells[4]?.Value?.ToString() ?? "";
                    string price = row.Cells[5]?.Value?.ToString() ?? "0.00";
                    string qty = row.Cells[6]?.Value?.ToString() ?? "0";
                    string discount = row.Cells[7]?.Value?.ToString() ?? "0.00";
                    string total = row.Cells[8]?.Value?.ToString() ?? "0.00";

                    g.DrawString(printedRowNo.ToString(), fRow, Brushes.Black, new RectangleF(xNo, y, colNo, rowHeight), sfCenter);
                    g.DrawString(desc, fRow, Brushes.Black, new RectangleF(xDesc + 4, y, colDesc - 8, rowHeight), sfLeft);
                    g.DrawString(price, fRow, Brushes.Black, new RectangleF(xPrice, y, colPrice - 6, rowHeight), sfRight);
                    g.DrawString(qty, fRow, Brushes.Black, new RectangleF(xQty, y, colQty - 6, rowHeight), sfRight);
                    g.DrawString(discount, fRow, Brushes.Black, new RectangleF(xDiscount, y, colDiscount - 6, rowHeight), sfRight);
                    g.DrawString(total, fRow, Brushes.Black, new RectangleF(xTotal, y, colTotal - 6, rowHeight), sfRight);

                    y += rowHeight;
                    _printRowIndex++;
                    printedRowNo++;
                }

                // ===== SUMMARY =====
                y += 10;
                int summaryWidth = 220;
                int summaryX = left + width - summaryWidth;

                g.DrawRectangle(borderPen, summaryX, y, summaryWidth, 34);
                g.DrawString("GRAND TOTAL", fHead, Brushes.Black, new RectangleF(summaryX + 8, y, 100, 34), sfLeft);
                g.DrawString("LKR " + lblTotal1.Text, fTitle, Brushes.Black, new RectangleF(summaryX + 100, y, summaryWidth - 108, 34), sfRight);

                y += 50;

                // ===== FOOTER =====
                g.DrawString("This is a system generated sold items report.", fFooter, Brushes.Black,
                    new RectangleF(left, y, width, 18), sfLeft);

                e.HasMorePages = false;
                _printRowIndex = 0;
                _printPageNumber = 0;
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string colName = dataGridView1.Columns[e.ColumnIndex].Name;

            if (colName == "ColCancel")
            {
                var row = dataGridView1.Rows[e.RowIndex];

                // Grab price from Column 6 (Index 5) to pass to constructor
                decimal price = 0;
                decimal.TryParse(row.Cells[5].Value.ToString(), out price);

                // Pass 'this' (frmSoldItems) and the unit price
                frmCancelDetails f = new frmCancelDetails(this, price);

                // WIRING MAPPING (No Spacers)
                f.txtID.Text = row.Cells[1].Value.ToString();       // ID (Index 1)
                f.txtTransno.Text = row.Cells[2].Value.ToString();  // INVOICE# (Index 2)
                f.txtPcode.Text = row.Cells[3].Value.ToString();    // PCODE (Index 3)
                f.txtDesc.Text = row.Cells[4].Value.ToString();     // DESCRIPTION (Index 4)
                f.txtPrice.Text = row.Cells[5].Value.ToString();    // PRICE (Index 5)
                f.txtQty.Text = row.Cells[6].Value.ToString();      // QTY (Index 6)
                f.txtDiscount.Text = row.Cells[7].Value.ToString(); // DISCOUNT (Index 7)

                // --- FIXED: Mapping txtTotal ---
                f.txtTotal.Text = row.Cells[8].Value.ToString();    // TOTAL (Index 8)

                f.txtCancelled.Text = _user; // Set who is performing the void
                f.ShowDialog();
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e) => this.Dispose();

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }


        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            currentPage = 0; // Reset to page 1
            _ = LoadSoldItemsAsync();
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            currentPage = 0; // Reset to page 1
            _ = LoadSoldItemsAsync();
        }

        private void cbCashier_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPage = 0; // Reset to page 1
            _ = LoadSoldItemsAsync();
        }

        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void lblTotal1_Click(object sender, EventArgs e) { }

        private void HandleError(string context, Exception ex)
        {
            MessageBox.Show($"{context}: {ex.Message}", _appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}