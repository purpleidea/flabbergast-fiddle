using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Web;
using System.Web.UI;
using Flabbergast;

namespace Flabbergast.Fiddle
{

public partial class Default : System.Web.UI.Page
{
    public void runScript(object sender, EventArgs args)
    {
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

            var task_master = new WebTaskMaster(output);
            task_master.AddUriHandler(new CurrentInformation(false));
            task_master.AddUriHandler(BuiltInLibraries.INSTANCE);
            var precomp = new LoadPrecompiledLibraries();
            task_master.AddUriHandler(precomp);
            var parser = new Parser("web", script.Text + "\n");
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
}
}
