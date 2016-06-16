<%@ Page Language="C#" Inherits="Flabbergast.Fiddle.Default" validateRequest="false" %>
<!DOCTYPE html>
<html>
<head runat="server">
	<title>Flabbergast Fiddle</title>
	<link rel="stylesheet" type="text/css" href="o_0-fiddle.css"/>
	<meta charset="UTF-8"/>
</head>
<body>
	<form id="fiddle" runat="server">
		<table>
			<tr>
				<td colspan="2" class="title">
					Flabbergast Fiddle
					<select onchange="if (this.selectedIndex > 0) { document.getElementById('script').value = this.value; this.selectedIndex = 0; }">
						<option value="">Pick a demo</option>
						<option value="value : &quot;Hello World&quot;">Hello</option>
					</select>
				</td>
			</tr>
			<tr><td height="90%" width="50%"><asp:TextBox id="script" runat="server" TextMode="multiline" /></td><td id="result" rowspan="2"><asp:Label id="output" runat="server" /></td></tr>
			<tr><td><asp:Button id="run" runat="server" Text="Do the Thing, Zhu Li!" OnClick="runScript" /></td></tr>
		</table>
	</form>
</body>
</html>

