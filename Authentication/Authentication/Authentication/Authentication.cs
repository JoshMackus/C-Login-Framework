using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Security.Cryptography;
using Google.Authenticator; 

namespace Example
{
    public class UserTool
    {
        static public string RandomSecretKey(int length)
        {
            var random = new Random();
            string s = string.Empty;
            for (int i = 0; i < length; i++)
                s = String.Concat(s, random.Next(10).ToString());
            return s;
        }

        public static string RegisterUser(TextBox Username, TextBox Password, string databaseConnectionString)
        {
            string dbConnection = databaseConnectionString;
            string username = Username.Text;
            string password = Password.Text;
            string encryptedPass = Encrypt.CreateHash(password);
            bool validatePass = Encrypt.ValidatePassword(password, encryptedPass);
            string response = "";
            if (validatePass)
            {
                // Acquire Secret Key for Google Auth, Salt for the password, and Hash for the password
                string key = RandomSecretKey(5);      
                string salt = encryptedPass.Substring(6, 32);
                string hash = encryptedPass.Split(':').Last();

                SqlConnection Connection = new SqlConnection(dbConnection);
                Connection.Open();
                string SQLInsertQuery = "BEGIN IF NOT EXISTS (SELECT * FROM Account WHERE Username = @USER) BEGIN INSERT INTO [Account] (Username, Password, PasswordLastSet, Salt, Hash, GoogleKey) VALUES (@USER, @PASS, @PASS_SET, @SALT, @HASH, @GOOGKEY) END END";
                SqlCommand Command = new SqlCommand(SQLInsertQuery, Connection);
                Command.Parameters.AddWithValue("USER", username);
                Command.Parameters.AddWithValue("PASS", hash);
                Command.Parameters.AddWithValue("PASS_SET", DateTime.Now.ToShortDateString());
                Command.Parameters.AddWithValue("SALT", salt);
                Command.Parameters.AddWithValue("HASH", hash);
                Command.Parameters.AddWithValue("GOOGKEY", key);
                Command.ExecuteNonQuery();
                Connection.Close();

                response = "User sucessfully registered!";

                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                var setupInfo = tfa.GenerateSetupCode("Group 12 Application", username, key, 300, 300);
                string qrCodeImageUrl = setupInfo.QrCodeSetupImageUrl;
                string manualEntrySetupCode = setupInfo.ManualEntryKey;                
                Control parent = Password.Parent;
                var manualCode = new HtmlGenericControl("span");
                manualCode.InnerHtml = "<br/>Manual Code: " + manualEntrySetupCode;
                parent.Controls.AddAt(parent.Controls.IndexOf(Password) + 1, manualCode);

                Image QR = new Image();
                QR.ImageUrl = qrCodeImageUrl;
                QR.Visible = true;
                parent.Controls.AddAt(parent.Controls.IndexOf(Password) + 2, QR);
                return response;
            }
            else
            {
                response = "Sorry. There was an error registering you into the database.\nPlease try registering again.";
                return response;
            }
        }
    }

    /// <summary>
    /// Salted password hashing with PBKDF2-SHA1.
    /// Author: havoc AT defuse.ca
    /// www: http://crackstation.net/hashing-security.htm
    /// Compatibility: .NET 3.0 and later.
    /// </summary>
    public class Encrypt
    {
        // The following constants may be changed without breaking existing hashes.
        public const int SALT_BYTE_SIZE = 24;
        public const int HASH_BYTE_SIZE = 24;
        public const int PBKDF2_ITERATIONS = 10000;

        public const int ITERATION_INDEX = 0;
        public const int SALT_INDEX = 1;
        public const int PBKDF2_INDEX = 2;

