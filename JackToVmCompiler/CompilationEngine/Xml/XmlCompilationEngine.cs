using JackToVmCompiler.Tokenizer;
using System.Text;

namespace JackToVmCompiler.CompilationEngine.Xml
{
    internal class XmlCompilationEngine : ICompilationEngine
    {
        private const string SubroutineCallCompileMarkupName = "subroutineCall";
        private const string TermCompileMarkupName = "term";
        private const string ClassCompileMarkupName = "class";
        private const string ClassVarDecMarkupName = "classVarDec";
        private const string SubroutineDecMarkupName = "subroutineDec";
        private const string SubroutineBodyMarkupName = "subroutineBody";
        private const string VarDecMarkupName = "varDec";
        private const string ParameterListMarkupName = "parameterList";
        private const string ExpressionMarkupName = "expression";
        private const string ExpressionListMarkupName = "expressionList";
        private const string LetStatementMarkupName = "letStatement";
        private const string IfStatementMarkupName = "ifStatement";
        private const string WhileStatementMarkupName = "whileStatement";
        private const string DoStatementMarkupName = "doStatement";
        private const string ReturnStatementMarkupName = "returnStatement";
        private const string StatementsMarkupName = "statements";

        private string _outputFilePath;
        private JackTokenizer _tokenizer;
        private StringBuilder _sb;

        private int _currentOffsetTabs = 0;
        private string Offset
        {
            get
            {
                var _sb = new StringBuilder();
                for (int i = 0; i < _currentOffsetTabs; i++)
                {
                    _sb.Append("\t");
                }

                return _sb.ToString();
            }
        }

        private string CurrentToken => _tokenizer.CurrentToken;

        private string NextToken => _tokenizer.NextToken;

        public XmlCompilationEngine(string sourcePath, JackTokenizer tokenizer)
        {
            
            _tokenizer = tokenizer;
            _sb = new StringBuilder(_tokenizer.Tokens.Count * 10);
            var pathDirectory = Path.GetDirectoryName(sourcePath);
            var fileName = Path.GetFileNameWithoutExtension(sourcePath);
            _outputFilePath = $"{Path.Combine(pathDirectory, fileName)}.xml";
           
        }

        public void Compile()
        {
            while (_tokenizer.HasMoreTokens())
            {
                _tokenizer.Next();
                var tokenType = _tokenizer.GetTokenType();
                switch (tokenType)
                {
                    case TokenType.KeyWord:
                        var keywordType = _tokenizer.GetKeyWordType();
                        if(keywordType == KeyWordType.Class)
                        {
                            WrapIntoMarkup(ClassCompileMarkupName, CompileClass);
                            Write(_sb.ToString());
                            _sb.Clear();
                        }
                        break;

                    default:
                        break;
                }
               
            }
        }

        public void CompileClass()
        {
            AppendWithOffset(CurrentKeyWordMarkUp);

            _tokenizer.Next();
            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Identifier)
                throw new Exception($"Expected class name, but got {tokenType}, {_tokenizer.GetIdentifier}");

            AppendWithOffset(CurrentClassName);

            _tokenizer.Next();
            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol || _tokenizer.CurrentToken != LexicalTables.OpenBracket)
                throw new Exception($"Expected class open brackets, but got {tokenType}, {_tokenizer.CurrentToken}");

            AppendWithOffset(CurrentSymbolMarkUp);

            var subroutineDeclarationExists = false;
            _tokenizer.Next();

            while(_tokenizer.GetTokenType() != TokenType.Symbol && CurrentToken != LexicalTables.CloseBracket)
            {
                if (_tokenizer.GetTokenType() != TokenType.KeyWord)
                    throw new Exception($"Expected keywords in class declaration, but got: {_tokenizer.GetTokenType()} ");

                var keyWordTYpe = _tokenizer.GetKeyWordType();
                if (JackTokenizerUtil.IsVariableDeclarationKeyWord(keyWordTYpe))
                {
                    if (subroutineDeclarationExists)
                        throw new Exception("Subroutine declaration already exists but classVariables declaration appeared");

                    WrapIntoMarkup(ClassVarDecMarkupName, CompileClassVarDec);
                }
                else if (JackTokenizerUtil.IsProcedureDeclarationKeyword(keyWordTYpe))
                {
                    WrapIntoMarkup(SubroutineDecMarkupName, CompileSubroutine);
                }

                _tokenizer.Next();
            }

