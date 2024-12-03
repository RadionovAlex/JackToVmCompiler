using System.Text.RegularExpressions;

namespace JackToVmCompiler.Tokenizer
{
    internal class JackTokenizer
    {
        const string Pattern = @"(?<quoted>""[^""]*"")|(?<symbol>[\(\)\{\}\[\]\;\.\,\-\+\~\|\&\s])";

        private readonly List<string> _originalLines;
        private readonly List<string> _tokens;

        private int _currentIndex;
        private string _currentToken;

        public JackTokenizer(List<string> textLines)
        {
            _originalLines = textLines;
            _tokens = new List<string>();
            _currentIndex = -1;

            SplitLinesOnTokens();
        }

        public List<string> Tokens => _tokens;

        public bool HasMoreTokens() =>
            _currentIndex < _originalLines.Count - 1;

        public string CurrentToken => _tokens[_currentIndex];
        public string NextToken => _tokens[_currentIndex + 1];

        public int TokensUntil(string untilToken)
        {
            var tempIndex = _currentIndex;
            while(tempIndex < _tokens.Count && _tokens[tempIndex] != untilToken)
            {
                tempIndex++;
            }

            return tempIndex - _currentIndex;
        }


        public void Next()
        {
            _currentIndex++;
            _currentToken = _tokens[_currentIndex];
        }



        public TokenType GetTokenType() => GetTokenType(CurrentToken);

        public TokenType GetNextTokenType() => GetTokenType(NextToken);
       

        private TokenType GetTokenType(string token)
        {
            if (LexicalTables.IsKeyword(token))
                return TokenType.KeyWord;

            if (LexicalTables.IsSymbol(token))
                return TokenType.Symbol;

            if (LexicalTables.IsIdentifier(token))
                return TokenType.Identifier;

            if (LexicalTables.IsStringConstant(token))
                return TokenType.StringConst;

            if (LexicalTables.IsIntegerConstant(token))
                return TokenType.IntConst;

            throw new Exception($"Cannot identify tokenType of : {token}");
        }


        public KeyWordType GetKeyWordType() =>
            LexicalTables.KeywordsMap[_currentToken];

        public KeyWordType GetNextKeywordType() => LexicalTables.KeywordsMap[NextToken];

        public char GetSymbol() =>
            _currentToken[0];

        public string GetIdentifier() => _currentToken;

        public int GetIntConst() => int.Parse(_currentToken);

        public string GetStringConst() => _currentToken;
        
        private void SplitLinesOnTokens()
        {
            foreach (var line in _originalLines)
                ProcessLine(line);
        }

        private void ProcessLine(string input)
        {
            var matches = Regex.Matches(input, Pattern);
            var result = new List<string>();
            var lastIndex = 0;

            foreach (Match match in matches)
            {
                if (match.Groups["quoted"].Success)
                {
                    result.Add(match.Groups["quoted"].Value);
                    lastIndex = match.Index + match.Length;
                }
                else if (match.Groups["symbol"].Success)
                {
                    if (match.Index > lastIndex)
                    {
                        string beforeSymbol = input.Substring(lastIndex, match.Index - lastIndex);
                        result.Add(beforeSymbol);
                    }
                    result.Add(match.Value.Trim());
                    lastIndex = match.Index + match.Length;
                }
            }

            if (lastIndex < input.Length)
            {
                result.Add(input.Substring(lastIndex));
            }

            result.RemoveAll(string.IsNullOrWhiteSpace);

            _tokens.AddRange(result);
        }
    }
}