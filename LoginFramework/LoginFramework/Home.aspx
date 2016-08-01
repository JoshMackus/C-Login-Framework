    <%@ Page Language="C#" AutoEventWireup="true" CodeFile="Home.aspx.cs" Inherits="Home" %>

<!DOCTYPE html>
<link href="StyleSheet.css" rel="stylesheet" />
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Super Software Solutions</title>
</head>
<body>
    <form id="form" runat="server" cssclass="form">
     <div class="title">
         <asp:Label ID="lblTitle" Text="SIT302 - Home Page" runat="server" />
     </div>
        <div class="subheading">
         <asp:Label ID="lblWelcome" Text="" CssClass="title" runat="server" />
        </div>

     
    </form>
</body>
</html>