            AppendWithOffset(CurrentSymbolMarkUp);
        }

        public void CompileClassVarDec()
        {
            AppendWithOffset(CurrentKeyWordMarkUp);

            _tokenizer.Next();

            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Identifier && tokenType != TokenType.KeyWord)
                throw new Exception($"Expected keyword or identifier as type of variables, but got: {_tokenizer.CurrentToken}");

            if (tokenType == TokenType.KeyWord && !JackTokenizerUtil.IsValidVariableKeyWord(_tokenizer.GetKeyWordType()))
                throw new Exception($"Ecpected var type as int, char or boolean, but {_tokenizer.GetKeyWordType()}");

            AppendWithOffset(GetTypeMarkUp(_tokenizer.CurrentToken));

            _tokenizer.Next();
            while(CurrentToken != LexicalTables.ComaAndDot)
            {
                tokenType = _tokenizer.GetTokenType();
                if (tokenType == TokenType.Symbol && CurrentToken == LexicalTables.Coma)
                    AppendWithOffset(CurrentSymbolMarkUp);
                else if (tokenType == TokenType.Identifier)
                    AppendWithOffset(CurrentVarNameMarkUp);
                else throw new Exception($"Expected coma or variableName but got: {tokenType}");

                _tokenizer.Next();
            }

            AppendWithOffset(CurrentSymbolMarkUp);
        }

        public void CompileSubroutine()
        {
            AppendWithOffset(CurrentKeyWordMarkUp);
            _tokenizer.Next();
            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.KeyWord && tokenType != TokenType.Identifier)
                throw new Exception("Expected keyword or iendifier");
            if (tokenType == TokenType.KeyWord && !JackTokenizerUtil.IsValidProcedureTypeKeyWorkd(_tokenizer.GetKeyWordType()))
                throw new Exception($"Expected var type as void, but {_tokenizer.GetKeyWordType()}");
            AppendWithOffset(GetTypeMarkUp(_tokenizer.CurrentToken));

            _tokenizer.Next();
            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Identifier)
                throw new Exception("Expected identifier as name of function");
            AppendWithOffset(CurrentSubroutineName);

            WrapIntoMarkup(ParameterListMarkupName, CompileParameterList);

            _tokenizer.Next();
            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol || CurrentToken != LexicalTables.OpenBracket)
                throw new Exception("Expected open bracket { of procedure ");

            WrapIntoMarkup(SubroutineBodyMarkupName, CompileSubroutineBody);
        }

        private void CompileSubroutineBody()
        {
            AppendWithOffset(CurrentSymbolMarkUp);

            while (NextToken == "var")
                WrapIntoMarkup(VarDecMarkupName, CompileVarDec);

            WrapIntoMarkup(StatementsMarkupName,CompileStatements);
        }

        public void CompileVarDec()
        {
            _tokenizer.Next();
            AppendWithOffset(CurrentKeyWordMarkUp);
            _tokenizer.Next();

            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.KeyWord && tokenType != TokenType.Identifier)
                throw new Exception("Expected keyword or iendifier");
            if (tokenType == TokenType.KeyWord && !JackTokenizerUtil.IsValidVariableKeyWord(_tokenizer.GetKeyWordType()))
                throw new Exception($"Expected variableKeyWord but got: {_tokenizer.GetKeyWordType()}");
            AppendWithOffset(GetTypeMarkUp(_tokenizer.CurrentToken));

            _tokenizer.Next();
            while (CurrentToken != ";")
            {
                tokenType = _tokenizer.GetTokenType();
                if (tokenType == TokenType.Symbol && CurrentToken == LexicalTables.Coma)
                    AppendWithOffset(CurrentSymbolMarkUp);
                else if (tokenType == TokenType.Identifier)
                    AppendWithOffset(CurrentVarNameMarkUp);
                else throw new Exception($"Expected coma or variableName but got: {tokenType}");

                _tokenizer.Next();
            }
        }

        public void CompileParameterList()
        {
            _tokenizer.Next();
            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol && CurrentToken != LexicalTables.OpenParenthesis)
                throw new Exception("Expected open parameters list brackets");

            AppendWithOffset(CurrentSymbolMarkUp);
            if(NextToken != LexicalTables.CloseParenthesis)
            {
                while (NextToken != LexicalTables.CloseParenthesis)
                {
                    CompileParameter();
                    if(NextToken == LexicalTables.Coma)
                    {
                        _tokenizer.Next();
                        AppendWithOffset(CurrentSymbolMarkUp);
                    }
                }
                _tokenizer.Next();
            }
            else
            {
                _tokenizer.Next();
            }
            
                
            AppendWithOffset(CurrentSymbolMarkUp);
        }

        public void CompileParameter()
        {
            _tokenizer.Next();
            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Identifier && tokenType != TokenType.KeyWord)
                throw new Exception($"Expected keyword or identifier as type of variables, but got: {_tokenizer.CurrentToken}");

            if (tokenType == TokenType.KeyWord && !JackTokenizerUtil.IsValidVariableKeyWord(_tokenizer.GetKeyWordType()))
                throw new Exception($"Ecpected var type as int, char or boolean, but {_tokenizer.GetKeyWordType()}");

            AppendWithOffset(GetTypeMarkUp(_tokenizer.CurrentToken));

            _tokenizer.Next();
            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Identifier)
                throw new Exception($"Expected var name but got {tokenType}");

            AppendWithOffset(CurrentVarNameMarkUp);
        }

        public void CompileExpression()
        {
            WrapIntoMarkup(TermCompileMarkupName, CompileTerm);

            while(LexicalTables.OperatorsTableString.Contains(NextToken))
            {
                AppendWithOffset(CurrentOperatorMarkUp);
                WrapIntoMarkup(TermCompileMarkupName, CompileTerm);
            }
        }

        public void CompileExpressionList()
        {
            _tokenizer.Next();
            if (CurrentToken != LexicalTables.OpenParenthesis)
                throw new Exception($"Expected {LexicalTables.OpenParenthesis} in expressionList");

            AppendWithOffset(CurrentSymbolMarkUp);

            while(NextToken != LexicalTables.CloseParenthesis)
            {
                WrapIntoMarkup(ExpressionMarkupName, CompileExpression);
                if(NextToken == LexicalTables.Coma)
                {
                    _tokenizer.Next();
                    AppendWithOffset(CurrentSymbolMarkUp);
                }
            }

            _tokenizer.Next();
            AppendWithOffset(CurrentSymbolMarkUp);
        }

        public void CompileLet()
        {
            AppendWithOffset(CurrentKeyWordMarkUp);

            _tokenizer.Next();
            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Identifier)
                throw new Exception($"Expected identifier as variable name, but got: {tokenType}");
            AppendWithOffset(CurrentVarNameMarkUp);

            _tokenizer.Next();
            tokenType = _tokenizer.GetTokenType();
            if(tokenType != TokenType.Symbol || CurrentToken != LexicalTables.Equal)
            {
                if (CurrentToken != LexicalTables.OpenSquareBracket)
                    throw new Exception($"Expected {LexicalTables.OpenSquareBracket}");
                AppendWithOffset(CurrentSymbolMarkUp);
                WrapIntoMarkup(ExpressionMarkupName, CompileExpression);
                _tokenizer.Next();
                if (CurrentToken != LexicalTables.CloseSquareBracket)
                    throw new Exception($"Expected {LexicalTables.CloseSquareBracket}");
                AppendWithOffset(CurrentSymbolMarkUp);
                _tokenizer.Next();
            }
            
            if (CurrentToken != LexicalTables.Equal)
                throw new Exception($"Expected {LexicalTables.Equal} in let statement");

            AppendWithOffset(CurrentSymbolMarkUp);

            WrapIntoMarkup(ExpressionMarkupName, CompileExpression);
            _tokenizer.Next();
            AppendWithOffset(CurrentSymbolMarkUp);
        }

        public void CompileIf()
        {
            AppendWithOffset(CurrentKeyWordMarkUp);

            _tokenizer.Next();
            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol && CurrentToken != LexicalTables.OpenParenthesis)
                throw new Exception($"Expected open parenthesis for if expression, but {tokenType}, {CurrentToken}");

            AppendWithOffset(CurrentSymbolMarkUp);

            WrapIntoMarkup(ExpressionMarkupName, CompileExpression);
            _tokenizer.Next();

            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol && CurrentToken != LexicalTables.CloseParenthesis)
                throw new Exception($"Expected close parenthesis for if expression, but {tokenType}, {CurrentToken}");

            AppendWithOffset(CurrentSymbolMarkUp);

            _tokenizer.Next();

            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol && CurrentToken != LexicalTables.OpenBracket)
                throw new Exception($"Expected open bracket for if statements, but {tokenType}, {CurrentToken}");

            AppendWithOffset(CurrentSymbolMarkUp);

            WrapIntoMarkup(StatementsMarkupName, CompileStatements);

            _tokenizer.Next();

            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol && CurrentToken != LexicalTables.CloseBracket)
                throw new Exception($"Expected close bracket for if statements, but {tokenType}, {CurrentToken}");

            AppendWithOffset(CurrentSymbolMarkUp);

            if(_tokenizer.GetNextTokenType() != TokenType.KeyWord || _tokenizer.GetNextKeywordType() != KeyWordType.Else)
                return;

            _tokenizer.Next();

            AppendWithOffset(CurrentKeyWordMarkUp);

            _tokenizer.Next();

            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol && CurrentToken != LexicalTables.OpenBracket)
                throw new Exception($"Expected open bracket for else statements, but {tokenType}, {CurrentToken}");

            AppendWithOffset(CurrentSymbolMarkUp);

            WrapIntoMarkup(StatementsMarkupName, CompileStatements);

            _tokenizer.Next();

            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol && CurrentToken != LexicalTables.CloseBracket)
                throw new Exception($"Expected close bracket for else statements, but {tokenType}, {CurrentToken}");

            AppendWithOffset(CurrentSymbolMarkUp);
        }

        public void CompileWhile()
        {
            AppendWithOffset(CurrentKeyWordMarkUp);

            _tokenizer.Next();
            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol && CurrentToken != LexicalTables.OpenParenthesis)
                throw new Exception($"Expected open parenthesis for while expression, but {tokenType}, {CurrentToken}");

            AppendWithOffset(CurrentSymbolMarkUp);

            WrapIntoMarkup(ExpressionMarkupName, CompileExpression);
            _tokenizer.Next();

            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol && CurrentToken != LexicalTables.CloseParenthesis)
                throw new Exception($"Expected close parenthesis for while expression, but {tokenType}, {CurrentToken}");

            AppendWithOffset(CurrentSymbolMarkUp);

            _tokenizer.Next();

            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol && CurrentToken != LexicalTables.OpenBracket)
                throw new Exception($"Expected open bracket for while statements, but {tokenType}, {CurrentToken}");

            AppendWithOffset(CurrentSymbolMarkUp);

            WrapIntoMarkup(StatementsMarkupName, CompileStatements);

            _tokenizer.Next();

            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol && CurrentToken != LexicalTables.CloseBracket)
                throw new Exception($"Expected close bracket for while statements, but {tokenType}, {CurrentToken}");

            AppendWithOffset(CurrentSymbolMarkUp);
        }

        public void CompileDo()
        {
            AppendWithOffset(CurrentKeyWordMarkUp);

            _tokenizer.Next(); // CompileSubroutineCall requires external Next call because of complicated Term parsing

            WrapIntoMarkup(SubroutineCallCompileMarkupName, CompileSubroutineCall);

            _tokenizer.Next();
            if(CurrentToken != LexicalTables.ComaAndDot)
                throw new Exception($"Expected ; for subroutine call , but {CurrentToken}");
        }

        public void CompileReturn()
        {
            AppendWithOffset(CurrentKeyWordMarkUp);

            if(NextToken != LexicalTables.ComaAndDot)
                WrapIntoMarkup(ExpressionMarkupName, CompileExpression);

            _tokenizer.Next();

            if (CurrentToken != LexicalTables.ComaAndDot)
                throw new Exception($"Expected ; for return but {CurrentToken}");

            AppendWithOffset(CurrentSymbolMarkUp);
        }

        public void CompileStatements()
        {
            while(NextToken != LexicalTables.CloseBracket)
                CompileStatement();
        }

        private void CompileStatement()
        {
            _tokenizer.Next();
            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.KeyWord)
                throw new Exception($"Statement compile expected keyword, type of statement, but get: {tokenType}");

            var keyWord = _tokenizer.GetKeyWordType();
            switch (keyWord)
            {
                case KeyWordType.Let:
                    WrapIntoMarkup(LetStatementMarkupName, CompileLet);
                    break;
                case KeyWordType.If:
                    WrapIntoMarkup(IfStatementMarkupName, CompileIf);
                    break;
                case KeyWordType.While:
                    WrapIntoMarkup(WhileStatementMarkupName, CompileWhile);
                    break;
                case KeyWordType.Do:
                    WrapIntoMarkup(DoStatementMarkupName, CompileDo);
                    break;
                case KeyWordType.Return:
                    WrapIntoMarkup(ReturnStatementMarkupName, CompileReturn);
                    break;

                default:
                    throw new Exception($"Not correct keyword for statementType : {keyWord}");
            }
        }

        public void CompileTerm()
        {
            _tokenizer.Next();

            if (LexicalTables.IsIntegerConstant(CurrentToken))
            {
                AppendWithOffset(CurrentIntegerConstant);
            }
            else if(LexicalTables.IsStringConstant(CurrentToken))
            {
                AppendWithOffset(CurrentStringConstant);
            }
            else if (LexicalTables.IsConstantKeyWord(CurrentToken))
            {
                AppendWithOffset(CurrentKeyWordConstant);
            }
            else if(_tokenizer.GetTokenType() == TokenType.Symbol 
                && LexicalTables.IsUnaryOperator(_tokenizer.GetSymbol()))
            {
                AppendWithOffset(CurrentUnaryOperatorMarkUp);
                WrapIntoMarkup(TermCompileMarkupName, CompileTerm);
            }
            else if(NextToken == LexicalTables.OpenSquareBracket)
            {
                AppendWithOffset(CurrentVarNameMarkUp);
                _tokenizer.Next();

                AppendWithOffset(CurrentSymbolMarkUp);
                WrapIntoMarkup(ExpressionMarkupName, CompileExpression);
                _tokenizer.Next();
                AppendWithOffset(CurrentSymbolMarkUp);
            }
            else if (CurrentToken == LexicalTables.OpenParenthesis)
            {
                AppendWithOffset(CurrentSymbolMarkUp);
                WrapIntoMarkup(ExpressionMarkupName, CompileExpression);
                _tokenizer.Next();
                AppendWithOffset(CurrentSymbolMarkUp);
            }
            else if(NextToken == LexicalTables.OpenParenthesis || NextToken == LexicalTables.Dot)
            {
                WrapIntoMarkup(SubroutineCallCompileMarkupName, CompileSubroutineCall);
            }
            else if(_tokenizer.GetTokenType() == TokenType.Identifier)
            {
                AppendWithOffset(CurrentVarNameMarkUp);
            }
        }

        public void CompileSubroutineCall()
        {
            switch (NextToken)
            {
                case ".":
                    AppendWithOffset(CurrentVarNameMarkUp); // todo: or class name
                    _tokenizer.Next();
                    AppendWithOffset(CurrentSymbolMarkUp);
                    _tokenizer.Next();
                    AppendWithOffset(CurrentSubroutineName);
                    WrapIntoMarkup(ExpressionListMarkupName, CompileExpressionList);
                    break;
                case "(":
                    AppendWithOffset(CurrentSubroutineName);
                    WrapIntoMarkup(ExpressionListMarkupName, CompileExpressionList);
                    break;
                default:
                    throw new Exception("Expected . or ( in subroutine call");

            }
        }

        private void WrapIntoMarkup(string markupElementName, Action toWrap)
        {
            if (toWrap == null)
                throw new Exception("Wrapping function is null");

            AppendWithOffset($"<{markupElementName}>\n");
            _currentOffsetTabs++;
            toWrap.Invoke();
            _currentOffsetTabs--;
            AppendWithOffset($"</{markupElementName}>\n");
        }

      
        private void AppendWithOffset(string markUp)
        {
            _sb.Append(Offset);
            _sb.Append(markUp);
        }

        private string CurrentSubroutineName => 
            $"<subroutineName> {CurrentToken} </subroutineName>\n";

        private string CurrentClassName =>
            $"<className> {CurrentToken} </className>\n";
        private string GetTypeMarkUp(string value) =>
            $"<type> {value} </type>\n";

        private string CurrentOperatorMarkUp =>
            $"<op> {_tokenizer.GetSymbol()} </op>\n";

        private string CurrentUnaryOperatorMarkUp =>
            $"<unaryOp> {_tokenizer.GetSymbol()} </unaryOp>\n";

        private string CurrentIntegerConstant =>
            $"<integerConstant> {CurrentToken} </integerConstant>\n";

        private string CurrentStringConstant =>
            $"<StringConstant> {CurrentToken} </StringConstant\n>";

        private string CurrentKeyWordConstant => 
            $"<KeywordConstant> {CurrentToken} </KeywordConstant\n>";

        private string CurrentKeyWordMarkUp =>
            $"<keyword> {CurrentToken} </keyword>\n";

        private string CurrentVarNameMarkUp =>
            $"<varName> {CurrentToken} </varName>\n";

        private string CurrentIdentifierMarkUp =>
            $"<identifier {CurrentToken} </identifier>\n";

        private string CurrentSymbolMarkUp =>
            $"<symbol> {CurrentToken} </symbol>\n";

        private void Write(string text)
        {
            // Create a FileStream and use StreamWriter to write text to the file
            using (FileStream fileStream = new FileStream(_outputFilePath, FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(fileStream))
                writer.Write(text);
        }
    }
}