﻿<%@ Page Language="C#" Inherits="Flabbergast.Fiddle.Default" validateRequest="false" %>
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
						<option value="aurora_lib : From lib:apache/aurora # Import the Aurora library.&#13;&#10;hw_file : aurora_lib.aurora_file { # Create an Aurora configuration file&#13;&#10;  jobs : [ ] # Aurora requires we provide a list of job. We have none so far.&#13;&#10;}&#13;&#10;value : hw_file.value # Flabbergast is going to dump this string to the output.">Aurora: Empty</option>
						<option value="aurora_lib : From lib:apache/aurora&#13;&#10;cluster : &quot;cluster1&quot;&#13;&#10;role : &quot;jrhacker&quot;&#13;&#10;resources : {&#13;&#10;  cpu : 0.1&#13;&#10;  ram : 16Mi&#13;&#10;  disk : 16Mi&#13;&#10;}&#13;&#10;hw_file : aurora_lib.aurora_file {&#13;&#10;  jobs : [&#13;&#10;    job {&#13;&#10;      instances : 1&#13;&#10;      job_name : &quot;hello_world&quot;&#13;&#10;      task : aurora_lib.task { processes : [] }&#13;&#10;    }&#13;&#10;  ]&#13;&#10;}&#13;&#10;value : hw_file.value">Aurora: Top-level Resources</option>
						<option value="aurora_lib : From lib:apache/aurora&#13;&#10;hw_file : aurora_lib.aurora_file {&#13;&#10;  resources : {&#13;&#10;    cpu : 0.1&#13;&#10;    ram : 16Mi&#13;&#10;    disk : 16Mi&#13;&#10;  }&#13;&#10;  jobs : [&#13;&#10;    job {&#13;&#10;      cluster : &quot;cluster1&quot;&#13;&#10;      role : &quot;jrhacker&quot;&#13;&#10;      instances : 1&#13;&#10;      job_name : &quot;hello_world&quot;&#13;&#10;      task : aurora_lib.task { processes : [] }&#13;&#10;    }&#13;&#10;  ]&#13;&#10;}&#13;&#10;value : hw_file.value">Aurora: Inner Resources</option>
						<option value="aurora_lib : From lib:apache/aurora&#13;&#10;role : &quot;jrhacker&quot;&#13;&#10;resources : {&#13;&#10;  cpu : 0.2 # This value will be eclipsed by⋯&#13;&#10;  ram : 16Mi&#13;&#10;}&#13;&#10;hw_file : aurora_lib.aurora_file {&#13;&#10;  resources : {&#13;&#10;    disk : 16Mi&#13;&#10;  }&#13;&#10;  jobs : [&#13;&#10;    job {&#13;&#10;      resources : {&#13;&#10;        cpu : 0.1 # ⋯ this one, because this one is “closer” to the point of use.&#13;&#10;      }&#13;&#10;      cluster : &quot;cluster1&quot;&#13;&#10;      instances : 1&#13;&#10;      job_name : &quot;hello_world&quot;&#13;&#10;      task : aurora_lib.task { processes : [] }&#13;&#10;    }&#13;&#10;  ]&#13;&#10;}&#13;&#10;value : hw_file.value">Aurora: Split Resources</option>
						<option value="aurora_lib : From lib:apache/aurora&#13;&#10;cluster : &quot;cluster1&quot;&#13;&#10;role : &quot;jrhacker&quot;&#13;&#10;resources : {&#13;&#10;  cpu : 0.1&#13;&#10;  ram : 16Mi&#13;&#10;  disk : 16Mi&#13;&#10;}&#13;&#10;hw_file : aurora_lib.aurora_file {&#13;&#10;  jobs : [&#13;&#10;    job {&#13;&#10;      instances : 1&#13;&#10;      job_name : &quot;hello_world&quot;&#13;&#10;      task : aurora_lib.task {&#13;&#10;        processes : { # Define the processes ⋯&#13;&#10;          hw : process { # ⋯ using process template&#13;&#10;            process_name : &quot;hw&quot; # The name of this process will be `hw`&#13;&#10;            command_line : [ &quot;echo hello world&quot; ] # The command line we want to run.&#13;&#10;          }&#13;&#10;        }&#13;&#10;      }&#13;&#10;    }&#13;&#10;  ]&#13;&#10;}&#13;&#10;value : hw_file.value">Aurora: Processes</option>
						<option value="aurora_lib : From lib:apache/aurora&#13;&#10;cluster : &quot;cluster1&quot;&#13;&#10;role : &quot;jrhacker&quot;&#13;&#10;resources : {&#13;&#10;  cpu : 0.1&#13;&#10;  ram : 16Mi&#13;&#10;  disk : 16Mi&#13;&#10;}&#13;&#10;hw_file : aurora_lib.aurora_file {&#13;&#10;  jobs : [&#13;&#10;    job {&#13;&#10;      instances : 1&#13;&#10;      job_name : &quot;hello_world&quot;&#13;&#10;      task : aurora_lib.task {&#13;&#10;        processes : {&#13;&#10;          hw : process {&#13;&#10;            process_name : &quot;hw&quot;&#13;&#10;            command_line : [ &quot;echo hello world. I am &quot;, current_instance ]&#13;&#10;          }&#13;&#10;        }&#13;&#10;      }&#13;&#10;    }&#13;&#10;  ]&#13;&#10;}&#13;&#10;value : hw_file.value">Aurora: Replica Identity</option>
						<option value="aurora_lib : From lib:apache/aurora&#13;&#10;cluster : &quot;cluster1&quot;&#13;&#10;role : &quot;jrhacker&quot;&#13;&#10;resources : {&#13;&#10;  cpu : 0.1&#13;&#10;  ram : 16Mi&#13;&#10;  disk : 16Mi&#13;&#10;}&#13;&#10;hw_file : aurora_lib.aurora_file {&#13;&#10;  jobs : [&#13;&#10;    job {&#13;&#10;      instances : 1&#13;&#10;      job_name : &quot;hello_world&quot;&#13;&#10;      task : aurora_lib.task {&#13;&#10;        port_defs +: {&#13;&#10;          xmpp : Null # Creating a new null entry, defines a new port. By setting it to a string, it becomes an alias.&#13;&#10;        }&#13;&#10;        processes : {&#13;&#10;          hw : process {&#13;&#10;            process_name : &quot;hw&quot;&#13;&#10;            command_line : [ &quot;helloworldd --http &quot;, ports.http, &quot; --xmpp &quot;, ports.xmpp ]&#13;&#10;          }&#13;&#10;        }&#13;&#10;      }&#13;&#10;    }&#13;&#10;  ]&#13;&#10;}&#13;&#10;value : hw_file.value">Aurora: Ports</option>
						<option value="sum_of_squares : Template {&#13;&#10;    args : Required&#13;&#10;    value : For x : args Reduce acc + x * x With acc : 0&#13;&#10;}&#13;&#10;c : sum_of_squares(3, 4, 5)&#13;&#10;d : sum_of_squares(args : 3 Through 5)">Sum of Squares</option>
						<option value="a : { x : 1  y : 2  z : 3 }&#13;&#10;b : For n : Name, v : a Select n : v + 1 # Yields { x : 2  y : 3  z : 4 }&#13;&#10;c : For v : a Reduce v + acc With acc : 0 # Yields 6">Fricassée: Intro</option>
						<option value="x : 1 Through 7&#13;&#10;cumulative_sum :&#13;&#10;  For v : x&#13;&#10;    Accumulate current_sum + v&#13;&#10;      With current_sum : 0&#13;&#10;    Select current_sum">Fricassée: Accumulation</option>
						<option value="item_base_tmpl : Template {&#13;&#10;  name : Required&#13;&#10;  country : Required&#13;&#10;}&#13;&#10;item_xml_tmpl : Template item_base_tmpl {&#13;&#10;  value : &quot;&lt;person&gt;&lt;name&gt;\(name)&lt;/name&gt;&lt;country&gt;\(country)&lt;/country&gt;&lt;/person&gt;&quot;&#13;&#10;}&#13;&#10;item_pretty_tmpl : Template item_base_tmpl {&#13;&#10;  value : &quot;name: \(name) country: \(country)&#13;&#10;&quot;&#13;&#10;}&#13;&#10;item_tmpl : If xml_output Then item_xml_tmpl Else item_pretty_tmpl&#13;&#10;items : [&#13;&#10;  item_tmpl { name : &quot;Andre&quot;  country : &quot;Canada&quot; },&#13;&#10;  item_tmpl { name : &quot;Gráinne&quot;  country : &quot;Ireland&quot; }&#13;&#10;]">Templates: Base Change</option>
						<option value="arg_tmpl : Template {&#13;&#10;  name : Required&#13;&#10;  value : Required&#13;&#10;  spec : &quot;--&quot; &amp; name &amp; &quot; &quot; &amp; value&#13;&#10;}&#13;&#10;switch_tmpl : Template {&#13;&#10;  name : Required&#13;&#10;  active : True&#13;&#10;  spec : If active Then &quot;--&quot; &amp; name Else &quot;&quot;&#13;&#10;}&#13;&#10;binary : &quot;foo&quot;&#13;&#10;args : {&#13;&#10;  input : arg_tmpl { name : &quot;input&quot;  value : &quot;~/input.txt&quot; }&#13;&#10;  compression : arg_tmpl { name : &quot;c&quot;  value : 8 }&#13;&#10;  log : switch_tmpl { name : &quot;log&quot; }&#13;&#10;}&#13;&#10;arg_str : For arg : args Reduce acc &amp; &quot; &quot; &amp; arg.spec With acc : binary&#13;&#10;">Self-Rendering</option>
						<option value="sql_lib : From lib:sql&#13;&#10;query : Template sql_lib.offline_query {&#13;&#10;   table : {&#13;&#10;      employee : offline_table_tmpl {&#13;&#10;        table_schema : &quot;public&quot;&#13;&#10;        table_name : &quot;employee&quot;&#13;&#10;        columns +: {&#13;&#10;            emp_id : column_tmpl { column_name : &quot;id&quot;  sql_type : sql_types.int }&#13;&#10;            given_name : column_tmpl { column_name : &quot;firstName&quot;  sql_type : sql_types.str }&#13;&#10;            family_name : column_tmpl { column_name : &quot;lastName&quot;  sql_type : sql_types.str }&#13;&#10;        }&#13;&#10;      }&#13;&#10;      payroll : offline_table_tmpl {&#13;&#10;        table_schema : &quot;public&quot;&#13;&#10;        table_name : &quot;payroll&quot;&#13;&#10;        columns +: {&#13;&#10;            emp_id : column_tmpl { column_name : &quot;employee_id&quot;  sql_type : sql_types.int }&#13;&#10;            salary : column_tmpl { column_name : &quot;salary&quot;  sql_type : sql_types.int }&#13;&#10;            hired : column_tmpl { column_name : &quot;hireDate&quot;  sql_type : sql_types.timestamp }&#13;&#10;            terminated : column_tmpl { column_name : &quot;terminationDate&quot;  sql_type : sql_types.timestamp }&#13;&#10;        }&#13;&#10;      }&#13;&#10;   }&#13;&#10;   columns : {&#13;&#10;      name : expr.str_join { args : [ column.employee.given_name, &quot; &quot;, column.employee.family_name ] }&#13;&#10;      salary : column.payroll.salary&#13;&#10;  }&#13;&#10;  where +: {&#13;&#10;      emp_join : expr.equal { left : column.employee.emp_id  right : column.payroll.emp_id }&#13;&#10;  }&#13;&#10;}&#13;&#10;value : For provider : sql_lib.sql_providers, name : Name Select name : query { provider : Drop }">SQL</option>
					</select>
				</td>
			</tr>
			<tr><td height="90%" width="50%"><asp:TextBox id="script" runat="server" TextMode="multiline" /></td><td id="result" rowspan="2"><asp:Label id="output" runat="server" /></td></tr>
			<tr><td><asp:Button id="run" runat="server" Text="Do the Thing, Zhu Li!" OnClick="runScript" /></td></tr>
		</table>
	</form>
</body>
</html>

