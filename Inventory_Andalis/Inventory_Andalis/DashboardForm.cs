using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;

namespace Inventory_Andalis
{
    public partial class DashboardForm : Form
    {

        private int selectedProductID = 0;
        public DashboardForm()
        {
            InitializeComponent();

           btnadd.Click += Btnadd_Click;
           btnupdate.Click += Btnupdate_Click;
           btndelete.Click += Btndelete_Click;
            btnclear.Click += Btnclear_Click;
           // btnrefresh.Click += Btnrefresh_Click;
            
            cmbcategory.DropDownStyle = ComboBoxStyle.DropDownList;

            //txtsearch.TextChanged += Txtsearch_TextChanged;
            dgvproducts.CellClick += Dgvproducts_CellClick;

            LoadProducts();
            LoadDashboardStats();

        }
        private void LoadDashboardStats()
        {
            try
            {
                using (OleDbConnection con = Database.GetConnection()) 
                {
                    con.Open();

                    string query = @"SELECT COUNT(*) AS TotalProducts, SUM(IIF(Quantity > ReorderLevel, 1, 0)) AS InStock, SUM(IIF(Quantity > 0 AND Quantity <= ReorderLevel, 1, 0 )) AS LowStock, SUM(IIF(Quantity = 0, 1, 0 )) AS OutofStock FROM Products";

                    using (OleDbCommand cmd = new OleDbCommand(query, con)) 
                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lbltotalcount.Text = reader["TotalProducts"].ToString();
                            lblinstock.Text = reader["InStock"].ToString();
                            lbllowstock.Text = reader["LowStock"].ToString();
                            lbloutofstock.Text = reader["OutofStock"].ToString();

                        }
                    }
                }
            }
            catch (Exception ex)
                {
                MessageBox.Show("Statistic error" + ex.Message);
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (OleDbConnection con = Database.GetConnection())
                { 
                    con.Open();
                    
                    string query = @"SELECT * FROM Products ORDER BY ProductID DESC";

                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();

                        adapter.Fill(dt);

                        dgvproducts.DataSource = dt;
                    }
                
                }    
            }
           catch (Exception ex) 
            {
                MessageBox.Show("Unable to load products" + ex.Message);
            }
        }

        private bool ValidateProduct()
        {
            if (string.IsNullOrEmpty(txtproductname.Text))
            {
                MessageBox.Show("Please enter a product name");
                txtproductname.Focus();
                return false;
            }
            if (cmbcategory.SelectedIndex == -1)
            {
                MessageBox.Show("Please enter a product Category");
                cmbcategory.Focus();
                return false;
            }

            decimal price;

            if (!decimal.TryParse(txtunitprice.Text, out price) || price < 0)
            {
                MessageBox.Show("Please enter a product price");
                txtunitprice.Focus();
                return false;
            }

            int quantity;

            if (!int.TryParse(txtquantity.Text, out quantity) || quantity < 0)
            {
                MessageBox.Show("Please enter quality");
                txtquantity.Focus();
                return false;
            }

            int reorder;

            if (!int.TryParse(txtreorderlevel.Text, out reorder) || reorder < 0)
            {
                MessageBox.Show("Please enter a Reorder Level");
                txtreorderlevel.Focus();
                return false;
            }
            return true;
        }
        private void Btnadd_Click (object sender, EventArgs e)
        {
            if (!ValidateProduct())  
                return; 
            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string query = @"INSERT INTO Products (ProductName, Category, Description, UnitPrice, Quantity, ReorderLevel, CreatedAt) VALUES (?,?,?,?,?,?, Now())";

                    using (OleDbCommand cmd = new OleDbCommand(query, con))
                    {
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = txtproductname.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = cmbcategory.Text;
                        cmd.Parameters.Add("?", OleDbType.LongVarWChar).Value = txtdescription.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.Currency).Value = decimal.Parse(txtunitprice.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = int.Parse(txtquantity.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = int.Parse(txtreorderlevel.Text);

                        cmd.ExecuteNonQuery();

                    }
                }
                MessageBox.Show("Products added successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearField();
                LoadProducts();
                LoadDashboardStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding products" + ex.Message);
            }
        }

        private void Btnupdate_Click (object sender, EventArgs e)
        {
            if (selectedProductID == 0)
            {
                MessageBox.Show("Please Select a product row to update");
                return;
            }
            if (!ValidateProduct())
                return;

            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string query = @"UPDATE Products SET  ProductName=?, Category=?, Description=?, UnitPrice=?, Quantity=?, ReorderLevel=? WHERE ProductID=?";

                    using (OleDbCommand cmd = new OleDbCommand(query, con))
                    {
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = txtproductname.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = cmbcategory.Text;
                        cmd.Parameters.Add("?", OleDbType.LongVarWChar).Value = txtdescription.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.Currency).Value = decimal.Parse(txtunitprice.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = int.Parse(txtquantity.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = int.Parse(txtreorderlevel.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = selectedProductID;

                        cmd.ExecuteNonQuery();

                    }
                }
                MessageBox.Show("Products updated successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearField();
                LoadDashboardStats();
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating products" + ex.Message);
            }
        }

        private void Btndelete_Click(object sender, EventArgs e)
        {
            if (selectedProductID == 0)
            {
                MessageBox.Show("Please Select a product row to delete");
                return;
            }
            DialogResult result = MessageBox.Show("Are you sure you want to delete this product?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (OleDbConnection con = Database.GetConnection())
                    {
                        con.Open();

                        string query = @"DELETE FROM Products WHERE ProductID = ?";

                        using (OleDbCommand cmd = new OleDbCommand(query, con))
                        {

                            cmd.Parameters.Add("?", OleDbType.Integer).Value = selectedProductID;

                            cmd.ExecuteNonQuery();

                        }
                    }

                    MessageBox.Show("Products deleted successfully");

                    ClearField();
                    LoadProducts();
                    LoadDashboardStats();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting products" + ex.Message);
                }
            }
        }

        private void Dgvproducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvproducts.Rows[e.RowIndex];

                selectedProductID = Convert.ToInt32(row.Cells["ProductID"].Value);

                txtproductname.Text = row.Cells["ProductName"].Value?.ToString() ?? "";
                cmbcategory.Text = row.Cells["Category"].Value?.ToString() ?? "";
                txtdescription.Text = row.Cells["Description"].Value?.ToString() ?? "";
                txtunitprice.Text = row.Cells["UnitPrice"].Value?.ToString() ?? "";
                txtquantity.Text = row.Cells["Quantity"].Value?.ToString() ?? "";
                txtreorderlevel.Text = row.Cells["ReorderLevel"].Value?.ToString() ?? "";
            }
        }
        private void Btnclear_Click (object sender, EventArgs e)
        {
            ClearField();
        }
        private void ClearField()
        {
            txtproductname.Clear();
            cmbcategory.SelectedIndex = -1;
            txtdescription.Clear();
            txtunitprice.Clear();
            txtquantity.Clear();
            txtreorderlevel.Clear();

            selectedProductID = 0;

            dgvproducts.ClearSelection();
        }
        

        private void Txtsearch_TextChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to log out?", "Confirm logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Restart();
            }

        }
    }
}
