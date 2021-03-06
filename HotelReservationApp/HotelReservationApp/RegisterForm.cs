﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace HotelReservationApp
{
    public partial class RegisterForm : Form
    {
        private const int SALT_SIZE = 32;

        public RegisterForm()
        {
            InitializeComponent();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            // Check to see if user entered data
            if (txtBxName.Text.Length < 4)
            {
                lblMessage.Text = "Please use a longer username.";
                return;
            }
            if(txtBxPass.Text.Length < 8)
            {
                lblMessage.Text = "Please use a longer password.";
                return;
            }

            // Establish connection to the database
            string dbConnection = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Workspace\SSP\HotelReservationApp\HotelReservationApp\DatabaseSSP.mdf;Integrated Security=True";
            SqlConnection conn = new SqlConnection(dbConnection);
            conn.Open();

            // Check to see if name is already in DB
            string sqlQuery = "SELECT Id FROM dbo.userTbl WHERE UserName = '" + txtBxName.Text + "';";
            SqlCommand sqlCmd = new SqlCommand(sqlQuery, conn);
            SqlDataReader dbReader = sqlCmd.ExecuteReader();

            if (dbReader.Read())
                lblMessage.Text = "That name is already in use.";
            else
            {
                // Insert username into userTbl
                sqlQuery = "INSERT INTO dbo.userTbl(UserName) VALUES(@userName);";
                sqlCmd = new SqlCommand(sqlQuery, conn);
                sqlCmd.Parameters.Add("@userName", SqlDbType.VarChar, 50);
                sqlCmd.Parameters["@userName"].Value = txtBxName.Text;
                sqlCmd.ExecuteNonQuery();

                try
                {                
                    // Hash password
                    byte[] salt = SignInForm.CreateSalt(SALT_SIZE);
                    byte[] hash = SignInForm.GenerateSaltedHash(Encoding.UTF8.GetBytes(txtBxPass.Text), salt);

                    //  Insert hashed password (and salt) into passTbl
                    sqlQuery = "INSERT INTO dbo.passTbl(Hash, Salt) VALUES('" + Convert.ToBase64String(hash) + "', '" + Convert.ToBase64String(salt) + "');";
                    sqlCmd = new SqlCommand(sqlQuery, conn);
                    sqlCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Make sure UserName is removed from db if there
                    // is an exception when attempting to set password
                    sqlQuery = "DELETE FROM dbo.userTbl WHERE (UserName='" + txtBxName.Text + "');";
                    sqlCmd = new SqlCommand(sqlQuery, conn);
                    sqlCmd.ExecuteNonQuery();

                    // Close connections
                    dbReader.Close();
                    conn.Close();

                    throw ex;
                }
            }

            // Close connection
            dbReader.Close();
            conn.Close();
        }
    }
}
