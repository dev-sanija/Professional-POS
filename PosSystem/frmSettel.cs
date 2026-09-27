using System;
using System.Data;
using System.Data.SQLite;
using System.Runtime.InteropServices; // Required for Draggable Logic
using System.Windows.Forms;

namespace PosSystem
{
    public partial class frmSettel : Form
    {
        // --- DRAGGABLE LOGIC (WinAPI) ---
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private void panelTitle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        // -------------------------------

        private readonly string connectionString = DBConnection.MyConnection();
        private Form1 f;
        private frmPOS fPOS;

        private decimal tempValue = 0;           // For arithmetic operations
        private string currentOperator = "";     // +, -, *, /
        private bool operatorClicked = false;    // Track if operator was clicked

        public frmSettel(Form1 fp)
        {
            InitializeComponent();
            f = fp;
            KeyPreview = true;

            // SAFE: only wire missing events here
            btnAdd.Click += btnOperator_Click;
            btnSubtract.Click += btnOperator_Click;
            btnMultiply.Click += btnOperator_Click;
            btnDivide.Click += btnOperator_Click;

            panel1.MouseDown += panelTitle_MouseDown;
        }

        public frmSettel(frmPOS fp)
        {
            InitializeComponent();
            fPOS = fp;
            KeyPreview = true;

            // SAFE: only wire missing events here
            btnAdd.Click += btnOperator_Click;
            btnSubtract.Click += btnOperator_Click;
            btnMultiply.Click += btnOperator_Click;
            btnDivide.Click += btnOperator_Click;

            panel1.MouseDown += panelTitle_MouseDown;
        }

        private void frmSettel_Load(object sender, EventArgs e)
        {
            DisableButtonTabStops();

            BeginInvoke((MethodInvoker)(() =>
            {
                this.ActiveControl = txtCash;
                txtCash.Focus();
                txtCash.SelectionStart = txtCash.Text.Length;
                txtCash.SelectionLength = 0;
            }));
        }

        private void pictureBox2_Click(object sender, EventArgs e) => this.Dispose();

        private void DisableButtonTabStops()
        {
            btn0.TabStop = false;
            btn1.TabStop = false;
            btn2.TabStop = false;
            btn3.TabStop = false;
            btn4.TabStop = false;
            btn5.TabStop = false;
            btn6.TabStop = false;
            btn7.TabStop = false;
            btn8.TabStop = false;
            btn9.TabStop = false;
            btn00.TabStop = false;
            btnC.TabStop = false;
            btnEnter.TabStop = false;
            btnAdd.TabStop = false;
            btnSubtract.TabStop = false;
            btnMultiply.TabStop = false;
            btnDivide.TabStop = false;
        }

        private void KeepCashFocus()
        {
            if (IsDisposed) return;

            txtCash.Focus();
            txtCash.SelectionStart = txtCash.Text.Length;
            txtCash.SelectionLength = 0;
        }

        #region Numeric & Math Button Handlers

        private void btnNumber_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            if (operatorClicked)
            {
                txtCash.Clear();
                operatorClicked = false;
            }

            txtCash.Text += btn.Text;
            KeepCashFocus();
        }

        private void btnC_Click(object sender, EventArgs e)
        {
            txtCash.Clear();
            tempValue = 0;
            currentOperator = "";
            operatorClicked = false;
            KeepCashFocus();
        }

        private void btnDecimal_Click(object sender, EventArgs e)
        {
            if (!txtCash.Text.Contains("."))
                txtCash.Text += ".";

            KeepCashFocus();
        }

        private void btnOperator_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            string op = btn.Text == "x" ? "*" : btn.Text;

            if (decimal.TryParse(txtCash.Text, out decimal val))
            {
                if (!string.IsNullOrEmpty(currentOperator) && !operatorClicked)
                {
                    val = Calculate(tempValue, val, currentOperator);
                    txtCash.Text = val.ToString("0.00");
                }

                tempValue = val;
                currentOperator = op;
                operatorClicked = true;
            }
            else
            {
                currentOperator = op;
                operatorClicked = true;
            }

