using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data.SqlClient;
using Google.Authenticator;


public partial class Authentication : System.Web.UI.Page
{
    protected const string CONNECTION_STRING = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Homework\PROJECT\PRESENTATION_ARTEFACT\LoginFramework\LoginFramework\App_Code\Users.mdf;Integrated Security=True;Connect Timeout=30";
    public static string KEY = null;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["USERNAME"] != null)
        {
            lblWelcome.Text = "Please enter the authentication code for " + Session["USERNAME"].ToString();

            SqlConnection Connection = new SqlConnection(CONNECTION_STRING);
            string SQLGetKey = "SELECT GoogleKey FROM Account WHERE Username = @USER";
            SqlCommand Command = new SqlCommand(SQLGetKey, Connection);
            Command.Parameters.AddWithValue("@USER", Session["USERNAME"]);
            try
            {
                Connection.Open();
                using (SqlDataReader Reader = Command.ExecuteReader())
                {
                    while (Reader.Read())
                    {
                        KEY = (Reader["GoogleKey"].ToString());
                    }
                }
            }
            finally
            {
                Connection.Close();
            }
        }
    }

    protected void btnLogin_Click(object sender, EventArgs e)
    {         
        TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
        bool isCorrectPIN = tfa.ValidateTwoFactorPIN(KEY, txtCode.Text);        

        if (isCorrectPIN)
        {
            Response.Redirect("Home.aspx");
        }
        else
        {
            lblWrongCode.Visible = true;
            lblWrongCode.Text = "Invalid code.";
        }
    }
}