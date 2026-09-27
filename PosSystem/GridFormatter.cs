using System.Windows.Forms;

namespace PosSystem
{
    public static class GridFormatter
    {
        public static void Format(DataGridView grid)
        {
            if (grid == null) return;

            // Hide technical columns
            if (grid.Columns.Contains("id"))
                grid.Columns["id"].Visible = false;

            // Rename headers for clarity
            if (grid.Columns.Contains("vendor"))
                grid.Columns["vendor"].HeaderText = "Vendor Name";

            // Standard UI settings
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ReadOnly = true;

            // Optional: enable double buffering to reduce flicker
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.SetProperty,
                null, grid, new object[] { true });
        }
    }
}