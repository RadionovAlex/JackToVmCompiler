namespace JackToVmCompiler.Tokenizer
{
    internal class JackTokenizer
    {
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

        

        public TokenType GetTokenType()
        {
            if (LexicalTables.IsKeyword(_currentToken))
                return TokenType.KeyWord;

            if (LexicalTables.IsSymbol(_currentToken))
                return TokenType.Symbol;

            if (LexicalTables.IsIdentifier(_currentToken))
                return TokenType.Identifier;

            if (LexicalTables.IsStringConstant(_currentToken))
                return TokenType.StringConst;

            if (LexicalTables.IsIntegerConstant(_currentToken))
                return TokenType.IntConst;

            throw new Exception($"Cannot identify tokenType of : {_currentToken}");
        }

        public KeyWordType GetKeyWordType() =>
            LexicalTables.KeywordsMap[_currentToken];

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

        private void ProcessLine(string line) =>
            _tokens.AddRange(line.Split(' '));
    }
}