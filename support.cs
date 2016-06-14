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
    private Dictionary<Frame, bool> result_seen = new Dictionary<Frame, bool>();
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
        MakeDirty();
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
            buffer.Write("<table class='errors'><tr><th>Location</th><th><th>Error</th></tr>");
        }
    }
    public void Finish() {
        if (dirty) {
            buffer.Write("</table>");
        }
        target.Text = buffer.ToString();
    }
    public void PrintResult(object result) {
        if (dirty) {
            buffer.Write("</table>");
            dirty = false;
        }
        buffer.Write("<div class='output'>");
        PrintResultHelper(result);
        buffer.Write("</>");
    }

    private void PrintResultHelper(object result) {
        if (result is bool) {
            buffer.Write((bool) result ? "True" : "False");
        } else if (result is Stringish || result is long || result is double) {
            buffer.Write("{0}", HttpUtility.HtmlEncode(result));
        } else if (result is Template) {
            buffer.Write("Template {");
            foreach (var attr in((Template)result).GetAttributeNames()) {
                buffer.Write(" {0}" , attr);
            }
            buffer.Write(" }");
        } else if (result is Frame) {
            var frame_result = (Frame)result;
            if (result_seen.ContainsKey(frame_result)) {
                buffer.Write("<a href='#{0}'>Frame {0}</a>", frame_result.Id);
                return;
            }
            result_seen[frame_result] = true;
            buffer.Write("<p>Frame {0}</p><table id='{0}' title='{0}'>", frame_result.Id);
            foreach (var attr_name in frame_result.GetAttributeNames()) {
                var type = frame_result[attr_name].GetType();
                buffer.Write("<tr class='{1}' title='{1}'><td>{0}</td><td>", attr_name, typeof(Computation).IsAssignableFrom(type) ? "" : Stringish.NameForType(type));
                PrintResultHelper(frame_result[attr_name]);
                buffer.Write("</td></tr>");
            }

            buffer.Write("</table>");

        } else if (result is Computation) {
            buffer.Write("Unfinished computation");

        } else {
            buffer.Write("Unknown value of type {0}.", Stringish.NameForType(result.GetType()));
        }
    }
}
}

