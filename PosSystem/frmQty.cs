using System;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PosSystem
{
    public partial class frmQty : Form
    {
        // 🔹 Product Data Variables
        private string pcode;
        private double price;
        private int stockOnHand; // Total physical stock available
        private string transno;

        // 🔹 Parent Form References
        private Form1 f;
        private frmPOS fPOS;

        public int Quantity { get; private set; }

        public frmQty(frmPOS frm)
        {
            InitializeComponent();
            this.fPOS = frm;
        }

        public frmQty(Form1 frm)
        {
            InitializeComponent();
            this.f = frm;
        }

        // ✅ UNIVERSAL DATA LOADER
        public void ProductDetails(string pcode, double price, string transno, int qty)
        {
            this.pcode = pcode;
            this.price = price;
            this.transno = transno;
            this.stockOnHand = qty;
        }

        private void frmQty_Load(object sender, EventArgs e)
        {
            txtQty.Focus();
        }

        private void txtQty_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 🔹 Triggered when Cashier presses ENTER
            if (e.KeyChar == (char)Keys.Enter)
            {
                ConfirmAndAdd();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // 🔹 Triggered if Cashier clicks the "Done/OK" button
            ConfirmAndAdd();
        }

        private void ConfirmAndAdd()
        {
            // 1️⃣ Validation: Check if input is a valid positive number
            if (string.IsNullOrWhiteSpace(txtQty.Text) || !int.TryParse(txtQty.Text, out int inputQty) || inputQty <= 0)
            {
                MessageBox.Show("Please enter a valid quantity.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtQty.SelectAll();
                txtQty.Focus();
                return;
            }

            try
            {
                using (var cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    cn.Open();

                    // 2️⃣ CHECK EXISTING CART QUANTITY (Prevent Null Errors)
                    int cartQty = 0;
                    string currentTransNo = fPOS?.lblTransno.Text ?? transno;

                    using (var cmd = new SQLiteCommand("SELECT SUM(qty) FROM tblCart1 WHERE pcode = @pcode AND transno = @transno", cn))
                    {
                        cmd.Parameters.AddWithValue("@pcode", pcode);
                        cmd.Parameters.AddWithValue("@transno", currentTransNo);

                        object result = cmd.ExecuteScalar();
                        // Elite Null Handling for SQLite SUM
                        cartQty = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
                    }

                    // 3️⃣ ELITE STOCK GUARD
                    // Check if: (What's already in cart + What user just typed) > Physical Stock
                    if (stockOnHand < (inputQty + cartQty))
                    {
                        MessageBox.Show($"UNABLE TO PROCEED!\n\n" +
                                        $"Stock on Hand: {stockOnHand}\n" +
                                        $"Already in Cart: {cartQty}\n" +
                                        $"Requested: {inputQty}\n\n" +
                                        $"You are exceeding available stock limits.", "Insufficient Stock", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        txtQty.SelectAll();
                        txtQty.Focus();
                        return;
                    }

                    // 4️⃣ DATABASE UPDATE OR INSERT
                    bool found = false;
                    string cartId = "";
                    using (var cmd = new SQLiteCommand("SELECT id FROM tblCart1 WHERE transno = @transno AND pcode = @pcode", cn))
                    {
                        cmd.Parameters.AddWithValue("@transno", currentTransNo);
                        cmd.Parameters.AddWithValue("@pcode", pcode);
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                found = true;
                                cartId = dr["id"].ToString();
                            }
                        }
                    }

                    if (found)
                    {
                        // Update existing row quantity
                        using (var cmd = new SQLiteCommand("UPDATE tblCart1 SET qty = qty + @qty WHERE id = @id", cn))
                        {
                            cmd.Parameters.AddWithValue("@qty", inputQty);
                            cmd.Parameters.AddWithValue("@id", cartId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Insert new product into cart
                        string user = fPOS?.LblUser.Text ?? f?.lblUserName.Text ?? "System";
                        using (var cmd = new SQLiteCommand("INSERT INTO tblCart1 (transno, pcode, price, qty, sdate, cashier) VALUES (@transno, @pcode, @price, @qty, @sdate, @cashier)", cn))
                        {
                            cmd.Parameters.AddWithValue("@transno", currentTransNo);
                            cmd.Parameters.AddWithValue("@pcode", pcode);
                            cmd.Parameters.AddWithValue("@price", price);
                            cmd.Parameters.AddWithValue("@qty", inputQty);
                            cmd.Parameters.AddWithValue("@sdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@cashier", user);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // 5️⃣ UI REFRESH & SUCCESS
                if (fPOS != null)
                {
                    fPOS.LoadCart();
                    fPOS.txtSearch.Clear();
                    fPOS.txtSearch.Focus();
                }

                this.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("System Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtQty_TextChanged(object sender, EventArgs e)
        {
            // 🔹 JUNK REMOVER: Keeps only numbers, removes letters instantly
            if (Regex.IsMatch(txtQty.Text, "[^0-9]"))
            {
                int selStart = txtQty.SelectionStart;
                txtQty.Text = Regex.Replace(txtQty.Text, "[^0-9]", "");
                txtQty.SelectionStart = Math.Max(0, selStart - 1);
            }
        }
    }
}