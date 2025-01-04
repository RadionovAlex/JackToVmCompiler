namespace JackToVmCompiler.VMWriter
{
    [Flags]
    internal enum CommandKind
    {
        None = 0,
        Add = 1 << 0,
        Sub = 1 << 1,
        Neg = 1 << 2,
        Eq = 1 << 3,
        Gt = 1 << 4, 
        Lt = 1 << 5,
        And = 1 << 7,
        Or = 1 << 8,
        Not = 1 << 9,
        Mult = 1 << 10,
        Divide = 1 << 11,
        Geq = Gt | Eq,
        Leq = Lt | Eq,
    }

    internal static class CommandKindExtensions
    {
        internal static Dictionary<string, CommandKind> JackCommandToVmCommandMap = new Dictionary<string, CommandKind>()
        {
            {"+", CommandKind.Add },
            {"-", CommandKind.Sub},
            {"=", CommandKind.Eq},
            {">", CommandKind.Gt},
            {"<", CommandKind.Lt},
            {"&", CommandKind.And},
            {"|", CommandKind.Or},
            {"*", CommandKind.Mult},
            {"/", CommandKind.Divide},
        };

        internal static Dictionary<CommandKind, string> CommandKindNames = new Dictionary<CommandKind, string>()
        {
            {CommandKind.Add, "add"},
            {CommandKind.Sub, "sub"},
            {CommandKind.Neg, "neg"},
            {CommandKind.Eq, "eq"},
            {CommandKind.Gt, "gt"},
            {CommandKind.Lt, "lt"},
            {CommandKind.Geq, "geq" },
            {CommandKind.Leq, "leq" },
            {CommandKind.And, "and"},
            {CommandKind.Or, "or"},
            {CommandKind.Not, "not"},
            {CommandKind.Mult, "call Math.multiply 2" },
            {CommandKind.Divide, "call Math.divide 2" },
        };

        internal static string ArithmeticCommandName(this CommandKind kind) => CommandKindNames[kind];
    }
}