        /// <summary>
        /// Creates a salted PBKDF2 hash of the password.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>The hash of the password.</returns>
        public static string CreateHash(string password)
        {
            // Generate a random salt
            RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider();
            byte[] salt = new byte[SALT_BYTE_SIZE];
            csprng.GetBytes(salt);

            // Hash the password and encode the parameters
            byte[] hash = PBKDF2(password, salt, PBKDF2_ITERATIONS, HASH_BYTE_SIZE);
            return PBKDF2_ITERATIONS + ":" +
                Convert.ToBase64String(salt) + ":" +
                Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Validates a password given a hash of the correct one.
        /// </summary>
        /// <param name="password">The password to check.</param>
        /// <param name="correctHash">A hash of the correct password.</param>
        /// <returns>True if the password is correct. False otherwise.</returns>
        public static bool ValidatePassword(string password, string correctHash)
        {
            // Extract the parameters from the hash
            char[] delimiter = { ':' };
            string[] split = correctHash.Split(delimiter);
            int iterations = Int32.Parse(split[ITERATION_INDEX]);
            byte[] salt = Convert.FromBase64String(split[SALT_INDEX]);
            byte[] hash = Convert.FromBase64String(split[PBKDF2_INDEX]);

            byte[] testHash = PBKDF2(password, salt, iterations, hash.Length);
            return SlowEquals(hash, testHash);
        }

        /// <summary>
        /// Compares two byte arrays in length-constant time. This comparison
        /// method is used so that password hashes cannot be extracted from
        /// on-line systems using a timing attack and then attacked off-line.
        /// </summary>
        /// <param name="a">The first byte array.</param>
        /// <param name="b">The second byte array.</param>
        /// <returns>True if both byte arrays are equal. False otherwise.</returns>
        private static bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
        }

