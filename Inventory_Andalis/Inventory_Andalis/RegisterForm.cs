using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Inventory_Andalis
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
            txtpassword1.UseSystemPasswordChar = true;
            txtpassword2.UseSystemPasswordChar = true;
            btnregister.Click += Btnregister_Click;
            linkregister.LinkClicked += Linkregister_LinkClicked;
        }

        private void Linkregister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
          
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Btnregister_Click(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            String fullname = txtfullname.Text.Trim();
            String username = txtusername.Text.Trim();
            String password = txtpassword1.Text.Trim();
            String confirmpassword = txtpassword2.Text.Trim();

            if (fullname == "" || username == "" || password == "" ||  confirmpassword == "")
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (password != confirmpassword)
            {
                MessageBox.Show("Password Do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (password.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters long.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                using (OleDbConnection CON = Database.GetConnection())
                {
                    CON.Open();
                    string checkQuery = "SELECT COUNT (*) FROM Users WHERE Username = ?";
                    using (OleDbCommand checkCmd = new OleDbCommand(checkQuery, CON)) 
                    {
                        checkCmd.Parameters.AddWithValue("@Username", username);
                        int userCount = (int)checkCmd.ExecuteScalar();
                        if (userCount > 0)
                        {
                            MessageBox.Show("Username Already exist please choose different username.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    string query = @"INSERT INTO users(Username, Fullname,  PasswordHash, UserRole, IsActive) VALUES (?, ?, ?, ?, ?)";

                    using (OleDbCommand Cmd = new OleDbCommand(query, CON))
                    {
                        string passwordHash = PasswordHelper.HashPassword(password);
                        Cmd.Parameters.AddWithValue("@Username", username);
                        Cmd.Parameters.AddWithValue("@Fullname", fullname);
                        Cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        Cmd.Parameters.AddWithValue("@UserRole", "user");
                        Cmd.Parameters.AddWithValue("@IsActive", true);
                        int rowsAffected = Cmd.ExecuteNonQuery();
                        if(rowsAffected > 0)
                        {
                            MessageBox.Show("User Registered Successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            txtfullname.Clear();
                            txtpassword1.Clear();
                            txtpassword2.Clear();
                            txtusername.Clear();
                            SignInForm login = new SignInForm();
                            login.Show();
                            this.Hide();
                        }
                        else
                        {
                            MessageBox.Show("Failed to register user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured while registering the user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkregister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SignInForm login = new SignInForm();
            login.Show();
            this.Hide();
        }
    }
}
