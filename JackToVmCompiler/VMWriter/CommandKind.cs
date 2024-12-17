using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackToVmCompiler.VMWriter
{
    internal enum CommandKind
    {
        Add,
        Sub,
        Neg,
        Eq,
        Gt,
        Lt,
        And,
        Or,
        Not
    }

    internal static class CommandKindExtensions
    {
        internal static Dictionary<CommandKind, string> CommandKindNames = new Dictionary<CommandKind, string>()
        {
            {CommandKind.Add, "add"},
            {CommandKind.Sub, "sub"},
            {CommandKind.Neg, "neg"},
            {CommandKind.Eq, "eq"},
            {CommandKind.Gt, "gt"},
            {CommandKind.Lt, "lt"},
            {CommandKind.And, "and"},
            {CommandKind.Or, "or"},
            {CommandKind.Not, "not"},
        };

        internal static string ArithmeticCommandName(this CommandKind kind) => CommandKindNames[kind];
    }
}