            KeepCashFocus();
        }

        private void btnEquals_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtCash.Text, out decimal secondValue))
                secondValue = tempValue;

            decimal result = Calculate(tempValue, secondValue, currentOperator);
            txtCash.Text = result.ToString("0.00");
            tempValue = 0;
            currentOperator = "";
            operatorClicked = false;

            KeepCashFocus();
        }

        private decimal Calculate(decimal first, decimal second, string op)
        {
            return op switch
            {
                "+" => first + second,
                "-" => first - second,
                "*" => first * second,
                "/" => second != 0 ? first / second : 0,
                _ => second
            };
        }

        #endregion

        private void txtCash_TextChanged(object sender, EventArgs e)
        {
            try
            {
                decimal sale = decimal.TryParse(txtSale.Text, out decimal s) ? s : 0;
                decimal cash = decimal.TryParse(txtCash.Text, out decimal c) ? c : 0;
                decimal change = cash - sale;
                txtChange.Text = (change < 0) ? "0.00" : change.ToString("0.00");
            }
            catch
            {
                txtChange.Text = "0.00";
            }
        }

        private void btnEnter_Click(object sender, EventArgs e)
        {
            string currentTransNo = fPOS != null ? fPOS.lblTransno.Text : f.lblTransno.Text;
            DataGridView dgv = fPOS != null ? fPOS.dataGridView1 : f.dataGridView1;

            if (dgv.Rows.Count == 0) return;

            decimal saleAmount = decimal.TryParse(txtSale.Text, out decimal s) ? s : 0;
            decimal cashAmount = decimal.TryParse(txtCash.Text, out decimal c) ? c : 0;

            if (cashAmount < saleAmount)
            {
                MessageBox.Show("Insufficient amount. Please enter the correct amount!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                KeepCashFocus();
                return;
            }

            try
            {
                using (SQLiteConnection cn = new SQLiteConnection(connectionString))
                {
                    cn.Open();
                    using (var transaction = cn.BeginTransaction())
                    {
                        foreach (DataGridViewRow row in dgv.Rows)
                        {
                            if (row.IsNewRow) continue;

                            string pcode = row.Cells[2].Value?.ToString();
                            int soldQty = int.TryParse(row.Cells[5].Value?.ToString(), out int q) ? q : 0;
                            int cartId = int.TryParse(row.Cells[1].Value?.ToString(), out int id) ? id : 0;

                            using (var cmdCheck = new SQLiteCommand("SELECT qty FROM TblProduct1 WHERE pcode=@pcode", cn))
                            {
                                cmdCheck.Parameters.AddWithValue("@pcode", pcode);
                                int stockQty = Convert.ToInt32(cmdCheck.ExecuteScalar() ?? 0);

                                if (soldQty > stockQty)
                                {
                                    MessageBox.Show($"Not enough stock for product {pcode}. Remaining: {stockQty}",
                                        "Stock Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);

                                    transaction.Rollback();
                                    KeepCashFocus();
                                    return;
                                }
                            }

                            using (var cmdUpdateCart = new SQLiteCommand("UPDATE tblCart1 SET status='Sold' WHERE id=@id", cn))
                            {
                                cmdUpdateCart.Parameters.AddWithValue("@id", cartId);
                                cmdUpdateCart.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                }

                MessageBox.Show("Payment successfully saved!", "Payment", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                KeepCashFocus();
            }
        }

        #region Keyboard Support
        private void frmSettel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                this.Dispose();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                btnEnter_Click(sender, e);
            }
            else if ((e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) || (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9))
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                if (operatorClicked)
                {
                    txtCash.Clear();
                    operatorClicked = false;
                }

                string number = e.KeyCode.ToString().Replace("D", "").Replace("NumPad", "");
                txtCash.Text += number;
                KeepCashFocus();
            }
            else if (e.KeyCode == Keys.Back)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                if (txtCash.Text.Length > 0)
                    txtCash.Text = txtCash.Text.Substring(0, txtCash.Text.Length - 1);

                KeepCashFocus();
            }
            else if (e.KeyCode == Keys.Decimal || e.KeyCode == Keys.OemPeriod)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                if (!txtCash.Text.Contains("."))
                    txtCash.Text += ".";

                KeepCashFocus();
            }
            else if (e.KeyCode == Keys.Add)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                btnOperator_Click(btnAdd, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.Subtract)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                btnOperator_Click(btnSubtract, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.Multiply)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                btnOperator_Click(btnMultiply, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.Divide)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                btnOperator_Click(btnDivide, EventArgs.Empty);
            }
        }

        private void txtSale_TextChanged(object sender, EventArgs e)
        {
            txtCash_TextChanged(sender, e);
        }

        #endregion

        private void btnprinterwindow_Click(object sender, EventArgs e)
        {
            try
            {
                Form activeForm = fPOS != null ? (Form)fPOS : (Form)f;
                string loggedInUser = fPOS != null ? fPOS.LblUser.Text : f.lblUserName.Text;

                using (frmResipt frm = new frmResipt(activeForm))
                {
                    frm.LoadReport(txtCash.Text, txtChange.Text, loggedInUser);
                    frm.ShowDialog();
                }

                KeepCashFocus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open receipt preview: " + ex.Message,
                    "Printer Preview",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                KeepCashFocus();
            }
        }
        private void btn1_Click(object sender, EventArgs e) => btnNumber_Click(sender, e);
        private void btn2_Click(object sender, EventArgs e) => btnNumber_Click(sender, e);
        private void btn3_Click(object sender, EventArgs e) => btnNumber_Click(sender, e);
        private void btn4_Click(object sender, EventArgs e) => btnNumber_Click(sender, e);
        private void btn5_Click(object sender, EventArgs e) => btnNumber_Click(sender, e);
        private void btn6_Click(object sender, EventArgs e) => btnNumber_Click(sender, e);
        private void btn7_Click(object sender, EventArgs e) => btnNumber_Click(sender, e);
        private void btn8_Click(object sender, EventArgs e) => btnNumber_Click(sender, e);
        private void btn9_Click(object sender, EventArgs e) => btnNumber_Click(sender, e);
        private void btn0_Click(object sender, EventArgs e) => btnNumber_Click(sender, e);
        private void btn00_Click(object sender, EventArgs e) => btnNumber_Click(sender, e);
    }
}