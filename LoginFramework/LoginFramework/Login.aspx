    <%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>

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
         

        </div>

     <div id="inputLogin" class="inputLogin">
         <br />
        <asp:Label ID="lblUsername" Text="Username (email)" CssClass="labels" runat="server" />
        <asp:TextBox ID="txtUsername"  placeholder="Welcome!" CssClass="textboxes" runat="server" />
        <br />
        <!-- 
            <asp:RegularExpressionValidator ID="regUsername" CssClass="errors" ControlToValidate="txtUsername" Text="Invalid e-mail address, please re-enter." ValidationExpression="^.+@[^\.].*\.[a-z]{2,}$" Display="Dynamic" ForeColor="Red"  runat="server"/>
        <asp:RequiredFieldValidator id="reqUsername" CssClass="errors" runat="server" EnableClientScript="true" Text="Oops! You forgot your username." ControlToValidate="txtUsername" Display="Dynamic" ForeColor="Red"/>
        <br />
        -->
        <asp:Label ID="lblPassword" Text="Password:" CssClass="labels" runat="server" />
        <asp:TextBox ID="txtPassword" textmode="Password" AutoPostBack="false" CssClass="textboxes" runat="server" />
        <br />
         <!--
        <asp:RequiredFieldValidator id="reqPassword" CssClass="errors" runat="server" EnableClientScript="true" Text="Oops! You forgot your password." ControlToValidate="txtPassword" Display="Dynamic" ForeColor="Red"/>
         -->

        <br />
        <asp:Label ID="lblNotify" runat="server" />
        <asp:Button id="btnLogin" CssClass="buttons" OnClick="btnLogin_Click" Text="Login" runat="server"/>

        <div class="smalltext">
            <p>Don't have an account? <a href="Register.aspx" class="registerlink">Click here to register.</a></p>
        </div>
     </div>
    </form>
</body>
</html>
