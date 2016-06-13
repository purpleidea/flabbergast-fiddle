<%@ Page Language="C#" Inherits="Flabbergast.Fiddle.Default" %>
<!DOCTYPE html>
<html>
<head runat="server">
	<title>Flabbergast Fiddle</title>
</head>
<body>
	<form id="fiddle" runat="server">
		<asp:TextBox id="script" runat="server" TextMode="multiline" />
		<asp:Button id="run" runat="server" Text="Run!" OnClick="runScript" />
		<asp:Label id="output" runat="server" />
	</form>
</body>
</html>

