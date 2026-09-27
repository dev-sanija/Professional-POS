using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PosSystem
{
    public partial class frmDiscount : Form
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

        private readonly string stitle = "POS System";
        private readonly frmPOS fPOS;
        private readonly List<frmPOS.DiscountTarget> _targets = new List<frmPOS.DiscountTarget>();
        private bool _internalUpdate;

        public Action<double, double> OnDiscountApplied;

        public frmDiscount(frmPOS frm)
        {
            InitializeComponent();
            fPOS = frm;
            KeyPreview = true;

            // No designer change needed
            panel1.MouseDown += panel1_MouseDown;
        }

        public void SetDiscountTargets(List<frmPOS.DiscountTarget> targets)
        {
            _targets.Clear();

            if (targets != null)
            {
                foreach (frmPOS.DiscountTarget item in targets
                    .Where(x => x != null && x.CartId > 0 && x.Qty > 0)
                    .GroupBy(x => x.CartId)
                    .Select(g => g.First())
                    .OrderBy(x => x.CartId))
                {
                    _targets.Add(item);
                }
            }

            BindTargetsToForm();
        }

        private void BindTargetsToForm()
        {
            if (_targets.Count == 0)
                return;

            decimal totalPrice = _targets.Sum(x => x.Price * x.Qty);
            decimal currentDiscount = _targets.Sum(x => x.CurrentDiscountPerUnit * x.Qty);

            lblID.Text = _targets.Count == 1 ? _targets[0].CartId.ToString() : "MULTI";

            _internalUpdate = true;
            try
            {
                txtPrice.Text = totalPrice.ToString("0.00");
                txtAmount.Text = currentDiscount.ToString("0.00");
                CalculateFromAmount();
            }
            finally
            {
                _internalUpdate = false;
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private decimal ParseMoney(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0m;

            decimal.TryParse(text.Replace(",", "").Trim(), out decimal value);
            return value;
        }

        private decimal RoundMoney(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private decimal RoundCartValue(decimal value)
        {
            return Math.Round(value, 4, MidpointRounding.AwayFromZero);
        }

        private int GetTotalSelectedQty()
        {
            return _targets.Sum(x => x.Qty);
        }

        private decimal GetTotalSelectedPrice()
        {
            return _targets.Sum(x => x.Price * x.Qty);
        }

        private decimal GetMaxAllowedDiscountAmount()
        {
            if (_targets.Count == 0)
                return ParseMoney(txtPrice.Text);

            decimal minUnitPrice = _targets.Min(x => x.Price);
            int totalQty = GetTotalSelectedQty();

            // Keeps your "same per-unit discount across selected quantity" rule safe
            return RoundMoney(minUnitPrice * totalQty);
        }

        private void txtDiscount_TextChanged(object sender, EventArgs e)
        {
            if (_internalUpdate) return;

            if (txtDiscount.Focused)
            {
                CalculateFromPercentage();
            }
        }

        private void txtAmount_TextChanged(object sender, EventArgs e)
        {
            if (_internalUpdate) return;

            if (txtAmount.Focused)
            {
                CalculateFromAmount();
            }
        }

        private void CalculateFromPercentage()
        {
            try
            {
                decimal totalPrice = ParseMoney(txtPrice.Text);
                decimal percent = ParseMoney(txtDiscount.Text);

                if (percent < 0m) percent = 0m;
                if (percent > 100m) percent = 100m;

                decimal amount = RoundMoney(totalPrice * (percent / 100m));
                decimal maxAllowed = GetMaxAllowedDiscountAmount();

                if (amount > maxAllowed)
                    amount = maxAllowed;

                _internalUpdate = true;
                try
                {
                    txtAmount.Text = amount.ToString("0.00");
                }
                finally
                {
                    _internalUpdate = false;
                }
            }
            catch
            {
            }
        }

        private void CalculateFromAmount()
        {
            try
            {
                decimal totalPrice = ParseMoney(txtPrice.Text);
                decimal amount = ParseMoney(txtAmount.Text);

                if (amount < 0m) amount = 0m;

                decimal maxAllowed = GetMaxAllowedDiscountAmount();
                if (amount > maxAllowed)
                    amount = maxAllowed;

                decimal percent = 0m;
                if (totalPrice > 0m)
                    percent = (amount / totalPrice) * 100m;

                _internalUpdate = true;
                try
                {
                    txtAmount.Text = amount.ToString("0.00");
                    txtDiscount.Text = percent.ToString("0.00");
                }
                finally
                {
                    _internalUpdate = false;
                }
            }
            catch
            {
            }
        }

        private void txtPrice_TextChanged(object sender, EventArgs e)
        {
            if (_internalUpdate) return;
            CalculateFromPercentage();
        }

        private void EnsureTargetsLoaded()
        {
            if (_targets.Count == 0 && fPOS != null)
            {
                SetDiscountTargets(fPOS.GetSelectedDiscountTargets());
            }
        }

        private async void btnConfirm_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Add Discount? Click Yes to confirm.", stitle,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            btnConfirm.Enabled = false;
            UseWaitCursor = true;

            try
            {
                EnsureTargetsLoaded();

                if (_targets.Count == 0)
                    throw new Exception("Please select one or more cart rows first.");

                int totalQty = GetTotalSelectedQty();
                if (totalQty <= 0)
                    throw new Exception("Invalid selected quantity.");

                decimal totalPrice = GetTotalSelectedPrice();
                if (totalPrice <= 0m)
                    throw new Exception("Invalid selected total price.");

                decimal discPercent = RoundMoney(ParseMoney(txtDiscount.Text));
                decimal discAmount = RoundMoney(ParseMoney(txtAmount.Text));

                if (discAmount < 0m)
                    throw new Exception("Discount amount cannot be negative.");

                decimal maxAllowed = GetMaxAllowedDiscountAmount();
                if (discAmount > maxAllowed)
                    throw new Exception("Discount amount is too high for one or more selected item prices.");

                List<frmPOS.DiscountTarget> orderedTargets = _targets
                    .OrderBy(x => x.CartId)
                    .ToList();

                decimal remainingDiscount = RoundCartValue(discAmount);
                int remainingQty = totalQty;

                using (SQLiteConnection cn = new SQLiteConnection(DBConnection.MyConnection()))
                {
                    await cn.OpenAsync();

                    using (SQLiteTransaction tr = cn.BeginTransaction())
                    {
                        try
                        {
                            using (SQLiteCommand cm = new SQLiteCommand(@"
                                UPDATE tblCart1
                                SET disc = @disc,
                                    discount_per_unit = @discPerUnit,
                                    total = @total
                                WHERE id = @id AND status = 'Pending'", cn, tr))
                            {
                                SQLiteParameter pDisc = cm.Parameters.Add("@disc", System.Data.DbType.Decimal);
                                SQLiteParameter pDiscPerUnit = cm.Parameters.Add("@discPerUnit", System.Data.DbType.Decimal);
                                SQLiteParameter pTotal = cm.Parameters.Add("@total", System.Data.DbType.Decimal);
                                SQLiteParameter pId = cm.Parameters.Add("@id", System.Data.DbType.Int32);

                                for (int i = 0; i < orderedTargets.Count; i++)
                                {
                                    frmPOS.DiscountTarget target = orderedTargets[i];

                                    decimal lineDiscount;
                                    if (i == orderedTargets.Count - 1)
                                    {
                                        lineDiscount = remainingDiscount;
                                    }
                                    else
                                    {
                                        lineDiscount = RoundCartValue((remainingDiscount / remainingQty) * target.Qty);
                                    }

                                    if (lineDiscount < 0m)
                                        lineDiscount = 0m;

                                    decimal discountPerUnit = target.Qty > 0
                                        ? RoundCartValue(lineDiscount / target.Qty)
                                        : 0m;

                                    if (discountPerUnit > target.Price)
                                        throw new Exception("Discount per unit cannot exceed the item unit price.");

                                    decimal lineTotal = RoundCartValue((target.Price * target.Qty) - lineDiscount);
                                    if (lineTotal < 0m)
                                        throw new Exception("Discount calculation produced a negative line total.");

                                    pDisc.Value = lineDiscount;
                                    pDiscPerUnit.Value = discountPerUnit;
                                    pTotal.Value = lineTotal;
                                    pId.Value = target.CartId;

                                    int affected = await cm.ExecuteNonQueryAsync();
                                    if (affected <= 0)
                                        throw new Exception("One or more selected cart rows could not be updated.");

                                    remainingDiscount -= lineDiscount;
                                    remainingQty -= target.Qty;
                                }
                            }

                            tr.Commit();
                        }
                        catch
                        {
                            try { tr.Rollback(); } catch { }
                            throw;
                        }
                    }
                }

                OnDiscountApplied?.Invoke((double)discAmount, (double)discPercent);
                fPOS?.LoadCart();
                Dispose();
            }
            catch (Exception ex)
            {
                Program.SilentLog(ex, "frmDiscount_btnConfirm_Click");
                MessageBox.Show("Discount Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                UseWaitCursor = false;
                btnConfirm.Enabled = true;
            }
        }

        private void frmDiscount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Dispose();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                btnConfirm_Click(sender, e);
            }
        }

        private void frmDiscount_Load(object sender, EventArgs e)
        {
            txtAmount.ReadOnly = false;
            txtAmount.Enabled = true;

            EnsureTargetsLoaded();

            txtDiscount.Focus();
            txtDiscount.SelectAll();
        }

        private void CalculateDiscount() { }
    }
}