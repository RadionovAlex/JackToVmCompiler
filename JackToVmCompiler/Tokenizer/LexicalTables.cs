﻿using System.Text.RegularExpressions;

namespace JackToVmCompiler.Tokenizer
{
    public static class LexicalTables
    {
        private static readonly Regex StringConstantRegex = new Regex("\"[^\n\"]*\"");
        private static readonly Regex IdentifierRegex = new Regex("[a-zA-Z_][a-zA-Z0-9_]*");
        private static readonly Regex IntegerConstantRegex = new Regex("^[0-9]+$");


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

        public static readonly HashSet<char> SymbolsTable = new HashSet<char>()
        {
            '{', '}', '(', ')', '[', ']', '.', ',', ';', '+', '-', '*', '/', '&', '|', '<', '>', '=', '~'
        };

        public static bool IsKeyword (string value) => KeywordsMap.ContainsKey(value);

        public static bool IsSymbol(string value) => value.Length == 1 && SymbolsTable.Contains(value[0]);

        public static bool IsStringConstant (string value) => StringConstantRegex.Match(value).Success;

        public static bool IsIdentifier (string value) => IdentifierRegex.Match(value).Success;
        
        public static bool IsIntegerConstant (string input)
        {
            if (!IntegerConstantRegex.IsMatch(input))
                return false;

            if (int.TryParse(input, out int value))
                return false;

            var isValid = value >= 0 && value <= 32767;

            if (!isValid)
                throw new Exception($"It is integer constant, but value is not in requrd range [0; 32767] : {value}");

            return isValid;

        }
    }
}