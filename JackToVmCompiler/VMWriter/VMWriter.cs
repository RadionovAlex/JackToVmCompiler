using System.Text;

namespace JackToVmCompiler.VMWriter
{
    internal class VMWriter
    {
        private StringBuilder _sb;

        private const string Offset = "    ";

        internal static string TemplateName(string funcName) => $"#n_locals_for_{funcName}#";

        internal VMWriter(StringBuilder sb)
        {
            _sb = sb;
        }

        internal void WritePush(SegmentKind segment, int index)
        {
            _sb.AppendLine($"{Offset}push {segment.SegmentName()} {index}");
        }

        internal void WritePop(SegmentKind segment, int index)
        {
            _sb.AppendLine($"{Offset}pop {segment.SegmentName()} {index}");
        }

        internal void WriteArithmetic(CommandKind command)
        {
            _sb.AppendLine($"{Offset}{command.ArithmeticCommandName()}");
        }

        internal void WriteLabel(string label)
        {
            _sb.AppendLine($"label {label}");
        }

        internal void WriteGoTo(string goToLabel)
        {
            _sb.AppendLine($"{Offset}goto {goToLabel}");
        }

        internal void WriteIfGoTo(string goToLabel)
        {
            _sb.AppendLine($"{Offset}if-goto {goToLabel}");
        }        

        internal void WriteCall(string name, int nArgs)
        {
            _sb.AppendLine($"{Offset}call {name} {nArgs}");
        }

        internal void WriteFunction(string name, int nLocals)
        {
            _sb.AppendLine($"function {name} {nLocals}");
        }

        internal void WriteFunctionWithNumberTemplate(string name)
        {
            _sb.AppendLine($"function {name} {TemplateName(name)}");
        }

        internal void WriteReturn()
        {
            _sb.AppendLine($"{Offset}return");
        }

        internal void Close()
        {

        }
    }
}