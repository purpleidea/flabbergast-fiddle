using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.UI.WebControls;
using Flabbergast;

namespace Flabbergast.Fiddle
{
public class WebTaskMaster : TaskMaster , Flabbergast.ErrorCollector  {
    private Label target;
    private StringWriter buffer = new StringWriter();
    private Dictionary<SourceReference, bool> seen = new Dictionary<SourceReference, bool>();
    private bool dirty = false;

    internal TextWriter Buffer {
        get {
            return buffer;
        }
    }
    public WebTaskMaster(Label target)
    {
        this.target = target;
    }
    public void ReportExpressionTypeError(CodeRegion where, Type new_type, Type existing_type) {
        MakeDirty();
        if (existing_type == 0) {
            buffer.Write("<tr><td>{0}:{1}-{2}:{3}</td><td>No possible type for {4}. Expression should have types: {5}.</td></tr>", where.StartRow, where.StartColumn, where.EndRow, where.EndColumn, where.PrettyName, new_type);
        } else {
            buffer.Write("<tr><td>{0}:{1}-{2}:{3}</td><td>Conflicting types for {4}: {5} versus {6}.</td></tr>", where.StartRow, where.StartColumn, where.EndRow, where.EndColumn, where.PrettyName, new_type, existing_type);
        }
    }

    public void ReportLookupTypeError(CodeRegion environment, string name, Type new_type, Type existing_type) {
        MakeDirty();
        if (existing_type == 0) {
            buffer.Write("<tr><td>{0}:{1}-{2}:{3}</td><td>No possible type for “{4}”. Expression should have types: {5}.</td></tr>", environment.StartRow, environment.StartColumn, environment.EndRow, environment.EndColumn, name, new_type);
        } else {
            buffer.Write("<tr><td>{0}:{1}-{2}:{3}</td><td>Lookup for “{4}” has conflicting types: {5} versus {6}.</td></tr>", environment.StartRow, environment.StartColumn, environment.EndRow, environment.EndColumn, name, new_type, existing_type);
        }
    }

    public void ReportForbiddenNameAccess(CodeRegion environment, string name) {
        MakeDirty();
        buffer.Write("<tr><td>{0}:{1}-{2}:{3}</td><td>Lookup for “{4}” is forbidden.<td></tr>", environment.StartRow, environment.StartColumn, environment.EndRow, environment.EndColumn, name);
    }

    public void ReportParseError(string filename, int index, int row, int column, string message) {
        MakeDirty();
        buffer.Write("<tr><td>{0}:{1}</td><td>{2}</td></tr>",  row, column, message);
    }

    public void ReportRawError(CodeRegion where, string message) {
        MakeDirty();
        buffer.Write("<tr><td>{0}:{1}-{2}:{3}</td><td>{4}</td></tr>", where.StartRow, where.StartColumn, where.EndRow, where.EndColumn, message);
    }

    public void ReportSingleTypeError(CodeRegion where, Type type) {
        MakeDirty();
        buffer.Write("<tr><td>{0}:{1}-{2}:{3}</td><td>The expression has types {4}, but it must only have one.</td></tr>", where.StartRow, where.StartColumn, where.EndRow, where.EndColumn, type);
    }

    public override void ReportExternalError(string uri, LibraryFailure reason) {
        MakeDirty();
        switch (reason) {
        case LibraryFailure.BadName:
            buffer.Write("<tr><td></td><td>The URI “{0}” is not a valid name.</td></tr>", uri);
            break;
        case LibraryFailure.Corrupt:
            buffer.Write("<tr><td></td><td>The URI “{0}” could not be loaded due to corruption.</td></tr>", uri);
            break;
        case LibraryFailure.Missing:
            buffer.Write("<tr><td></td><td>The URI “{0}” could not be found.</td></tr>", uri);
            break;
        default:
            buffer.Write("<tr><td></td><td>The URI “{0}” could not be resolved.</td></tr>", uri);
            break;
        }
    }
    public void ReportCircularEvaluation() {
        if (!HasInflightLookups || dirty) {
            return;
        }
        foreach (var lookup in this) {
            buffer.Write("<tr><td><td>Lookup for “{0}” blocked. Lookup initiated at:<pre>", lookup.Name);
            lookup.SourceReference.Write(buffer, "  ", seen);
            buffer.Write("</pre> is waiting for “{0}” in frame defined at:<pre>", lookup.LastName);
            lookup.LastFrame.SourceReference.Write(buffer, "  ", seen);
            buffer.Write("</td></tr>");
        }
    }

    public override void ReportOtherError(SourceReference reference, string message) {
        MakeDirty();
        buffer.Write("<tr><td>");
        reference.Write(buffer, "  ", seen);
        buffer.Write("</td><td>{0}</td></tr>", message);
    }
    private void MakeDirty() {
        if (!dirty) {
            dirty = true;
            buffer.Write("<table><tr><th>Location</th><th><th>Error</th></tr>");
        }
    }
    public void Finish() {
        if (dirty) {
            buffer.Write("</table>");
        }
        target.Text = buffer.ToString();
    }


}
public class CompleteResult : Computation {
    private readonly Computation source;
    private long interlock = 1;
    public CompleteResult(WebTaskMaster task_master, Computation source) : base(task_master) {
        this.source = source;
    }
    private void HandleResult(object result) {
        if (this.result == null) {
            this.result = result;
        }
        if (result is Frame) {
            var frame_result = (Frame) result;
            Interlocked.Add(ref interlock, frame_result.Count);
            foreach (var attr_name in frame_result.GetAttributeNames()) {
                frame_result.GetOrSubscribe(attr_name, HandleResult);
            }

        }
        if (Interlocked.Decrement(ref interlock) == 0) {
            WakeupListeners();
        }
    }
    protected override bool Run() {

        source.Notify(HandleResult);
        task_master.Slot(source);
        return false;
    }

}
public class WebResult : Computation {
    public bool Success {
        get;
        private set;
    }
    private readonly TextWriter target;
    private readonly Computation source;
    private Dictionary<Frame, bool> seen = new Dictionary<Frame, bool>();

    public WebResult(WebTaskMaster task_master, Computation source) : base(task_master) {
        this.source = source;
        target = task_master.Buffer;
    }

    private void HandleResult(object result) {
        if (result is bool) {
            Success = true;
            target.Write((bool) result ? "True" : "False");
        } else if (result is Stringish || result is long || result is double) {
            Success = true;
            target.Write("{0}", HttpUtility.HtmlEncode(result));
        } else if (result is Template) {
            target.Write("Template {");
            foreach (var attr in((Template)result).GetAttributeNames()) {
                target.Write(" {0}" , attr);
            }
            target.Write(" }");
        } else if (result is Frame) {
            var frame_result = (Frame)result;
            if (seen.ContainsKey(frame_result)) {
                target.Write("<a href='#{0}'>Frame {0}</a>", frame_result.Id);
                return;
            }
            seen[frame_result] = true;
            target.Write("<table id='{0}' title='{0}'>", frame_result.Id);
            foreach (var attr_name in frame_result.GetAttributeNames()) {
                target.Write("<tr class='{1}'><td>{0}</td><td>", attr_name, Stringish.NameForType(frame_result[attr_name].GetType()));
                HandleResult(frame_result[attr_name]);
                target.Write("</td></tr>");
            }

            target.Write("</table>");

        } else if (result is Computation) {
            target.Write("Unfinished computation");

        } else {
            target.Write("Unknown value of type {0}.", Stringish.NameForType(result.GetType()));
        }
    }

    protected override bool Run() {
        source.Notify(HandleResult);
        task_master.Slot(source);
        return false;
    }
}
}

