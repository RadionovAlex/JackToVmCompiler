using System.Text;

namespace JackToVmCompiler.VMWriter
{
    internal class VMWriter
    {
        private StringBuilder _sb;

        internal static string TemplateName(string funcName) => $"#n_locals_for_{funcName}#";

        internal VMWriter(StringBuilder sb)
        {
            _sb = sb;
        }

        internal void WritePush(SegmentKind segment, int index)
        {
            _sb.AppendLine($"push {segment.SegmentName()} {index} \n");
        }

        internal void WritePop(SegmentKind segment, int index)
        {
            _sb.AppendLine($"pop {segment.SegmentName()} {index} \n");
        }

        internal void WriteArithmetic(CommandKind command)
        {
            _sb.AppendLine($"{command.ArithmeticCommandName()} \n");
        }

        internal void WriteLabel(string label)
        {
            _sb.AppendLine($"label {label} \n");
        }

        internal void WriteGoTo(string goToLabel)
        {
            _sb.AppendLine($"goto {goToLabel} \n");
        }

        internal void WriteIfGoTo(string goToLabel)
        {
            _sb.AppendLine($"if-goto {goToLabel} \n");
        }        

        internal void WriteCall(string name, int nArgs)
        {
            _sb.AppendLine($"call {name} {nArgs} \n");
        }

        internal void WriteFunction(string name, int nLocals)
        {
            _sb.AppendLine($"function {name} {nLocals} \n");
        }

        internal void WriteFunctionWithNumberTemplate(string name)
        {
            _sb.AppendLine($"function {name} {TemplateName(name)} \n");
        }

        internal void WriteReturn()
        {

        }

        internal void Close()
        {

        }
    }
}