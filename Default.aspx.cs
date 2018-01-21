using System;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using Flabbergast;

namespace Flabbergast.Fiddle
{

public partial class Default : System.Web.UI.Page
{

    private IDbConnection Connection;
    private LoadPrecompiledLibraries precomp = new LoadPrecompiledLibraries();

    public Default() {
        var str = WebConfigurationManager.ConnectionStrings["savedSnippetsDb"];
        Connection = DbProviderFactories.GetFactory(str.ProviderName).CreateConnection();
        Connection.ConnectionString = str.ConnectionString;
        Connection.Open();
        precomp.Finder = new ResourcePathFinder();
        precomp.Finder.AddDefault();
    }
    public void runScript(object sender, EventArgs args)
    {
        var hash = CreateSnippet(script.Text);
        var builder = new UriBuilder(HttpContext.Current.Request.Url.AbsoluteUri);
        builder.Query = String.Format("saved={0}", hash);
        link.NavigateUrl = builder.Uri.ToString();
        link.Visible = true;
        try {
            output.Text = "";
            CompilationUnit unit;
            int type_counter = 0;
            if (Session["unit"] == null) {
                var assembly_builder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("Fiddle" + Session.SessionID), AssemblyBuilderAccess.Run);
                var module_builder = assembly_builder.DefineDynamicModule("ReplModule");
                unit = new CompilationUnit(module_builder, false);
                Session["unit" ] = unit;
            } else  {
                unit = (CompilationUnit)Session["unit"];
                type_counter = (int)Session["type_counter" ];
            }
            Session["type_counter" ] = type_counter + 1;

            var task_master = new WebTaskMaster(output, filter_lib.Checked);
            task_master.AddUriHandler(new CurrentInformation(false));
            task_master.AddUriHandler(BuiltInLibraries.INSTANCE);
            task_master.AddUriHandler(precomp);
            var parser = new Parser("web", script.Text + "\n");
            parser.DisableExtensions = true;
            var root_type = parser.ParseFile(task_master, unit, "WebForm" + type_counter);
            object result = null;
            if (root_type != null) {
                var computation = (Computation) Activator.CreateInstance(root_type, task_master);
                computation.Notify(r => result = r);
                computation.Slot();
                task_master.Run();
                task_master.ReportCircularEvaluation();
                if (result != null) {
                    task_master.PrintResult(result);
                }
            }
            task_master.Finish();
        } catch (Exception e) {
            output.Text = e.Message;
            Console.WriteLine(e);
        }
    }
    public void Page_Load(object sender, EventArgs args) {
        var savedId = Request.QueryString["saved"];
        uint id;
        if (savedId != null && uint.TryParse(savedId, out id)) {
            var str = GetSnippet(id);
            if (str != null) {
                UpdateSnippet(id);
                script.Text = str;
            }
        }
    }

    private string GetSnippet(uint id) {
        var query = Connection.CreateCommand();
        query.CommandText = "SELECT content FROM snippets WHERE hash_code = ?";
        var idParam = query.CreateParameter();
        idParam.Value = id;
        query.Parameters.Add(idParam);
        return (string) query.ExecuteScalar();
    }
    private void UpdateSnippet(uint id) {
        var update = Connection.CreateCommand();
        update.CommandText = "UPDATE snippets SET last_used = ?, hits = hits + 1 WHERE hash_code = ?";
        var usedParam = update.CreateParameter();
        usedParam.Value = DateTime.Now;
        update.Parameters.Add(usedParam);
        var idParam = update.CreateParameter();
        idParam.Value = id;
        update.Parameters.Add(idParam);
        update.ExecuteNonQuery();
    }
    private uint CreateSnippet(string content) {
        uint hash = 2166136261;
        foreach (var octet in Encoding.UTF8.GetBytes(script.Text)) {
            hash ^=  octet;
            hash *= 16777619;
        }

        var str = GetSnippet(hash);
        if (str != null) {
            return hash;
        }
        var update = Connection.CreateCommand();
        update.CommandText = "INSERT INTO snippets(hash_code, content, last_used) VALUES (?, ?, ?)";
        var idParam = update.CreateParameter();
        idParam.Value = hash;
        update.Parameters.Add(idParam);
        var contentParam = update.CreateParameter();
        contentParam.Value = script.Text;
        update.Parameters.Add(contentParam);
        var usedParam = update.CreateParameter();
        usedParam.Value = DateTime.Now;
        update.Parameters.Add(usedParam);
        update.ExecuteNonQuery();
        return hash;
    }
}
}
