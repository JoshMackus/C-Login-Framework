using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Example;
using Google.Authenticator;

public partial class Login : System.Web.UI.Page
{
    protected const string CONNECTION_STRING = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Homework\PROJECT\PRESENTATION_ARTEFACT\LoginFramework\LoginFramework\App_Code\Users.mdf;Integrated Security=True;Connect Timeout=30";
    protected void Page_Load(object sender, EventArgs e)
    {
            
    }

    protected void btnLogin_Click(object sender, EventArgs e)
    {
        //bool t = false;
        //Page p = this.Page;

        /* Accesses the custom authentication class. It requires 3 parameters:
         * 1. A boolean expression to return true or false depending on the state of the field (empty or not empty);
         * 2. The controller for the textbox or field in which the username is entered, in our case it is called 'txtUsername'
         *    add the .Text modifier in order for it to be retrieved as a STRING data type. 
         * 3. The page that the form is on.*/

        if (!UsernameValidation.UsernameIsEmpty(txtUsername) && !PasswordValidation.PasswordIsEmpty(txtPassword))
        {
            if (UsernameValidation.UserExists(txtUsername, txtPassword, CONNECTION_STRING))
            {
                if (PasswordValidation.PasswordMatch(txtUsername, txtPassword, CONNECTION_STRING))
                {
                    string user = txtUsername.Text;
                    Session["USERNAME"] = user;
                    Response.Redirect("Authentication.aspx");
                }
            }
        }
    }
}