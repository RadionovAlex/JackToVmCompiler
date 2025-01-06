using JackToVmCompiler.Tokenizer;

namespace JackToVmCompiler.VMWriter
{
    internal static class VmTranslationUtil
    {
       
        internal static void WritePushKeywordConstant(string token, VMWriter vmWriter)
        {
            var keyWordType = LexicalTables.ConstantKeyWordMap[token];
            switch (keyWordType)
            {
                //todo: check how true is passing in different scenarios
                case KeyWordType.True:
                    vmWriter.WritePush(SegmentKind.Const, 0);
                    vmWriter.WriteArithmetic(CommandKind.Not);
                    break;

                case KeyWordType.False:
                case KeyWordType.Null:
                    vmWriter.WritePush(SegmentKind.Const, 0);
                    break;

                case KeyWordType.This:
                    vmWriter.WritePush(SegmentKind.Pointer, 0);
                    break;
                    
                default:
                    throw new Exception($"cannot write KeywordConstant {keyWordType}");
            }
        }

        internal static void WriteUnaryOperator(UnaryOperatorType unaryOperatorKind, VMWriter vMWriter)
        {
            switch (unaryOperatorKind)
            {
                case UnaryOperatorType.Minus:
                    vMWriter.WriteArithmetic(CommandKind.Neg);
                    break;
                case UnaryOperatorType.BitwiseNot:
                    vMWriter.WriteArithmetic(CommandKind.Not);
                    break;
                default:
                    throw new Exception("Cannot parse unary operator");
            }
        }

        internal static string MethodNameLabel(string typeName, string methodName) => 
            $"{typeName}.{methodName}";
    }
}