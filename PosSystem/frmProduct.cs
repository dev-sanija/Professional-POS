using System;
using System.Data;
using System.Data.SQLite;
using System.Runtime.InteropServices; // Required for Draggable Logic
using System.Windows.Forms;

namespace PosSystem
{
    public partial class frmProduct : Form
    {
        #region Win32 API for Draggable Form
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

        frmProduct_List flist;
        frmAdjustment fadjustment;
        private string _originalPcode = string.Empty;

        public frmProduct(frmProduct_List frm)
        {
            InitializeComponent();
            flist = frm;
        }

        public frmProduct(frmAdjustment frm)
        {
            InitializeComponent();
            fadjustment = frm;
        }

        public void LocalCategory()
        {
            comboBox2.Items.Clear();
            comboBox2.Items.Add("NONE");

            using (SQLiteConnection con = new SQLiteConnection(DBConnection.MyConnection()))
            {
                con.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT category FROM TblCategory WHERE id != 0 ORDER BY category", con))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        comboBox2.Items.Add(reader[0].ToString());
                    }
                }
            }
        }

        public void LocalBrand()
        {
            comboBox1.Items.Clear();
            using (SQLiteConnection con = new SQLiteConnection(DBConnection.MyConnection()))
            {
                con.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT brand FROM BrandTbl ORDER BY brand", con))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        comboBox1.Items.Add(reader[0].ToString());
                    }
                }
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void frmProduct_Load(object sender, EventArgs e)
        {
            if (comboBox1.Items.Count == 0) LocalBrand();
            if (comboBox2.Items.Count == 0) LocalCategory();

            if (btnUpdate.Enabled)
            {
                TxtPcode.ReadOnly = true;
                if (string.IsNullOrWhiteSpace(_originalPcode))
                    _originalPcode = TxtPcode.Text.Trim();
            }
            else
            {
                TxtPcode.ReadOnly = false;
            }
        }

        public async System.Threading.Tasks.Task<bool> LoadProductForUpdateAsync(string pcode)
        {
            if (string.IsNullOrWhiteSpace(pcode))
                return false;

            try
            {
                LocalBrand();
                LocalCategory();

                using (SQLiteConnection connection = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    await connection.OpenAsync();

                    using (SQLiteCommand cmd = new SQLiteCommand(@"
                        SELECT 
                            p.pcode,
                            p.barcode,
                            p.pdesc,
                            IFNULL(b.brand, '') AS brand,
                            CASE 
                                WHEN IFNULL(p.cid, 0) = 0 THEN 'NONE'
                                ELSE IFNULL(c.category, 'NONE')
                            END AS category,
                            p.price,
                            p.reorder
                        FROM TblProduct1 p
                        LEFT JOIN BrandTbl b ON b.id = p.bid
                        LEFT JOIN TblCategory c ON c.id = p.cid
                        WHERE p.pcode = @pcode
                        LIMIT 1", connection))
                    {
                        cmd.Parameters.AddWithValue("@pcode", pcode);

                        using (SQLiteDataReader reader = (SQLiteDataReader)await cmd.ExecuteReaderAsync())
                        {
                            if (!await reader.ReadAsync())
                            {
                                MessageBox.Show("Selected product was not found.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return false;
                            }

                            _originalPcode = reader["pcode"].ToString();

                            TxtPcode.Text = reader["pcode"].ToString();
                            txtBarcode.Text = reader["barcode"].ToString();
                            txtPdesc.Text = reader["pdesc"].ToString();
                            comboBox1.Text = reader["brand"].ToString();
                            comboBox2.Text = reader["category"].ToString();
                            txtPrice.Text = Convert.ToDouble(reader["price"]).ToString("0.##");
                            txtReOrder.Text = Convert.ToInt32(reader["reorder"]).ToString();

                            btnSave.Enabled = false;
                            btnUpdate.Enabled = true;
                            TxtPcode.ReadOnly = true;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading product: " + ex.Message, "POS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool TryResolveBrandAndCategory(SQLiteConnection connection, out int brandID, out int categoryID)
        {
            brandID = 0;
            categoryID = 0;

            if (string.IsNullOrWhiteSpace(comboBox1.Text))
            {
                MessageBox.Show("Please select a Brand.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBox1.Focus();
                return false;
            }

            using (SQLiteCommand cmd = new SQLiteCommand("SELECT id FROM BrandTbl WHERE brand = @brand", connection))
            {
                cmd.Parameters.AddWithValue("@brand", comboBox1.Text.Trim());
                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    MessageBox.Show("Selected Brand was not found.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    comboBox1.Focus();
                    return false;
                }
                brandID = Convert.ToInt32(result);
            }

            if (string.IsNullOrWhiteSpace(comboBox2.Text) || comboBox2.Text == "NONE")
            {
                categoryID = 0;
                return true;
            }

            using (SQLiteCommand cmd = new SQLiteCommand("SELECT id FROM TblCategory WHERE category = @category", connection))
            {
                cmd.Parameters.AddWithValue("@category", comboBox2.Text.Trim());
                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    MessageBox.Show("Selected Category was not found.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    comboBox2.Focus();
                    return false;
                }
                categoryID = Convert.ToInt32(result);
            }

            return true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TxtPcode.Text) || string.IsNullOrEmpty(txtBarcode.Text) || string.IsNullOrEmpty(txtPdesc.Text))
                {
                    MessageBox.Show("Please fill in required fields.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (comboBox1.SelectedIndex < 0 || comboBox2.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select a Brand and Category.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!double.TryParse(txtPrice.Text, out double price))
                {
                    MessageBox.Show("Invalid Price value.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPrice.Focus();
                    return;
                }

                if (!int.TryParse(txtReOrder.Text, out int reorder))
                {
                    MessageBox.Show("Invalid Reorder value.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtReOrder.Focus();
                    return;
                }

                if (MessageBox.Show("Save this product?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(DBConnection.MyConnection()))
                    {
                        connection.Open();

                        using (SQLiteCommand cmdCheck = new SQLiteCommand(
                            "SELECT COUNT(*) FROM TblProduct1 WHERE barcode = @barcode", connection))
                        {
                            cmdCheck.Parameters.AddWithValue("@barcode", txtBarcode.Text.Trim());
                            int count = Convert.ToInt32(cmdCheck.ExecuteScalar());
                            if (count > 0)
                            {
                                MessageBox.Show("Barcode already exists!", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }

                        if (!TryResolveBrandAndCategory(connection, out int brandID, out int categoryID))
                            return;

                        using (SQLiteCommand cmd = new SQLiteCommand(
                            @"INSERT INTO TblProduct1 (pcode, barcode, pdesc, bid, cid, price, reorder) 
                              VALUES (@pcode, @barcode, @pdesc, @bid, @cid, @price, @reorder)", connection))
                        {
                            cmd.Parameters.AddWithValue("@pcode", TxtPcode.Text.Trim());
                            cmd.Parameters.AddWithValue("@barcode", txtBarcode.Text.Trim());
                            cmd.Parameters.AddWithValue("@pdesc", txtPdesc.Text.Trim());
                            cmd.Parameters.AddWithValue("@bid", brandID);
                            cmd.Parameters.AddWithValue("@cid", categoryID);
                            cmd.Parameters.AddWithValue("@price", price);
                            cmd.Parameters.AddWithValue("@reorder", reorder);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Product saved successfully!");
                    Clear();

                    if (flist != null) flist.LoadRecords();
                    if (fadjustment != null) _ = fadjustment.LoadRecordsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public void Clear()
        {
            txtPrice.Clear();
            txtBarcode.Clear();
            txtPdesc.Clear();
            TxtPcode.Clear();
            txtReOrder.Clear();
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            TxtPcode.ReadOnly = false;
            _originalPcode = string.Empty;
            TxtPcode.Focus();
            btnSave.Enabled = true;
            btnUpdate.Enabled = false;
        }

        public void Clear1()
        {
            txtPrice.Clear();
            txtBarcode.Clear();
            txtPdesc.Clear();
            TxtPcode.Clear();
            txtReOrder.Clear();
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            TxtPcode.ReadOnly = true;
            TxtPcode.Focus();
            btnSave.Enabled = false;
            btnUpdate.Enabled = true;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to update this product?", "Update product", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (string.IsNullOrWhiteSpace(TxtPcode.Text) || string.IsNullOrWhiteSpace(txtBarcode.Text) || string.IsNullOrWhiteSpace(txtPdesc.Text))
                    {
                        MessageBox.Show("Please fill in required fields.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (!double.TryParse(txtPrice.Text, out double price))
                    {
                        MessageBox.Show("Invalid Price value.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtPrice.Focus();
                        return;
                    }

                    if (!int.TryParse(txtReOrder.Text, out int reorder))
                    {
                        MessageBox.Show("Invalid Reorder value.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtReOrder.Focus();
                        return;
                    }

                    using (SQLiteConnection connection = new SQLiteConnection(DBConnection.MyConnection()))
                    {
                        connection.Open();

                        string targetPcode = string.IsNullOrWhiteSpace(_originalPcode) ? TxtPcode.Text.Trim() : _originalPcode;

                        using (SQLiteCommand cmdCheck = new SQLiteCommand(
                            "SELECT COUNT(*) FROM TblProduct1 WHERE barcode = @barcode AND pcode != @pcode", connection))
                        {
                            cmdCheck.Parameters.AddWithValue("@barcode", txtBarcode.Text.Trim());
                            cmdCheck.Parameters.AddWithValue("@pcode", targetPcode);
                            int count = Convert.ToInt32(cmdCheck.ExecuteScalar());
                            if (count > 0)
                            {
                                MessageBox.Show("Barcode already assigned to another product!", "Duplicate Barcode", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }

                        if (!TryResolveBrandAndCategory(connection, out int brandID, out int categoryID))
                            return;

                        using (SQLiteCommand cmd = new SQLiteCommand(
                            @"UPDATE TblProduct1 
                              SET barcode=@barcode, pdesc=@pdesc, bid=@bid, cid=@cid, price=@price, reorder=@reorder 
                              WHERE pcode=@pcode", connection))
                        {
                            cmd.Parameters.AddWithValue("@pcode", targetPcode);
                            cmd.Parameters.AddWithValue("@barcode", txtBarcode.Text.Trim());
                            cmd.Parameters.AddWithValue("@pdesc", txtPdesc.Text.Trim());
                            cmd.Parameters.AddWithValue("@bid", brandID);
                            cmd.Parameters.AddWithValue("@cid", categoryID);
                            cmd.Parameters.AddWithValue("@price", price);
                            cmd.Parameters.AddWithValue("@reorder", reorder);

                            int affected = cmd.ExecuteNonQuery();
                            if (affected <= 0)
                            {
                                MessageBox.Show("Product record not found or already removed.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                    }

                    MessageBox.Show("Product has been successfully updated.");

                    if (flist != null) flist.LoadRecords();
                    if (fadjustment != null) _ = fadjustment.LoadRecordsAsync();

                    this.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        Random rand = new Random();

        private void GenerateUniqueBarcode()
        {
            string newBarcode = "";
            bool isUnique = false;

            try
            {
                using (SQLiteConnection con = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    con.Open();

                    int attempts = 0;
                    while (!isUnique && attempts < 100)
                    {
                        attempts++;
                        newBarcode = "";
                        for (int i = 0; i < 12; i++)
                        {
                            newBarcode += rand.Next(0, 10).ToString();
                        }

                        using (SQLiteCommand cmd = new SQLiteCommand("SELECT COUNT(*) FROM TblProduct1 WHERE barcode = @barcode", con))
                        {
                            cmd.Parameters.AddWithValue("@barcode", newBarcode);
                            int count = Convert.ToInt32(cmd.ExecuteScalar());

                            if (count == 0)
                                isUnique = true;
                        }
                    }
                }

                if (isUnique)
                {
                    if (txtBarcode != null)
                    {
                        txtBarcode.Clear();
                        txtBarcode.Text = newBarcode;
                        txtBarcode.Invalidate();
                        txtBarcode.Update();
                        txtBarcode.Refresh();
                        txtBarcode.Focus();
                    }
                }
                else
                {
                    MessageBox.Show("Could not generate a unique barcode after 100 tries.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error: " + ex.Message);
            }
        }

        private void btncreaterandombarcode_Click(object sender, EventArgs e)
        {
            GenerateUniqueBarcode();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void txtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
                e.Handled = true;

            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
                e.Handled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            frmscanBarcode frm = new frmscanBarcode(this);
            frm.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            frmCreatebarcode FRM = new frmCreatebarcode();
            FRM.ShowDialog();
        }

        private void textBox1_TextChanged(object sender, EventArgs e) { }
    }
}