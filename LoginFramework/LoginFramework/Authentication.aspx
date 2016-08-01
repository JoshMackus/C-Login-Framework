    <%@ Page Language="C#" AutoEventWireup="true" CodeFile="Authentication.aspx.cs" Inherits="Authentication" %>

<!DOCTYPE html>
<link href="StyleSheet.css" rel="stylesheet" />
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Super Software Solutions</title>
</head>
<body>
    <form id="form" runat="server" cssclass="form">
     <div class="title">
         <asp:Label ID="lblTitle" Text="SIT302 - Example Login Page" runat="server" />
     </div>
        <div class="subheading">
         <asp:Label ID="lblWelcome" Text="asd" CssClass="title" runat="server" />
        </div>

     <div id="inputLogin" class="inputLogin">
        <br />
        <asp:Label ID="lblCode" Text="Authenticator Code " CssClass="labels" runat="server" />
        <asp:TextBox ID="txtCode"  placeholder="(e.g. 123 456)" CssClass="textboxes" runat="server" />
        <br />
         <br />
         <asp:Label ID="lblWrongCode" CssClass="errors" Visible="false" Text="" runat="server" />
         <br />
        <asp:Button id="btnLogin" CssClass="buttons" OnClick="btnLogin_Click" Text="Submit" runat="server" />
         <br />

         <!--
        <div class="smalltext">
            <p>Don't have an account? <a href="Register.aspx" class="registerlink">Click here to register.</a></p>
        </div>
         -->
     </div>
    </form>
</body>
</html>
