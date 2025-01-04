namespace JackToVmCompiler.VMWriter
{
    internal enum SegmentKind
    {
        Const,
        Arg,
        Local,
        Static,
        This,
        That,
        Pointer,
        Temp
    }

    internal static class SegmentKindExtensions
    {
        internal static Dictionary<SegmentKind, string> KindNames = new Dictionary<SegmentKind, string>()
        {
            {SegmentKind.Const, "constant"},
            {SegmentKind.Arg, "argument"},
            {SegmentKind.Local, "local"},
            {SegmentKind.Static, "static"},
            {SegmentKind.This, "this"},
            {SegmentKind.That, "that"},
            {SegmentKind.Pointer, "pointer"},
            {SegmentKind.Temp, "temp"},
        };

        internal static string SegmentName(this SegmentKind kind) => KindNames[kind];
    }
}
