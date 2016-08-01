    <%@ Page Language="C#" AutoEventWireup="true" CodeFile="Register.aspx.cs" Inherits="Register" %>

<!DOCTYPE html>
<link href="StyleSheet.css" rel="stylesheet" />
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Super Software Solutions</title>
</head>
<body>
    <form id="form" runat="server" cssclass="form">
     <div class="title">
         <asp:Label ID="lblTitle" Text="SIT302 - Example Register Page" runat="server" />
     </div>
        <div class="subheading">
         

        </div>

     <div id="inputRegister" class="inputRegister">
         <br />
        <asp:Label ID="lblUsername" Text="Username (email)" CssClass="labels" runat="server" />
        <asp:TextBox ID="txtUsername"  placeholder="someone@example.com"  CssClass="textboxes" runat="server" />  
        
        <br />
        
        <asp:Label ID="lblPassword" Text="Password:" CssClass="labels" runat="server" />
        <asp:TextBox ID="txtPassword" textmode="Password"  CssClass="textboxes"  OnTextChanged="txtPassword_TextChanged" runat ="server" />
        <%--<asp:RequiredFieldValidator id="validatePassword" CssClass="errors" runat="server" EnableClientScript="false" Text="Oops. You forgot your password." ControlToValidate="txtPassword" Display="Dynamic" ForeColor="Red"/>--%>
        <br />

        <asp:Label ID="lblConfirm" Text="Confirm Password" CssClass="labels" runat="server" />
        <asp:TextBox ID="txtConfirm" textmode="Password"  CssClass="textboxes" OnTextChanged="txtConfirm_TextChanged" runat="server" />
        <%--<asp:RequiredFieldValidator id="validateLastName" runat="server" EnableClientScript="false" Text="< You must enter your last name." ControlToValidate="txtConfirm" Display="Dynamic" ForeColor="Red"/>--%>
         
        <br />
               <asp:Label ID="lblNotify" runat="server" />
        <br />       
        <asp:Button id="btnRegister" CssClass="buttons" OnClick="btnRegister_Click" Text="Register" runat="server"/>
         <br />
     </div>
    </form>
</body>
</html>
