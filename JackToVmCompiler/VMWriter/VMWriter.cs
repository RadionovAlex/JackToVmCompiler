using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackToVmCompiler.VMWriter
{
    internal class VMWriter
    {
        private StringBuilder _sb = new StringBuilder();

        internal VMWriter(string filePath)
        {

        }

        internal void WritePush(SegmentKind segment, int index)
        {
            _sb.AppendLine($"push {segment.SegmentName()} {index}");
        }

        internal void WritePop(SegmentKind segment, int index)
        {
            _sb.AppendLine($"pop {segment.SegmentName()} {index}");
        }

        internal void WriteArithmetic(CommandKind command)
        {
            _sb.AppendLine(command.ArithmeticCommandName());
        }

        internal void WriteLabel(string label)
        {
            _sb.AppendLine($"label {label}");
        }

        internal void WriteGoTo(string goToLabel)
        {
            _sb.AppendLine($"goto {goToLabel}");
        }

        internal void WriteIfGoTo(string goToLabel)
        {
            _sb.AppendLine($"if-goto {goToLabel}");
        }        

        internal void WriteCall(string name, int nArgs)
        {
            _sb.AppendLine($"call {name} {nArgs}");
        }

        internal void WriteFunction(string name, int nLocals)
        {
            _sb.AppendLine($"function {name} {nLocals}");
        }

        internal void WriteReturn()
        {

        }

        internal void Close()
        {

        }
    }
}