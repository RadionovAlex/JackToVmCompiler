using JackToVmCompiler.VMWriter;

namespace JackToVmCompiler.Jack
{
    internal enum SymbolKind
    {
        Static,
        Field,
        Arg,
        Var
    }

    internal static class SymbolKindExtensions
    {
        internal static SegmentKind ToSegmentKind(this SymbolKind kind)
        {
            switch (kind)
            {
                case SymbolKind.Static: 
                    return SegmentKind.Static;

                case SymbolKind.Field:
                    return SegmentKind.This;

                case SymbolKind.Arg:
                    return SegmentKind.Arg;

                    case SymbolKind.Var:
                    return SegmentKind.Local;

                default: throw new Exception("Cannot translate SymbolKind into SegmentKind");
            }
        }
    }
}