        /// <summary>
        /// Computes the PBKDF2-SHA1 hash of a password.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="iterations">The PBKDF2 iteration count.</param>
        /// <param name="outputBytes">The length of the hash to generate, in bytes.</param>
        /// <returns>A hash of the password.</returns>
        private static byte[] PBKDF2(string password, byte[] salt, int iterations, int outputBytes)
        {
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt);
            pbkdf2.IterationCount = iterations;
            return pbkdf2.GetBytes(outputBytes);
        }
    }


    // *** This class is for everything related to USERNAME validation: ***
    // ~ Checking a username exists
    // ~ Checking the username format is valid: - Using RegEx for email                                  
    //                                          - Checking any rules (e.g requires 6+ characters)
    // ~ Checking if username already exists
    public class UsernameValidation
    {
        
        public static bool EmailAsUsernameCheck(TextBox Username)
        {
            bool isValid = false;

            string username = Username.Text;
            Control parent = Username.Parent;
            var userEmailCheck = new HtmlGenericControl("span");
            userEmailCheck.Attributes["class"] = "errors";
           
            /***** FORMAT CHECK *****/
            // Check the email format is something@something.something
            if (!Regex.IsMatch(Username.Text, @"^.+@[^\.].*\.[a-z]{2,}$", RegexOptions.IgnorePatternWhitespace))
            {
                userEmailCheck.InnerHtml = "<br />Username must be an email (example@example.com).";
                parent.Controls.AddAt(parent.Controls.IndexOf(Username) + 1, userEmailCheck);
            }

            // The username is checked to be valid.
            else
                isValid = true;

            return isValid;
        }

        public static bool UsernameIsEmpty(TextBox Username)
        {
            bool isEmpty = false;

            string username = Username.Text;
            Control parent = Username.Parent;
            var userEmailCheck = new HtmlGenericControl("span");
            userEmailCheck.Attributes["class"] = "errors";

            /***** EMPTY CHECK *****/
            if (String.IsNullOrEmpty(username) || String.IsNullOrWhiteSpace(username))
            {
                if (parent != null && parent.Controls.IndexOf(Username) >= 0)
                {
                    isEmpty = true;
                    userEmailCheck.InnerHtml = "<br />Please enter a Username.";
                    parent.Controls.AddAt(parent.Controls.IndexOf(Username) + 1, userEmailCheck);
                    return true;

                }
            }
            return isEmpty;
        }

        public static bool UserExists(TextBox Username, TextBox Password, string databaseConnectionString)
        {
            string dbConnection = databaseConnectionString;
            bool usernameExists = false;
            string username = Username.Text;

            Control parent = Password.Parent;
            var invalidLogin = new HtmlGenericControl("span");
            invalidLogin.Attributes["class"] = "errors";

            // Establish db connection.
            SqlConnection myConnection = new SqlConnection(dbConnection);
            SqlDataReader myReader = null;

            try
            {
                myConnection.Open();

                // Build sql query statement.
                SqlCommand myCommand = new SqlCommand("SELECT Username FROM [Account] WHERE Username=@USER", myConnection);
                myCommand.Parameters.AddWithValue("USER", username);

                // Execute query and read extracted field.
                myReader = myCommand.ExecuteReader();
                while (myReader.Read())
                {
                    string tempUser = myReader["Username"].ToString();
                    // Compare attempted username with existing username
                    if (String.Equals(tempUser, username))
                    {
                        usernameExists = true;
                    }
                    else
                    {
                        usernameExists = false;
                    }
                }
            }
            finally
            {
                if (myReader != null)
                    myReader.Close();
            }
            if (myConnection != null)
                myConnection.Close();

            if(!usernameExists)
                invalidLogin.InnerHtml = "<br />Invalid username or password.";
                parent.Controls.AddAt(parent.Controls.IndexOf(Password) + 1, invalidLogin);


            return usernameExists;
        }
        
        public static bool UsernameConflictCheck(TextBox Username, string databaseConnectionString)
        {
            string dbConnection = databaseConnectionString;
            bool usernameExists = false;

            string username = Username.Text;
            Control parent = Username.Parent;
            var userConflictCheck = new HtmlGenericControl("span");
            userConflictCheck.Attributes["class"] = "errors";

            // Establish db connection.
            SqlConnection myConnection = new SqlConnection(dbConnection);  //  Arguments need populating.....
            SqlDataReader myReader = null;

            try
            {
                myConnection.Open();

                // Build sql query statement.
                SqlCommand myCommand = new SqlCommand("SELECT Username FROM Account WHERE Username = @USER", myConnection);
                myCommand.Parameters.AddWithValue("USER", username);

                // Execute query and read extracted field.
                myReader = myCommand.ExecuteReader();
                while (myReader.Read())
                {
                    // Compare attempted username with existent usernames.
                    // Set flag and stop loop once the attemped username is checked to be existent in db.
                    if (myReader[0].ToString() == Username.Text)
                    {
                        usernameExists = true;
                        if (parent != null && parent.Controls.IndexOf(Username) >= 0)
                        {
                            userConflictCheck.InnerHtml = "<br />This username is taken!";
                            parent.Controls.AddAt(parent.Controls.IndexOf(Username) + 1, userConflictCheck);
                        }
                        break;
                    }
                }
            }

            finally 
            {
                if (myReader != null)
                    myReader.Close();
            }

            if (myConnection != null)
                myConnection.Close();

            return usernameExists;
        }

        
    }




    public class PasswordValidation
    {        
        /*
        public static bool FieldIsEmpty(bool t, string Password, Page p)
        {
            if (String.IsNullOrEmpty(Password) || String.IsNullOrWhiteSpace(Password))
            {
                t = true;
                p.ClientScript.RegisterStartupScript(p.GetType(), "alert", "alert('Please enter a password.')", true);
                return t;
            }
            else
                return t;
        }
        */
        //example methods

        public static bool PasswordIsEmpty(TextBox Password)
        {
            bool isEmpty = false;
            
            string password = Password.Text;
            Control parent = Password.Parent;
            var pswEmptyCheck = new HtmlGenericControl("span");
            pswEmptyCheck.Attributes["class"] = "errors";

            /***** EMPTY CHECK *****/
            if (String.IsNullOrEmpty(password) || String.IsNullOrWhiteSpace(password))
            {
                isEmpty = true;
                
                if (parent != null && parent.Controls.IndexOf(Password) >= 0)
                {
                    pswEmptyCheck.InnerHtml = "<br />Please enter a Password.";
                    parent.Controls.AddAt(parent.Controls.IndexOf(Password) + 1, pswEmptyCheck);
                }
            }
            return isEmpty;
        }

        // Check the password is 6 characters AT LEAST
        // Check there is at least 1 number
        // password9
        /* public static bool BasicPasswordCheck(TextBox Password)
         {
             bool isValid = false;
             
             Control parent = Password.Parent;
            var pwdStrengthCheck = new HtmlGenericControl("span");
            pwdStrengthCheck.Attributes["class"] = "errors";

            // Check the password is 6 characters AT LEAST
            if (!Regex.IsMatch(Password.Text, @"^.{6,}$", RegexOptions.IgnorePatternWhitespace))
            {
                pwdStrengthCheck.InnerHtml = "<br />Password should consist of at least 6 characters.";
            }
            
            // Check there is at least 1 number

            //else if (!Regex.IsMatch(Password.Text, @"^(?=.*\d)$", RegexOptions.IgnorePatternWhitespace))
            else if (!Regex.IsMatch(Password.Text, @"^.*\d.*$", RegexOptions.IgnorePatternWhitespace))
            {
                pwdStrengthCheck.InnerHtml = "<br />Password should include at least 1 number.";
            }
            
            // If the passed-in txtbox exists and has a 0+ index 
            if (parent != null && parent.Controls.IndexOf(Password) >= 0 && pwdStrengthCheck.InnerHtml != "")
            {
                parent.Controls.AddAt(parent.Controls.IndexOf(Password) + 1, pwdStrengthCheck);
            }

             // The password is checked to be valid.
            else
                isValid = true;

            return isValid;
         } 

        */


        // Check the password is 8 characters AT LEAST
        // Check there is at least 1 number
        // Check there is at least 1 capital letter
        /*
        public static bool ModeratePasswordCheck(TextBox Password)
         {
             bool isValid = false;

             Control parent = Password.Parent;
             var pwdStrengthCheck = new HtmlGenericControl("span");
             pwdStrengthCheck.Attributes["class"] = "errors";

             // Check the password is 8 characters AT LEAST
             if (!Regex.IsMatch(Password.Text, @"^.{6,}$", RegexOptions.IgnorePatternWhitespace))
             {
                 pwdStrengthCheck.InnerHtml = "<br />Password should consist of at least 8 characters.";
             }

             // Check there is at least 1 number
             else if (!Regex.IsMatch(Password.Text, @"^.*\d.*$", RegexOptions.IgnorePatternWhitespace))
             {
                 pwdStrengthCheck.InnerHtml = "<br />Password should include at least 1 number.";
             }

             // Check there is at least 1 capital letter
             else if (!Regex.IsMatch(Password.Text, @"^.*[A-Z].*$", RegexOptions.IgnorePatternWhitespace))
             {
                 pwdStrengthCheck.InnerHtml = "<br />Password should include at least 1 capital letter.";
             }

             // If the passed-in txtbox exists and has a 0+ index 
             if (parent != null && parent.Controls.IndexOf(Password) >= 0 && pwdStrengthCheck.InnerHtml!="")
             {
                 parent.Controls.AddAt(parent.Controls.IndexOf(Password) + 1, pwdStrengthCheck);
             }

             // The password is checked to be valid.
             else
                 isValid = true;

             return isValid;
         } 

        */

        
        // Check the password is 8 characters AT LEAST
        // Check there is at least 1 number
        // Check there is at least 1 capital letter
        // Check there is at least 1 special characte
        public static bool BestPasswordCheck(TextBox Password)
        {
            bool isValid = false;

            Control parent = Password.Parent;
            var pwdStrengthCheck = new HtmlGenericControl("span");
            pwdStrengthCheck.Attributes["class"] = "errors";

            // Check the password is 8 characters AT LEAST
            if (!Regex.IsMatch(Password.Text, @"^.{8,}$", RegexOptions.IgnorePatternWhitespace))
            {
                pwdStrengthCheck.InnerHtml = "<br />Password should consist of at least 8 characters.";
            }

            // Check there is at least 1 number
            else if (!Regex.IsMatch(Password.Text, @"^.*\d.*$", RegexOptions.IgnorePatternWhitespace))
            {
                pwdStrengthCheck.InnerHtml = "<br />Password should include at least 1 number.";
            }

            // Check there is at least 1 capital letter
            else if (!Regex.IsMatch(Password.Text, @"^.*[A-Z].*$", RegexOptions.IgnorePatternWhitespace))
            {
                pwdStrengthCheck.InnerHtml = "<br />Password should include at least 1 capital letter.";
            }

            // Check there is at least 1 special characte
            else if (!Regex.IsMatch(Password.Text, @"^.*[^A-Za-z0-9].*$", RegexOptions.IgnorePatternWhitespace))
            {
                pwdStrengthCheck.InnerHtml = "<br />Password should include at least 1 special letter.";
            }
           
            // If the passed-in txtbox exists and has a 0+ index 
            if (parent != null && parent.Controls.IndexOf(Password) >= 0 && pwdStrengthCheck.InnerHtml != "")
            {
                parent.Controls.AddAt(parent.Controls.IndexOf(Password) + 1, pwdStrengthCheck);
            }
            
            // The password is checked to be valid.
            else
                isValid = true;

            return isValid;
        }

        public static bool ConfirmEmptyCheck(TextBox Confirm)
        {
            bool isEmpty=false;
        
            string password = Confirm.Text;
            Control parent = Confirm.Parent;
            var cfmEmptyCheck = new HtmlGenericControl("span");
            cfmEmptyCheck.Attributes["class"] = "errors";

            /***** EMPTY CHECK *****/
            if (String.IsNullOrEmpty(password) || String.IsNullOrWhiteSpace(password))
            {
                isEmpty=true;

                if (parent != null && parent.Controls.IndexOf(Confirm) >= 0)
                {
                    cfmEmptyCheck.InnerHtml = "<br />Please confirm the password.";
                    parent.Controls.AddAt(parent.Controls.IndexOf(Confirm) + 1, cfmEmptyCheck);
                }
            }

            return isEmpty;
        }

        public static bool PasswordMatch(TextBox Username, TextBox Password, string databaseConnectionString)
        { 
            string username = Username.Text;
            string password = Password.Text;
            string dbConnection = databaseConnectionString;
            string encryptedPass = Encrypt.CreateHash(password);
            bool passwordMatches = false;

            Control parent = Password.Parent;
            var invalidLogin = new HtmlGenericControl("span");
            invalidLogin.Attributes["class"] = "errors";
            
                //string salt = encryptedPass.Substring(6, 32);
                string hash = encryptedPass.Split(':').Last();

                SqlConnection myConnection = new SqlConnection(dbConnection);  
                SqlDataReader myReader = null;

                try
                {
                    myConnection.Open();

                    // Build sql query statement.
                    SqlCommand myCommand = new SqlCommand("SELECT Username, Hash, Salt FROM Account WHERE Username = @USER", myConnection);                    
                    myCommand.Parameters.AddWithValue("USER", username);

                    // Execute query and read extracted field.
                    myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    { 
                        if (String.Equals(myReader["Username"], username))
                        {
                            string storedHash, storedSalt;
                            storedHash = myReader["Hash"].ToString();
                            storedSalt = myReader["Salt"].ToString();
                            bool pMatch = Encrypt.ValidatePassword(password, "10000:" + storedSalt + ":" + storedHash);
                            
                            if(pMatch)
                            {
                                passwordMatches = true;
                            }
                            else
                            {
                                passwordMatches = false;                               
                            }
                        } 
                    }
                }
                finally
                {
                    if (myReader != null)
                        myReader.Close();
                }
                if (myConnection != null)
                    myConnection.Close();

                if (!passwordMatches)
                {
                    if (parent != null && parent.Controls.IndexOf(Password) >= 0)
                    {
                        invalidLogin.InnerHtml = "<br />Invalid username or password.";
                        parent.Controls.AddAt(parent.Controls.IndexOf(Password) + 1, invalidLogin);
                    }
                }

                return passwordMatches;
        }
        

        public static bool passwordConfirm(TextBox Password, TextBox Confirm)
        {
            bool passwordsMatch = true;

            Control parent = Confirm.Parent;
            var pwdConfirm = new HtmlGenericControl("span");
            pwdConfirm.Attributes["class"] = "errors";
            
            // Check if the comfirm password is identical to password
            if (Password.Text != Confirm.Text)
            {
                passwordsMatch = false;                
                pwdConfirm.InnerHtml = "<br />Confirm password is not identical to password above.";
                
                if (parent != null && parent.Controls.IndexOf(Confirm) >= 0 && pwdConfirm.InnerHtml != "")
                {
                    parent.Controls.AddAt(parent.Controls.IndexOf(Confirm) + 1, pwdConfirm);
                    return passwordsMatch;
                }
            }

            // If the passed-in txtbox exists and has a 0+ index 
            
            return passwordsMatch; 
        }


    }
}