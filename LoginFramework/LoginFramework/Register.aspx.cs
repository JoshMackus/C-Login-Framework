using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Cryptography;
using System.Web.Security;
using Example;
using DBEncryption;

public partial class Register : System.Web.UI.Page
{
    protected const string CONNECTION_STRING = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Homework\PROJECT\PRESENTATION_ARTEFACT\LoginFramework\LoginFramework\App_Code\Users.mdf;Integrated Security=True;Connect Timeout=30";
    protected void Page_Load(object sender, EventArgs e)
    {
        if(btnRegister.Text.Equals("Continue to Login -->"))
        {
            // prevent the Username input from clearing after button click
            string Username = txtUsername.Text;
            txtUsername.Attributes.Add("value", Username);
        }
       
        // prevent the confirm password input from clearing after button click
        string ConfirmPassword = txtConfirm.Text;
        txtConfirm.Attributes.Add("value", ConfirmPassword);
    }
 
    protected void btnRegister_Click(object sender, EventArgs e)
    {   
        // Flags to determine if further validations need executing.
        bool isUsernameEmpty, isPasswordEmpty, isConfirmEmpty, isUsernameValid, UsernameExists, isPasswordValid, isConfirmValid;

        /***** Username and Password Validation *****/

        isUsernameEmpty = UsernameValidation.UsernameIsEmpty(txtUsername);        
        isPasswordEmpty = PasswordValidation.PasswordIsEmpty(txtPassword);
        isConfirmEmpty = PasswordValidation.ConfirmEmptyCheck(txtConfirm);

        // Start to check password and user format if it's not empty.
        if (!isPasswordEmpty && !isUsernameEmpty && !isConfirmEmpty)
        {
            isUsernameValid = UsernameValidation.EmailAsUsernameCheck(txtUsername);
            UsernameExists = UsernameValidation.UsernameConflictCheck(txtUsername, CONNECTION_STRING);    
            isPasswordValid = PasswordValidation.BestPasswordCheck(txtPassword);            
            isConfirmValid = PasswordValidation.passwordConfirm(txtPassword, txtConfirm);

            // Start to encrypt password if all the fields are valid and username does not already exist in the DB.
            if (!UsernameExists && isPasswordValid && isUsernameValid && isConfirmValid)
            {
                string registerMessage = UserTool.RegisterUser(txtUsername, txtPassword, CONNECTION_STRING);
                lblTitle.Text = registerMessage;
                AllowLogin();
                
                
            }
            
        }


        

        
    }
    
    private void AllowLogin()
    {
        txtConfirm.Visible = false;
        txtPassword.Visible = false;
        txtUsername.Visible = false;
        lblConfirm.Visible = false;
        lblUsername.Visible = false;
        lblPassword.Visible = false;
        btnRegister.Text = "Continue to login -->";
        btnRegister.PostBackUrl = "Login.aspx";
    }

    protected void txtPassword_TextChanged(object sender, EventArgs e)
    {
        
    }
    protected void txtConfirm_TextChanged(object sender, EventArgs e)
    {
        
    }
}