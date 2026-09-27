using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace PosSystem
{
    public partial class frmRecords : Form
    {
        private readonly string stitle = "PosSystem";
        private const int DefaultLookbackDays = 30;
        private const int PageSize = 100;

        private System.Windows.Forms.Timer searchDelayTimer;
        private System.Windows.Forms.Timer realTimeTimer;

        private int _topSellingPage = 0;
        private int _soldItemsPage = 0;
        private int _criticalPage = 0;
        private int _inventoryPage = 0;
        private int _cancelledPage = 0;
        private int _stockPage = 0;

        private int _topSellingTotalCount = 0;
        private int _soldItemsTotalCount = 0;
        private int _criticalTotalCount = 0;
        private int _inventoryTotalCount = 0;
        private int _cancelledTotalCount = 0;
        private int _stockTotalCount = 0;

        private int _isTopSellingLoading = 0;
        private int _isSoldLoading = 0;
        private int _isCriticalLoading = 0;
        private int _isInventoryLoading = 0;
        private int _isCancelLoading = 0;
        private int _isStockLoading = 0;

        private string _lastCriticalSearch = string.Empty;
        private string _lastInventorySearch = string.Empty;
        private string _lastCancelSearch = string.Empty;

        private CancellationTokenSource _topSellingCts;
        private CancellationTokenSource _soldCts;
        private CancellationTokenSource _criticalCts;
        private CancellationTokenSource _inventoryCts;
        private CancellationTokenSource _cancelCts;
        private CancellationTokenSource _stockCts;

        private readonly List<string> _allCategories = new List<string>();
        private bool _isFilteringCategoryCombo;

        private static readonly Font hFont = new Font("Segoe UI", 9, FontStyle.Bold);
        private static readonly Font cFont = new Font("Segoe UI", 8, FontStyle.Regular);
        private static readonly Font titleFont = new Font("Segoe UI", 16, FontStyle.Bold);
        private static readonly Font dateFont = new Font("Segoe UI", 8);
        private static readonly Pen gridPen = new Pen(Color.Gray, 1);

        private readonly PrintDocument printDoc = new PrintDocument();
        private DataGridView dgvToPrint;
        private int checkRow = 0;
        private string _printTitle = "REPORT";
        private string _printSubTitle = "";
        private string _printStoreName = "POS SYSTEM";
        private string _printStoreAddress = "";

        private EventHandler _inventoryChangedHandler;

        public frmRecords()
        {
            InitializeComponent();

            SetDataGridViewFormats();
            InitializeSearchTimer();
            ConfigurePagingPanels();
            ConfigureCategoryComboBoxes();
            SetupRealTimeTimer();

            printDoc.PrintPage += new PrintPageEventHandler(PrintDocument_PrintPage);

            _inventoryChangedHandler = (s, e) => _ = LoadCurrentTabAsync();
            DataEvents.InventoryChanged += _inventoryChangedHandler;

            if (txtSearchCritical != null)
                txtSearchCritical.TextChanged += txtSearchCritical_TextChanged;

            if (tabControl1 != null)
                tabControl1.SelectedIndexChanged += async (s, e) => await LoadCurrentTabAsync();

            if (cbCriticle != null)
                cbCriticle.SelectedIndexChanged += async (s, e) =>
                {
                    _criticalPage = 0;
                    await LoadCriticalItemsAsync();
                };

            if (cbCategory != null)
                cbCategory.SelectedIndexChanged += async (s, e) =>
                {
                    if (_isFilteringCategoryCombo) return;
                    _criticalPage = 0;
                    await LoadCriticalItemsAsync();
                };

            if (cbcategoryinventorysearch != null)
                cbcategoryinventorysearch.SelectedIndexChanged += async (s, e) =>
                {
                    if (_isFilteringCategoryCombo) return;
                    _inventoryPage = 0;
                    await LoadInventoryAsync();
                };

            if (dateTimePicker3 != null)
                dateTimePicker3.ValueChanged += async (s, e) =>
                {
                    _soldItemsPage = 0;
                    await LoadSoldSummaryAsync();
                };

            if (dateTimePicker4 != null)
                dateTimePicker4.ValueChanged += async (s, e) =>
                {
                    _soldItemsPage = 0;
                    await LoadSoldSummaryAsync();
                };

            if (dateTimePicker5 != null)
                dateTimePicker5.ValueChanged += async (s, e) =>
                {
                    _cancelledPage = 0;
                    await LoadCancelledOrdersAsync();
                };

            if (dateTimePicker6 != null)
                dateTimePicker6.ValueChanged += async (s, e) =>
                {
                    _cancelledPage = 0;
                    await LoadCancelledOrdersAsync();
                };

            if (dateTimePicker7 != null)
                dateTimePicker7.ValueChanged += async (s, e) =>
                {
                    _stockPage = 0;
                    await LoadStockHistoryAsync();
                };

            if (dateTimePicker8 != null)
                dateTimePicker8.ValueChanged += async (s, e) =>
                {
                    _stockPage = 0;
                    await LoadStockHistoryAsync();
                };
        }

        private void UI(Action a)
        {
            if (IsDisposed || !IsHandleCreated) return;
            if (InvokeRequired) BeginInvoke(a);
            else a();
        }

        private void SetupRealTimeTimer()
        {
            realTimeTimer = new System.Windows.Forms.Timer();
            realTimeTimer.Interval = 30000;
            realTimeTimer.Tick += async (s, e) =>
            {
                if (searchDelayTimer != null && searchDelayTimer.Enabled) return;
                await LoadCurrentTabAsync();
            };
            realTimeTimer.Start();
        }

        public static class DataEvents
        {
            public static event EventHandler InventoryChanged;
            public static void OnInventoryChanged() => InventoryChanged?.Invoke(null, EventArgs.Empty);
        }

        private void InitializeSearchTimer()
        {
            searchDelayTimer = new System.Windows.Forms.Timer();
            searchDelayTimer.Interval = 350;
            searchDelayTimer.Tick += async (s, e) =>
            {
                searchDelayTimer.Stop();

                string tabText = tabControl1?.SelectedTab?.Text ?? string.Empty;

                if (tabText == "Critical Stocks")
                {
                    _criticalPage = 0;
                    await LoadCriticalItemsAsync();
                }
                else if (tabText == "Inventory List")
                {
                    _inventoryPage = 0;
                    await LoadInventoryAsync();
                }
                else if (tabText == "Cancelled Order")
                {
                    _cancelledPage = 0;
                    await LoadCancelledOrdersAsync();
                }
            };
        }

        private void TriggerSearch()
        {
            if (searchDelayTimer == null) return;
            searchDelayTimer.Stop();
            searchDelayTimer.Start();
        }

        private void frmRecords_Load(object sender, EventArgs e)
        {
            try
            {
                ConfigureDateDefaults();

                LoadCategoryFilter(cbCategory);
                LoadCategoryFilter(cbcategoryinventorysearch);

                if (cdTopSelling.Items.Count > 0 && cdTopSelling.SelectedIndex < 0)
                    cdTopSelling.SelectedIndex = 0;

                if (cbCriticle.Items.Count > 0 && cbCriticle.SelectedIndex < 0)
                    cbCriticle.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, stitle);
            }

            if (cdTopSelling != null)
            {
                cdTopSelling.SelectedIndexChanged += async (s, e2) =>
                {
                    _topSellingPage = 0;
                    await LoadTopSellingAsync();
                };
            }

            if (dateTimePicker1 != null)
                dateTimePicker1.ValueChanged += async (s, e2) =>
                {
                    _topSellingPage = 0;
                    await LoadTopSellingAsync();
                };

            if (dateTimePicker2 != null)
                dateTimePicker2.ValueChanged += async (s, e2) =>
                {
                    _topSellingPage = 0;
                    await LoadTopSellingAsync();
                };

            _ = LoadAllAsync();
        }

        private void ConfigureDateDefaults()
        {
            DateTime today = DateTime.Today;
            DateTime oldDate = today.AddDays(-DefaultLookbackDays);

            if (dateTimePicker1 != null) dateTimePicker1.Value = oldDate;
            if (dateTimePicker2 != null) dateTimePicker2.Value = today;

            if (dateTimePicker4 != null) dateTimePicker4.Value = oldDate;
            if (dateTimePicker3 != null) dateTimePicker3.Value = today;

            if (dateTimePicker5 != null) dateTimePicker5.Value = oldDate;
            if (dateTimePicker6 != null) dateTimePicker6.Value = today;

            if (dateTimePicker8 != null) dateTimePicker8.Value = oldDate;
            if (dateTimePicker7 != null) dateTimePicker7.Value = today;
        }

        private void ConfigureCategoryComboBoxes()
        {
            ConfigureCategoryCombo(cbCategory);
            ConfigureCategoryCombo(cbcategoryinventorysearch);
        }

        private void ConfigureCategoryCombo(ComboBox combo)
        {
            if (combo == null) return;

            combo.DropDownStyle = ComboBoxStyle.DropDown;
            combo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            combo.AutoCompleteSource = AutoCompleteSource.ListItems;
            combo.TextUpdate += CategoryCombo_TextUpdate;
        }

        private void CategoryCombo_TextUpdate(object sender, EventArgs e)
        {
            if (_isFilteringCategoryCombo) return;

            ComboBox combo = sender as ComboBox;
            if (combo == null) return;

            string typedText = combo.Text.Trim();

            _isFilteringCategoryCombo = true;
            try
            {
                IEnumerable<string> matches = _allCategories;

                if (!string.IsNullOrWhiteSpace(typedText))
                    matches = _allCategories.Where(x => x.IndexOf(typedText, StringComparison.OrdinalIgnoreCase) >= 0);

                combo.BeginUpdate();
                combo.Items.Clear();
                foreach (string item in matches)
                    combo.Items.Add(item);
                combo.EndUpdate();

                combo.DroppedDown = true;
                combo.Text = typedText;
                combo.SelectionStart = combo.Text.Length;
                combo.SelectionLength = 0;
                System.Windows.Forms.Cursor.Current = Cursors.Default;
            }
            finally
            {
                _isFilteringCategoryCombo = false;
            }
        }

        public void LoadCategoryFilter(ComboBox combo)
        {
            try
            {
                if (combo == null) return;

                LoadCategoryMasterList();

                combo.BeginUpdate();
                combo.Items.Clear();
                foreach (string item in _allCategories)
                    combo.Items.Add(item);
                combo.EndUpdate();

                if (combo.Items.Count > 0)
                    combo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, stitle);
            }
        }

        private void LoadCategoryMasterList()
        {
            _allCategories.Clear();
            _allCategories.Add("All Categories");

            using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
            {
                cn.Open();
                using (var cmd = new SQLiteCommand("SELECT category FROM TblCategory ORDER BY category ASC", cn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string value = reader["category"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(value) && !_allCategories.Contains(value))
                            _allCategories.Add(value);
                    }
                }
            }
        }

        private string GetCategoryFilterValue(ComboBox combo)
        {
            if (combo == null) return "All Categories";

            string value = combo.Text.Trim();
            if (string.IsNullOrWhiteSpace(value))
                return "All Categories";

            string exact = _allCategories.FirstOrDefault(x => x.Equals(value, StringComparison.OrdinalIgnoreCase));
            return string.IsNullOrWhiteSpace(exact) ? "All Categories" : exact;
        }

        private async Task LoadAllAsync()
        {
            try
            {
                await LoadTopSellingAsync();
                await LoadSoldSummaryAsync();
                await LoadCriticalItemsAsync();
                await LoadInventoryAsync();
                await LoadCancelledOrdersAsync();
                await LoadStockHistoryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private async Task LoadCurrentTabAsync()
        {
            if (tabControl1 == null || tabControl1.SelectedTab == null)
            {
                await LoadTopSellingAsync();
                return;
            }

            string tabText = tabControl1.SelectedTab.Text;

            if (tabText == "Top Selling")
                await LoadTopSellingAsync();
            else if (tabText == "Sold Items")
                await LoadSoldSummaryAsync();
            else if (tabText == "Critical Stocks")
                await LoadCriticalItemsAsync();
            else if (tabText == "Inventory List")
                await LoadInventoryAsync();
            else if (tabText == "Cancelled Order")
                await LoadCancelledOrdersAsync();
            else if (tabText == "Stock In History")
                await LoadStockHistoryAsync();
        }

        private void SetDataGridViewFormats()
        {
            DataGridView[] grids = { dataGridView1, dataGridView2, dataGridView3, dataGridView4, dataGridView5, dataGridView6 };
            foreach (var dgv in grids)
            {
                if (dgv == null) continue;

                dgv.ReadOnly = true;
                dgv.AllowUserToAddRows = false;
                dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgv.BackgroundColor = Color.White;
                dgv.RowHeadersVisible = false;
                dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(238, 239, 249);

                typeof(DataGridView).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.SetProperty,
                    null, dgv, new object[] { true });
            }
        }

        #region Paging Panels
        private void ConfigurePagingPanels()
        {
            ConfigureNavPanel(panel11, false, async () =>
            {
                if (_topSellingPage <= 0) return;
                _topSellingPage--;
                await LoadTopSellingAsync();
            }, () => _topSellingPage > 0);

            ConfigureNavPanel(panel10, true, async () =>
            {
                if (!CanGoNext(_topSellingPage, _topSellingTotalCount)) return;
                _topSellingPage++;
                await LoadTopSellingAsync();
            }, () => CanGoNext(_topSellingPage, _topSellingTotalCount));

            ConfigureNavPanel(panelprevpage, false, async () =>
            {
                if (_soldItemsPage <= 0) return;
                _soldItemsPage--;
                await LoadSoldSummaryAsync();
            }, () => _soldItemsPage > 0);

            ConfigureNavPanel(panelnextpage, true, async () =>
            {
                if (!CanGoNext(_soldItemsPage, _soldItemsTotalCount)) return;
                _soldItemsPage++;
                await LoadSoldSummaryAsync();
            }, () => CanGoNext(_soldItemsPage, _soldItemsTotalCount));

            ConfigureNavPanel(panel14, false, async () =>
            {
                if (_criticalPage <= 0) return;
                _criticalPage--;
                await LoadCriticalItemsAsync();
            }, () => _criticalPage > 0);

            ConfigureNavPanel(panel13, true, async () =>
            {
                if (!CanGoNext(_criticalPage, _criticalTotalCount)) return;
                _criticalPage++;
                await LoadCriticalItemsAsync();
            }, () => CanGoNext(_criticalPage, _criticalTotalCount));

            ConfigureNavPanel(panel17, false, async () =>
            {
                if (_inventoryPage <= 0) return;
                _inventoryPage--;
                await LoadInventoryAsync();
            }, () => _inventoryPage > 0);

            ConfigureNavPanel(panel16, true, async () =>
            {
                if (!CanGoNext(_inventoryPage, _inventoryTotalCount)) return;
                _inventoryPage++;
                await LoadInventoryAsync();
            }, () => CanGoNext(_inventoryPage, _inventoryTotalCount));

            ConfigureNavPanel(panel20, false, async () =>
            {
                if (_cancelledPage <= 0) return;
                _cancelledPage--;
                await LoadCancelledOrdersAsync();
            }, () => _cancelledPage > 0);

            ConfigureNavPanel(panel19, true, async () =>
            {
                if (!CanGoNext(_cancelledPage, _cancelledTotalCount)) return;
                _cancelledPage++;
                await LoadCancelledOrdersAsync();
            }, () => CanGoNext(_cancelledPage, _cancelledTotalCount));

            ConfigureNavPanel(panel23, false, async () =>
            {
                if (_stockPage <= 0) return;
                _stockPage--;
                await LoadStockHistoryAsync();
            }, () => _stockPage > 0);

            ConfigureNavPanel(panel22, true, async () =>
            {
                if (!CanGoNext(_stockPage, _stockTotalCount)) return;
                _stockPage++;
                await LoadStockHistoryAsync();
            }, () => CanGoNext(_stockPage, _stockTotalCount));
        }

        private void ConfigureNavPanel(Panel panel, bool isNext, Func<Task> clickAction, Func<bool> isEnabled)
        {
            if (panel == null) return;

            panel.Visible = true;
            panel.Cursor = Cursors.Hand;
            panel.BackColor = Color.WhiteSmoke;
            panel.BorderStyle = BorderStyle.FixedSingle;

            panel.Paint += (s, e) =>
            {
                bool enabled = isEnabled();
                PaintNavArrow(panel, e.Graphics, isNext, enabled);
            };

            panel.Click += async (s, e) =>
            {
                if (!isEnabled()) return;
                await clickAction();
            };

            panel.MouseEnter += (s, e) =>
            {
                if (isEnabled()) panel.BackColor = Color.FromArgb(232, 245, 251);
            };

            panel.MouseLeave += (s, e) =>
            {
                panel.BackColor = Color.WhiteSmoke;
            };
        }

        private void PaintNavArrow(Panel panel, Graphics g, bool isNext, bool enabled)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(panel.BackColor);

            using (Brush b = new SolidBrush(enabled ? Color.FromArgb(20, 158, 132) : Color.LightGray))
            {
                Point[] points = isNext
                    ? new Point[]
                    {
                        new Point(panel.Width / 2 - 4, panel.Height / 2 - 8),
                        new Point(panel.Width / 2 + 5, panel.Height / 2),
                        new Point(panel.Width / 2 - 4, panel.Height / 2 + 8)
                    }
                    : new Point[]
                    {
                        new Point(panel.Width / 2 + 4, panel.Height / 2 - 8),
                        new Point(panel.Width / 2 - 5, panel.Height / 2),
                        new Point(panel.Width / 2 + 4, panel.Height / 2 + 8)
                    };

                g.FillPolygon(b, points);
            }
        }

        private bool CanGoNext(int currentPage, int totalCount)
        {
            return ((currentPage + 1) * PageSize) < totalCount;
        }

        private void RefreshPagingPanels()
        {
            Panel[] panels =
            {
                panel10, panel11,
                panelprevpage, panelnextpage,
                panel13, panel14,
                panel16, panel17,
                panel19, panel20,
                panel22, panel23
            };

            foreach (Panel p in panels)
            {
                if (p != null)
                    p.Invalidate();
            }
        }
        #endregion

        #region Data Loaders
        private async Task<int> GetScalarCountAsync(SQLiteConnection cn, string sql, Action<SQLiteCommand> bindParameters, CancellationToken token)
        {
            using (var cmd = new SQLiteCommand(sql, cn))
            {
                bindParameters?.Invoke(cmd);
                object result = await cmd.ExecuteScalarAsync(token);
                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
        }

        private void NormalizeDates(DateTime from, DateTime to, out DateTime d1, out DateTime d2)
        {
            d1 = from.Date;
            d2 = to.Date;
            if (d1 > d2)
            {
                DateTime temp = d1;
                d1 = d2;
                d2 = temp;
            }
        }

        private string BuildDateRangeText(DateTime from, DateTime to)
        {
            NormalizeDates(from, to, out DateTime d1, out DateTime d2);
            return $"Date Range: {d1:yyyy-MM-dd} to {d2:yyyy-MM-dd}";
        }

        private string SafeValue(object value)
        {
            return value == null || value == DBNull.Value ? "" : value.ToString();
        }

        public async Task LoadTopSellingAsync()
        {
            if (Interlocked.CompareExchange(ref _isTopSellingLoading, 1, 0) != 0) return;

            _topSellingCts?.Cancel();
            _topSellingCts?.Dispose();
            _topSellingCts = new CancellationTokenSource();
            var token = _topSellingCts.Token;

            var rows = new List<object[]>();

            try
            {
                NormalizeDates(dateTimePicker1.Value, dateTimePicker2.Value, out DateTime d1, out DateTime d2);
                string sortColumn = cdTopSelling != null && cdTopSelling.Text == "Sort by Total Amount"
                    ? "total_amount"
                    : "total_qty";

                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    await cn.OpenAsync(token);

                    string countSql = @"
                        SELECT COUNT(*) FROM (
                            SELECT pcode, pdesc
                            FROM vwSoldItems
                            WHERE status = 'Sold'
                              AND date(sdate) BETWEEN date(@d1) AND date(@d2)
                            GROUP BY pcode, pdesc
                        ) t";

                    _topSellingTotalCount = await GetScalarCountAsync(cn, countSql, cmd =>
                    {
                        cmd.Parameters.AddWithValue("@d1", d1.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@d2", d2.ToString("yyyy-MM-dd"));
                    }, token);

                    string sql = $@"
                        SELECT pcode, pdesc,
                               SUM(qty) AS total_qty,
                               SUM(total) AS total_amount
                        FROM vwSoldItems
                        WHERE status = 'Sold'
                          AND date(sdate) BETWEEN date(@d1) AND date(@d2)
                        GROUP BY pcode, pdesc
                        ORDER BY {sortColumn} DESC, pdesc ASC
                        LIMIT @limit OFFSET @offset";

                    using (var cmd = new SQLiteCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("@d1", d1.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@d2", d2.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@limit", PageSize);
                        cmd.Parameters.AddWithValue("@offset", _topSellingPage * PageSize);

                        using (var reader = await cmd.ExecuteReaderAsync(token))
                        {
                            int i = _topSellingPage * PageSize;
                            while (await reader.ReadAsync(token))
                            {
                                rows.Add(new object[]
                                {
                                    ++i,
                                    reader["pcode"],
                                    reader["pdesc"],
                                    reader["total_qty"],
                                    Convert.ToDouble(reader["total_amount"]).ToString("#,##0.00")
                                });
                            }
                        }
                    }
                }

                if (token.IsCancellationRequested) return;

                UI(() =>
                {
                    dataGridView1.SuspendLayout();
                    dataGridView1.Rows.Clear();
                    foreach (var row in rows) dataGridView1.Rows.Add(row);
                    dataGridView1.ResumeLayout();
                    RefreshPagingPanels();
                });
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, stitle);
            }
            finally
            {
                Interlocked.Exchange(ref _isTopSellingLoading, 0);
            }
        }

        public async Task LoadSoldSummaryAsync()
        {
            if (Interlocked.CompareExchange(ref _isSoldLoading, 1, 0) != 0) return;

            _soldCts?.Cancel();
            _soldCts?.Dispose();
            _soldCts = new CancellationTokenSource();
            var token = _soldCts.Token;

            var rows = new List<object[]>();
            decimal visibleTotal = 0m;

            try
            {
                NormalizeDates(dateTimePicker4.Value, dateTimePicker3.Value, out DateTime d1, out DateTime d2);

                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    await cn.OpenAsync(token);

                    string countSql = @"
                        SELECT COUNT(*)
                        FROM vwSoldItems
                        WHERE status = 'Sold'
                          AND date(sdate) BETWEEN date(@d1) AND date(@d2)";

                    _soldItemsTotalCount = await GetScalarCountAsync(cn, countSql, cmd =>
                    {
                        cmd.Parameters.AddWithValue("@d1", d1.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@d2", d2.ToString("yyyy-MM-dd"));
                    }, token);

                    string sql = @"
                        SELECT pcode, pdesc, price, qty, disc, total, sdate
                        FROM vwSoldItems
                        WHERE status = 'Sold'
                          AND date(sdate) BETWEEN date(@d1) AND date(@d2)
                        ORDER BY date(sdate) DESC, id DESC
                        LIMIT @limit OFFSET @offset";

                    using (var cmd = new SQLiteCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("@d1", d1.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@d2", d2.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@limit", PageSize);
                        cmd.Parameters.AddWithValue("@offset", _soldItemsPage * PageSize);

                        using (var reader = await cmd.ExecuteReaderAsync(token))
                        {
                            int i = _soldItemsPage * PageSize;
                            while (await reader.ReadAsync(token))
                            {
                                decimal rowTotal = Convert.ToDecimal(reader["total"]);
                                visibleTotal += rowTotal;

                                rows.Add(new object[]
                                {
                                    ++i,
                                    reader["pcode"],
                                    reader["pdesc"],
                                    Convert.ToDouble(reader["price"]).ToString("#,##0.00"),
                                    reader["qty"],
                                    Convert.ToDouble(reader["disc"]).ToString("#,##0.00"),
                                    Convert.ToDouble(reader["total"]).ToString("#,##0.00")
                                });
                            }
                        }
                    }
                }

                if (token.IsCancellationRequested) return;

                UI(() =>
                {
                    dataGridView2.SuspendLayout();
                    dataGridView2.Rows.Clear();
                    foreach (var row in rows) dataGridView2.Rows.Add(row);
                    dataGridView2.ResumeLayout();

                    if (lblTotal != null)
                        lblTotal.Text = "LKR " + visibleTotal.ToString("#,##0.00");

                    RefreshPagingPanels();
                });
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, stitle);
            }
            finally
            {
                Interlocked.Exchange(ref _isSoldLoading, 0);
            }
        }

        public async Task LoadCriticalItemsAsync()
        {
            string currentSearch = (txtSearchCritical != null ? txtSearchCritical.Text.Trim() : "") +
                                   "|" + (cbCriticle != null ? cbCriticle.Text : "") +
                                   "|" + GetCategoryFilterValue(cbCategory);

            if (currentSearch == _lastCriticalSearch && _isCriticalLoading == 1) return;
            if (Interlocked.CompareExchange(ref _isCriticalLoading, 1, 0) != 0) return;

            _criticalCts?.Cancel();
            _criticalCts?.Dispose();
            _criticalCts = new CancellationTokenSource();
            var token = _criticalCts.Token;

            var rows = new List<object[]>();

            try
            {
                string search = "%" + (txtSearchCritical != null ? txtSearchCritical.Text.Trim() : "") + "%";
                string category = GetCategoryFilterValue(cbCategory);
                string qtySort = (cbCriticle != null && cbCriticle.Text.IndexOf("ASC", StringComparison.OrdinalIgnoreCase) >= 0)
                    ? "ASC"
                    : "DESC";

                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    await cn.OpenAsync(token);

                    string baseWhere = @"
                        FROM TblProduct1 p
                        LEFT JOIN BrandTbl b ON p.bid = b.id
                        LEFT JOIN TblCategory c ON p.cid = c.id
                        WHERE p.isactive = 1
                          AND p.qty <= p.reorder
                          AND (
                              p.pdesc LIKE @search
                              OR p.barcode LIKE @search
                              OR p.pcode LIKE @search
                              OR IFNULL(b.brand,'') LIKE @search
                          )";

                    if (category != "All Categories")
                        baseWhere += " AND IFNULL(c.category,'') = @category";

                    string countSql = "SELECT COUNT(*) " + baseWhere;
                    _criticalTotalCount = await GetScalarCountAsync(cn, countSql, cmd =>
                    {
                        cmd.Parameters.AddWithValue("@search", search);
                        if (category != "All Categories")
                            cmd.Parameters.AddWithValue("@category", category);
                    }, token);

                    string sql = @"
                        SELECT p.pcode, p.barcode, p.pdesc,
                               IFNULL(b.brand,'') AS brand,
                               IFNULL(c.category,'') AS category,
                               p.price, p.reorder, p.qty "
                               + baseWhere +
                               $" ORDER BY p.qty {qtySort}, p.pdesc ASC LIMIT @limit OFFSET @offset";

                    using (var cmd = new SQLiteCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("@search", search);
                        if (category != "All Categories")
                            cmd.Parameters.AddWithValue("@category", category);
                        cmd.Parameters.AddWithValue("@limit", PageSize);
                        cmd.Parameters.AddWithValue("@offset", _criticalPage * PageSize);

                        using (var reader = await cmd.ExecuteReaderAsync(token))
                        {
                            int i = _criticalPage * PageSize;
                            while (await reader.ReadAsync(token))
                            {
                                rows.Add(new object[]
                                {
                                    ++i,
                                    reader["pcode"],
                                    reader["barcode"],
                                    reader["pdesc"],
                                    reader["brand"],
                                    reader["category"],
                                    reader["price"],
                                    reader["reorder"],
                                    reader["qty"]
                                });
                            }
                        }
                    }
                }

                if (token.IsCancellationRequested) return;

                UI(() =>
                {
                    dataGridView3.SuspendLayout();
                    dataGridView3.Rows.Clear();
                    foreach (var row in rows) dataGridView3.Rows.Add(row);
                    dataGridView3.ResumeLayout();

                    _lastCriticalSearch = currentSearch;
                    RefreshPagingPanels();
                });
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, stitle);
            }
            finally
            {
                Interlocked.Exchange(ref _isCriticalLoading, 0);
            }
        }

        public async Task LoadInventoryAsync()
        {
            string currentSearch = (textboxinventorysearch != null ? textboxinventorysearch.Text.Trim() : "") +
                                   "|" + GetCategoryFilterValue(cbcategoryinventorysearch);

            if (currentSearch == _lastInventorySearch && _isInventoryLoading == 1) return;
            if (Interlocked.CompareExchange(ref _isInventoryLoading, 1, 0) != 0) return;

            _inventoryCts?.Cancel();
            _inventoryCts?.Dispose();
            _inventoryCts = new CancellationTokenSource();
            var token = _inventoryCts.Token;

            var rows = new List<object[]>();

            try
            {
                string search = "%" + (textboxinventorysearch != null ? textboxinventorysearch.Text.Trim() : "") + "%";
                string category = GetCategoryFilterValue(cbcategoryinventorysearch);

                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    await cn.OpenAsync(token);

                    string baseWhere = @"
                        FROM TblProduct1 p
                        LEFT JOIN BrandTbl b ON p.bid = b.id
                        LEFT JOIN TblCategory c ON p.cid = c.id
                        WHERE (
                            p.pdesc LIKE @search
                            OR p.pcode LIKE @search
                            OR p.barcode LIKE @search
                        )";

                    if (category != "All Categories")
                        baseWhere += " AND IFNULL(c.category,'') = @category";

                    string countSql = "SELECT COUNT(*) " + baseWhere;
                    _inventoryTotalCount = await GetScalarCountAsync(cn, countSql, cmd =>
                    {
                        cmd.Parameters.AddWithValue("@search", search);
                        if (category != "All Categories")
                            cmd.Parameters.AddWithValue("@category", category);
                    }, token);

                    string sql = @"
                        SELECT p.pcode, p.barcode, p.pdesc,
                               IFNULL(b.brand,'') AS brand,
                               IFNULL(c.category,'') AS category,
                               p.price, p.reorder, p.qty "
                               + baseWhere +
                               " ORDER BY p.pdesc ASC LIMIT @limit OFFSET @offset";

                    using (var cmd = new SQLiteCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("@search", search);
                        if (category != "All Categories")
                            cmd.Parameters.AddWithValue("@category", category);
                        cmd.Parameters.AddWithValue("@limit", PageSize);
                        cmd.Parameters.AddWithValue("@offset", _inventoryPage * PageSize);

                        using (var reader = await cmd.ExecuteReaderAsync(token))
                        {
                            int i = _inventoryPage * PageSize;
                            while (await reader.ReadAsync(token))
                            {
                                rows.Add(new object[]
                                {
                                    ++i,
                                    reader["pcode"],
                                    reader["barcode"],
                                    reader["pdesc"],
                                    reader["brand"],
                                    reader["category"],
                                    reader["price"],
                                    reader["reorder"],
                                    reader["qty"]
                                });
                            }
                        }
                    }
                }

                if (token.IsCancellationRequested) return;

                UI(() =>
                {
                    dataGridView4.SuspendLayout();
                    dataGridView4.Rows.Clear();
                    foreach (var row in rows) dataGridView4.Rows.Add(row);
                    dataGridView4.ResumeLayout();

                    _lastInventorySearch = currentSearch;
                    RefreshPagingPanels();
                });
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, stitle);
            }
            finally
            {
                Interlocked.Exchange(ref _isInventoryLoading, 0);
            }
        }

        public async Task LoadCancelledOrdersAsync()
        {
            string currentSearch = (textsearchTransNo != null ? textsearchTransNo.Text.Trim() : "") +
                                   "|" + dateTimePicker5.Value.Date +
                                   "|" + dateTimePicker6.Value.Date;

            if (currentSearch == _lastCancelSearch && _isCancelLoading == 1) return;
            if (Interlocked.CompareExchange(ref _isCancelLoading, 1, 0) != 0) return;

            _cancelCts?.Cancel();
            _cancelCts?.Dispose();
            _cancelCts = new CancellationTokenSource();
            var token = _cancelCts.Token;

            var rows = new List<object[]>();

            try
            {
                NormalizeDates(dateTimePicker5.Value, dateTimePicker6.Value, out DateTime d1, out DateTime d2);
                string tn = (textsearchTransNo != null ? textsearchTransNo.Text.Trim() : "") + "%";

                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    await cn.OpenAsync(token);

                    string countSql = @"
                        SELECT COUNT(*)
                        FROM tblCancel c
                        WHERE date(c.sdate) BETWEEN date(@d1) AND date(@d2)
                          AND c.transno LIKE @tn";

                    _cancelledTotalCount = await GetScalarCountAsync(cn, countSql, cmd =>
                    {
                        cmd.Parameters.AddWithValue("@d1", d1.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@d2", d2.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@tn", tn);
                    }, token);

                    string sql = @"
                        SELECT c.transno, c.pcode, p.pdesc, c.price, c.qty, c.total, c.sdate, c.voidby, c.cancelledby, c.reason
                        FROM tblCancel c
                        LEFT JOIN TblProduct1 p ON c.pcode = p.pcode
                        WHERE date(c.sdate) BETWEEN date(@d1) AND date(@d2)
                          AND c.transno LIKE @tn
                        ORDER BY date(c.sdate) DESC, c.id DESC
                        LIMIT @limit OFFSET @offset";

                    using (var cmd = new SQLiteCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("@d1", d1.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@d2", d2.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@tn", tn);
                        cmd.Parameters.AddWithValue("@limit", PageSize);
                        cmd.Parameters.AddWithValue("@offset", _cancelledPage * PageSize);

                        using (var reader = await cmd.ExecuteReaderAsync(token))
                        {
                            int i = _cancelledPage * PageSize;
                            while (await reader.ReadAsync(token))
                            {
                                rows.Add(new object[]
                                {
                                    ++i,
                                    SafeValue(reader["transno"]),
                                    SafeValue(reader["pcode"]),
                                    SafeValue(reader["pdesc"]),
                                    SafeValue(reader["price"]),
                                    SafeValue(reader["qty"]),
                                    SafeValue(reader["total"]),
                                    SafeValue(reader["sdate"]),
                                    SafeValue(reader["voidby"]),
                                    SafeValue(reader["cancelledby"]),
                                    SafeValue(reader["reason"]),
                                    "Print Slip"
                                });
                            }
                        }
                    }
                }

                if (token.IsCancellationRequested) return;

                UI(() =>
                {
                    dataGridView5.SuspendLayout();
                    dataGridView5.Rows.Clear();
                    foreach (var row in rows) dataGridView5.Rows.Add(row);
                    dataGridView5.ResumeLayout();

                    _lastCancelSearch = currentSearch;
                    RefreshPagingPanels();
                });
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, stitle);
            }
            finally
            {
                Interlocked.Exchange(ref _isCancelLoading, 0);
            }
        }

        public async Task LoadStockHistoryAsync()
        {
            if (Interlocked.CompareExchange(ref _isStockLoading, 1, 0) != 0) return;

            _stockCts?.Cancel();
            _stockCts?.Dispose();
            _stockCts = new CancellationTokenSource();
            var token = _stockCts.Token;

            var rows = new List<object[]>();

            try
            {
                NormalizeDates(dateTimePicker8.Value, dateTimePicker7.Value, out DateTime d1, out DateTime d2);

                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    await cn.OpenAsync(token);

                    string countSql = @"
                        SELECT COUNT(*)
                        FROM tblStockIn s
                        WHERE date(s.sdate) BETWEEN date(@d1) AND date(@d2)
                          AND s.status = 'Done'";

                    _stockTotalCount = await GetScalarCountAsync(cn, countSql, cmd =>
                    {
                        cmd.Parameters.AddWithValue("@d1", d1.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@d2", d2.ToString("yyyy-MM-dd"));
                    }, token);

                    string sql = @"
                        SELECT s.id, s.refno, s.pcode, p.pdesc, s.qty, s.sdate, s.stockinby
                        FROM tblStockIn s
                        INNER JOIN TblProduct1 p ON s.pcode = p.pcode
                        WHERE date(s.sdate) BETWEEN date(@d1) AND date(@d2)
                          AND s.status = 'Done'
                        ORDER BY date(s.sdate) DESC, s.id DESC
                        LIMIT @limit OFFSET @offset";

                    using (var cmd = new SQLiteCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("@d1", d1.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@d2", d2.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@limit", PageSize);
                        cmd.Parameters.AddWithValue("@offset", _stockPage * PageSize);

                        using (var reader = await cmd.ExecuteReaderAsync(token))
                        {
                            int i = _stockPage * PageSize;
                            while (await reader.ReadAsync(token))
                            {
                                rows.Add(new object[]
                                {
                                    ++i,
                                    reader["id"],
                                    reader["refno"],
                                    reader["pcode"],
                                    reader["pdesc"],
                                    reader["qty"],
                                    reader["sdate"],
                                    reader["stockinby"]
                                });
                            }
                        }
                    }
                }

                if (token.IsCancellationRequested) return;

                UI(() =>
                {
                    dataGridView6.SuspendLayout();
                    dataGridView6.Rows.Clear();
                    foreach (var row in rows) dataGridView6.Rows.Add(row);
                    dataGridView6.ResumeLayout();

                    RefreshPagingPanels();
                });
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, stitle);
            }
            finally
            {
                Interlocked.Exchange(ref _isStockLoading, 0);
            }
        }

        #endregion

        #region Event Handlers
        private void textboxinventorysearch_TextChanged(object sender, EventArgs e)
        {
            TriggerSearch();
        }

        private void txtSearchCritical_TextChanged(object sender, EventArgs e)
        {
            TriggerSearch();
        }

        private void textsearchTransNo_TextChanged(object sender, EventArgs e)
        {
            TriggerSearch();
        }

        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _stockPage = 0;
            _ = LoadStockHistoryAsync();
        }

        private void linkLabel_PrintStock_Click(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PrintGrid(dataGridView6, true, "STOCK IN HISTORY", BuildDateRangeText(dateTimePicker8.Value, dateTimePicker7.Value));
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel_PrintStock_Click(sender, e);
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _topSellingPage = 0;
            _ = LoadTopSellingAsync();
        }

        private void linkLabel9_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _cancelledPage = 0;
            _ = LoadCancelledOrdersAsync();
        }

        private void linkLabel12_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _inventoryPage = 0;
            _ = LoadInventoryAsync();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PrintGrid(dataGridView4, true, "INVENTORY LIST", "Current Inventory Snapshot");
        }

        private void linkLabel10_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PrintGrid(dataGridView5, true, "CANCELLED ORDER HISTORY", BuildDateRangeText(dateTimePicker5.Value, dateTimePicker6.Value));
        }

        private async void linkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                NormalizeDates(dateTimePicker4.Value, dateTimePicker3.Value, out DateTime d1, out DateTime d2);

                string sql = $@"
                    SELECT pdesc, SUM(total) AS total
                    FROM vwSoldItems
                    WHERE status = 'Sold'
                      AND date(sdate) BETWEEN date('{d1:yyyy-MM-dd}') AND date('{d2:yyyy-MM-dd}')
                    GROUP BY pdesc
                    ORDER BY SUM(total) DESC";

                frmCharts chartForm = new frmCharts();
                await chartForm.LoadChartAsync(
                    sql,
                    "SOLD ITEMS CHART",
                    "Sold Items",
                    "pdesc",
                    "total",
                    SeriesChartType.Pie,
                    true,
                    "ITEM LIST");

                chartForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, stitle);
            }
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PrintGrid(dataGridView1, false, "TOP SELLING ITEMS", BuildDateRangeText(dateTimePicker1.Value, dateTimePicker2.Value));
        }

        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _soldItemsPage = 0;
            _ = LoadSoldSummaryAsync();
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PrintGrid(dataGridView2, true, "SOLD ITEMS", BuildDateRangeText(dateTimePicker4.Value, dateTimePicker3.Value));
        }

        private void linkLabel11_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _criticalPage = 0;
            _ = LoadCriticalItemsAsync();
        }

        private void dataGridView5_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (dataGridView5.Columns[e.ColumnIndex].HeaderText == "Print Slip" ||
                dataGridView5.Columns[e.ColumnIndex].Name == "Print Slip" ||
                dataGridView5.Columns[e.ColumnIndex].HeaderText == "ACTION" ||
                dataGridView5.Columns[e.ColumnIndex].Name == "Column16")
            {
                DataGridViewRow row = dataGridView5.Rows[e.RowIndex];

                PrintCancellationInvoice(
                    SafeValue(row.Cells[1].Value),
                    SafeValue(row.Cells[2].Value),
                    SafeValue(row.Cells[3].Value),
                    SafeValue(row.Cells[4].Value),
                    SafeValue(row.Cells[5].Value),
                    SafeValue(row.Cells[6].Value),
                    SafeValue(row.Cells[9].Value),
                    SafeValue(row.Cells[10].Value),
                    SafeValue(row.Cells[7].Value)
                );
            }
        }

        private void PrintGrid(DataGridView dgv, bool isLandscape, string reportTitle, string reportSubTitle)
        {
            if (dgv == null) return;

            LoadStoreHeader();

            dgvToPrint = dgv;
            checkRow = 0;
            _printTitle = reportTitle;
            _printSubTitle = reportSubTitle ?? string.Empty;

            printDoc.DefaultPageSettings.Landscape = isLandscape;
            printDoc.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(35, 35, 40, 40);

            using (var dlg = new PrintPreviewDialog())
            {
                dlg.Document = printDoc;
                dlg.UseAntiAlias = true;
                dlg.WindowState = FormWindowState.Maximized;
                dlg.ShowDialog();
            }
        }
        #endregion

        #region Printing Engine
        private void LoadStoreHeader()
        {
            _printStoreName = "POS SYSTEM";
            _printStoreAddress = "";

            try
            {
                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    cn.Open();
                    using (var cmd = new SQLiteCommand("SELECT store, address FROM tblStore LIMIT 1", cn))
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            _printStoreName = SafeValue(dr["store"]).ToUpperInvariant();
                            _printStoreAddress = SafeValue(dr["address"]);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (dgvToPrint == null) return;

            Graphics g = e.Graphics;
            float x = e.MarginBounds.Left;
            float y = e.MarginBounds.Top;
            float cellHeight = 28;

            g.DrawString(_printStoreName, titleFont, Brushes.DarkSlateGray, x, y);
            y += 26;

            if (!string.IsNullOrWhiteSpace(_printStoreAddress))
            {
                g.DrawString(_printStoreAddress, cFont, Brushes.Black, x, y);
                y += 18;
            }

            g.DrawString(_printTitle, hFont, Brushes.Black, x, y);
            y += 18;

            if (!string.IsNullOrWhiteSpace(_printSubTitle))
            {
                g.DrawString(_printSubTitle, dateFont, Brushes.Black, x, y);
                y += 16;
            }

            g.DrawString("Printed on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"), dateFont, Brushes.Black, x, y);
            y += 24;

            float totalGridWidth = 0;
            foreach (DataGridViewColumn col in dgvToPrint.Columns)
            {
                if (col.Visible && col.HeaderText != "Print Slip" && col.HeaderText != "ACTION")
                    totalGridWidth += col.Width;
            }

            if (totalGridWidth <= 0) return;

            float scale = (float)e.MarginBounds.Width / totalGridWidth;

            float curX = x;
            foreach (DataGridViewColumn col in dgvToPrint.Columns)
            {
                if (!col.Visible || col.HeaderText == "Print Slip" || col.HeaderText == "ACTION") continue;

                float pWidth = col.Width * scale;
                g.FillRectangle(Brushes.LightGray, curX, y, pWidth, cellHeight);
                g.DrawRectangle(Pens.Black, curX, y, pWidth, cellHeight);
                g.DrawString(col.HeaderText, hFont, Brushes.Black,
                    new RectangleF(curX + 2, y + 5, pWidth - 4, cellHeight));
                curX += pWidth;
            }
            y += cellHeight;

            if (dgvToPrint.Rows.Count == 0)
            {
                curX = x;
                foreach (DataGridViewColumn col in dgvToPrint.Columns)
                {
                    if (!col.Visible || col.HeaderText == "Print Slip" || col.HeaderText == "ACTION") continue;

                    float pWidth = col.Width * scale;
                    g.DrawRectangle(gridPen, curX, y, pWidth, cellHeight);

                    if (Math.Abs(curX - x) < 1)
                    {
                        g.DrawString("No Data Found", cFont, Brushes.Gray,
                            new RectangleF(curX + 2, y + 5, pWidth - 4, cellHeight));
                    }

                    curX += pWidth;
                }

                return;
            }

            for (int i = checkRow; i < dgvToPrint.Rows.Count; i++)
            {
                DataGridViewRow row = dgvToPrint.Rows[i];
                if (row.IsNewRow) continue;

                curX = x;

                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (!cell.OwningColumn.Visible ||
                        cell.OwningColumn.HeaderText == "Print Slip" ||
                        cell.OwningColumn.HeaderText == "ACTION")
                        continue;

                    float pWidth = cell.OwningColumn.Width * scale;
                    g.DrawRectangle(gridPen, curX, y, pWidth, cellHeight);

                    string val = SafeValue(cell.Value);
                    g.DrawString(val, cFont, Brushes.Black,
                        new RectangleF(curX + 2, y + 5, pWidth - 4, cellHeight));

                    curX += pWidth;
                }

                y += cellHeight;
                checkRow++;

                if (y > e.MarginBounds.Bottom - 20)
                {
                    e.HasMorePages = true;
                    return;
                }
            }

            checkRow = 0;
        }

        public void PrintCancellationInvoice(string transno, string pcode, string pdesc, string price, string qty, string total, string user, string reason, string date)
        {
            string store = "POS SYSTEM";
            string addr = "";

            try
            {
                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    cn.Open();
                    using (var cmd = new SQLiteCommand("SELECT store, address FROM tblStore LIMIT 1", cn))
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            store = SafeValue(dr["store"]).ToUpperInvariant();
                            addr = SafeValue(dr["address"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, stitle);
            }

            PrintDocument doc = new PrintDocument();
            doc.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(35, 35, 40, 40);
            doc.PrintPage += (s, ev) =>
            {
                Graphics g = ev.Graphics;
                float x = ev.MarginBounds.Left;
                float y = ev.MarginBounds.Top;

                using (Font sFont = new Font("Segoe UI", 16, FontStyle.Bold))
                using (Font aFont = new Font("Segoe UI", 9))
                using (Font vFont = new Font("Segoe UI", 11, FontStyle.Bold))
                using (Font rFont = new Font("Segoe UI", 9, FontStyle.Italic))
                {
                    g.DrawString(store, sFont, Brushes.Black, x, y);
                    y += 28;

                    if (!string.IsNullOrWhiteSpace(addr))
                    {
                        g.DrawString(addr, aFont, Brushes.Black, x, y);
                        y += 22;
                    }

                    g.DrawString("VOID INVOICE: " + transno, vFont, Brushes.Red, x, y);
                    y += 28;
                    g.DrawString("Item: " + pdesc, aFont, Brushes.Black, x, y);
                    y += 18;
                    g.DrawString("Qty: " + qty + "    Total: " + total, aFont, Brushes.Black, x, y);
                    y += 18;
                    g.DrawString("Date: " + date, aFont, Brushes.Black, x, y);
                    y += 18;
                    g.DrawString("Cancelled By: " + user, aFont, Brushes.Black, x, y);
                    y += 18;
                    g.DrawString("Reason: " + reason, rFont, Brushes.Black, x, y);
                }
            };

            using (var dlg = new PrintPreviewDialog())
            {
                dlg.Document = doc;
                dlg.UseAntiAlias = true;
                dlg.WindowState = FormWindowState.Maximized;
                dlg.ShowDialog();
            }

            doc.Dispose();
        }
        #endregion

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                searchDelayTimer?.Stop();
                realTimeTimer?.Stop();

                searchDelayTimer?.Dispose();
                realTimeTimer?.Dispose();

                _topSellingCts?.Cancel();
                _topSellingCts?.Dispose();

                _soldCts?.Cancel();
                _soldCts?.Dispose();

                _criticalCts?.Cancel();
                _criticalCts?.Dispose();

                _inventoryCts?.Cancel();
                _inventoryCts?.Dispose();

                _cancelCts?.Cancel();
                _cancelCts?.Dispose();

                _stockCts?.Cancel();
                _stockCts?.Dispose();

                printDoc.PrintPage -= PrintDocument_PrintPage;

                if (_inventoryChangedHandler != null)
                    DataEvents.InventoryChanged -= _inventoryChangedHandler;
            }
            catch
            {
            }

            base.OnFormClosed(e);
        }
    }
}