using System.Text.RegularExpressions;

namespace JackToVmCompiler.Tokenizer
{
    public static class LexicalTables
    {
        public const string OpenSquareBracket = "[";
        public const string CloseSquareBracket = "]";
        public const string OpenParenthesis = "(";
        public const string CloseParenthesis = ")";
        public const string OpenBracket = "{";
        public const string CloseBracket = "}";
        public const string Coma = ",";
        public const string Dot = ".";
        public const string ComaAndDot = ";";
        public const string Equal = "=";

        public static readonly Regex StringConstantRegex = new Regex("\"[^\n\"]*\"");
        public static readonly Regex IdentifierRegex = new Regex("[a-zA-Z_][a-zA-Z0-9_]*");
        public readonly Regex IntegerConstantRegex = new Regex("^[0-9]+$");


        public static readonly Dictionary<string, KeyWordType> KeywordsMap = new Dictionary<string, KeyWordType>()
        {
            { "class", KeyWordType.Class },
            { "method", KeyWordType.Method },
            { "function", KeyWordType.Function },
            { "constructor", KeyWordType.Constructor },
            { "int", KeyWordType.Int },
            { "boolean", KeyWordType.Boolean },
            { "char", KeyWordType.Char },
            { "void", KeyWordType.Void },
            { "var", KeyWordType.Var },
            { "static", KeyWordType.Static },
            { "field", KeyWordType.Field },
            { "let", KeyWordType.Let },
            { "do", KeyWordType.Do },
            { "if", KeyWordType.If },
            { "else", KeyWordType.Else },
            { "while", KeyWordType.While },
            { "return", KeyWordType.Return },
            { "true", KeyWordType.True },
            { "false", KeyWordType.False },
            { "null", KeyWordType.Null },
            { "this", KeyWordType.This }
        };

        public static readonly Dictionary<string, KeyWordType> ConstantKeyWordMap = new Dictionary<string, KeyWordType>()
        {
            { "true", KeyWordType.True },
            { "false", KeyWordType.False },
            { "null", KeyWordType.Null },
            { "this", KeyWordType.This }
        };

        public static readonly Dictionary<string, UnaryOperatorType> UnaryOperatorsMap = new Dictionary<string, UnaryOperatorType>()
        {
            {"-", UnaryOperatorType.Minus },
            {"~", UnaryOperatorType.BitwiseNot }
        };

        public static readonly Dictionary<string, OperatorTypes> OperatorTypesMap = new Dictionary<string, OperatorTypes>()
        {
            {"+", OperatorTypes.Plus},
            {"-", OperatorTypes.Minus},
            {"*", OperatorTypes.Mult},
            {"/", OperatorTypes.Division},
            {"&", OperatorTypes.BitwiseAnd},
            {"|", OperatorTypes.BitwiseOr},
            {"<", OperatorTypes.LessThan },
            {">", OperatorTypes.GreaterThan},
            {"=", OperatorTypes.EqualTo }
        };

        public static readonly Dictionary<string, CompareOperators> CompareOperatorsMap = new Dictionary<string, CompareOperators>()
        {
            {"<", CompareOperators.Less },
            {">", CompareOperators.Great },
            {"=", CompareOperators.Equal }

        };        

        public static readonly HashSet<char> SymbolsTable = new HashSet<char>()
        {
            '{', '}', '(', ')', '[', ']', '.', ',', ';', '+', '-', '*', '/', '&', '|', '<', '>', '=', '~'
        };

        public static readonly HashSet<char> OperatorsTable = new HashSet<char>()
        {
          '+', '-', '*', '/', '&', '|', '<', '>', '='
        };

        public static readonly HashSet<string> OperatorsTableString = new HashSet<string>()
        {
          "+", "-", "*", "/", "&", "|", "<", ">", "="
        };

        public static readonly HashSet<char> UnaryOperators = new HashSet<char>()
        {
            '-','~'
        };

        public static readonly HashSet<string> UnaryOperatorsString = new HashSet<string>()
        {
            "-","~"
        };

        public static bool IsOperator(char value) => OperatorsTable.Contains(value);

        public static bool IsUnaryOperator(char value) => UnaryOperators.Contains(value);

        public static bool IsKeyword (string value) => KeywordsMap.ContainsKey(value);

        public static bool IsSymbol(string value) => value.Length == 1 && SymbolsTable.Contains(value[0]);

        public static bool IsConstantKeyWord(string value) => ConstantKeyWordMap.ContainsKey(value);

        public static bool IsStringConstant (string value) => StringConstantRegex.Match(value).Success;

        public static bool IsIdentifier (string value) => IdentifierRegex.Match(value).Success;

        public static bool IsVar (string value) => KeywordsMap.ContainsKey(value) && KeywordsMap[value] == KeyWordType.Var;
        
        public static bool IsIntegerConstant (string input)
        {
            if (!IntegerConstantRegex.IsMatch(input))
                return false;

            if (!int.TryParse(input, out int value))
                return false;

            var isValid = value >= 0 && value <= 32767;

            if (!isValid)
                throw new Exception($"It is integer constant, but value is not in requrd range [0; 32767] : {value}");

            return isValid;

        }
    }
}
