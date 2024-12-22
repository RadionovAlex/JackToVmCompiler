namespace JackToVmCompiler.Tokenizer
{
    internal static class JackTokenizerUtil
    {
        public static bool IsValidVariableKeyWord(KeyWordType keyword)
        {
            var isValid = false;
            isValid |= keyword == KeyWordType.Int;
            isValid |= keyword == KeyWordType.Char;
            isValid |= keyword == KeyWordType.Boolean;

            return isValid;
        }

        public static bool IsValidProcedureTypeKeyWord(KeyWordType keyword) => 
            keyword == KeyWordType.Void || IsValidVariableKeyWord(keyword);

        public static bool IsVariableDeclarationKeyWord(KeyWordType keyword)
        {
            var isVariableDeclaration = false;
            isVariableDeclaration |= keyword == KeyWordType.Static;
            isVariableDeclaration |= keyword == KeyWordType.Field;

            return isVariableDeclaration;
        }

        public static bool IsProcedureDeclarationKeyword(KeyWordType keyword)
        {
            var isProcedureDeclaration = false;
            isProcedureDeclaration |= keyword == KeyWordType.Constructor;
            isProcedureDeclaration |= keyword == KeyWordType.Method;
            isProcedureDeclaration |= keyword == KeyWordType.Function;

            return isProcedureDeclaration;
        }
    }
}
