using JackToVmCompiler.Tokenizer;
using System.Text;

namespace JackToVmCompiler.CompilationEngine
{
    internal class XmlCompilationEngine : ICompilationEngine
    {
        private const string SubroutineCallCompileMarkupName = "subroutineCall";

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
                            CompileClass();
                        }
                        break;

                    default:
                        break;
                }
               
            }
        }

        public void CompileClass()
        {
            AppendWithOffset("<class>\n");

            _currentOffsetTabs++;

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

                    CompileClassVarDec();
                }
                else if (JackTokenizerUtil.IsProcedureDeclarationKeyword(keyWordTYpe))
                {
                    CompileSubroutine();
                }

                _tokenizer.Next();
            }

            AppendWithOffset(CurrentSymbolMarkUp);

            _currentOffsetTabs--;
            AppendWithOffset("</class>");

            Write(_sb.ToString());
            _sb.Clear();
        }

        public void CompileClassVarDec()
        {
            AppendWithOffset("<classVarDec>\n");
            _currentOffsetTabs++;

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
            _currentOffsetTabs--;
            AppendWithOffset("</classVarDec>\n");
        }

        public void CompileSubroutine()
        {
            AppendWithOffset("<subroutineDec>\n");
            _currentOffsetTabs++;

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

            CompileParameterList();

            _tokenizer.Next();
            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol || CurrentToken != LexicalTables.OpenBracket)
                throw new Exception("Expected open bracket { of procedure ");

            CompileSubroutineBody();

            _currentOffsetTabs--;
            AppendWithOffset("</subroutineDec>\n");
        }

        private void CompileSubroutineBody()
        {
            AppendWithOffset("<subroutineBody>\n");
            _currentOffsetTabs++;
            AppendWithOffset(CurrentSymbolMarkUp);

            while (NextToken == "var")
            {
                CompileVarDec();
            }

            CompileStatements();


            _currentOffsetTabs--;
            AppendWithOffset("</subroutineBody>\n");
        }

        public void CompileVarDec()
        {
            AppendWithOffset("<varDec>\n");
            _currentOffsetTabs++;

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


            _currentOffsetTabs--;
            AppendWithOffset("</varDec>\n");
        }

        public void CompileParameterList()
        {
            AppendWithOffset("<parameterList>\n");
            _currentOffsetTabs++;
            _tokenizer.Next();
            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol && CurrentToken != LexicalTables.OpenParenthesis)
                throw new Exception("Expected open parameters list brackets");

            AppendWithOffset(CurrentSymbolMarkUp);
            if(NextToken != LexicalTables.CloseParenthesis)
            {
                while (CurrentToken != LexicalTables.CloseParenthesis)
                {
                    CompileParameter();
                    _tokenizer.Next();
                    if (_tokenizer.GetTokenType() == TokenType.Symbol && CurrentToken == LexicalTables.Coma)
                    {
                        AppendWithOffset(CurrentSymbolMarkUp);
                        _tokenizer.Next();
                    }
                }
            }
            else
            {
                _tokenizer.Next();
            }
            
                
            AppendWithOffset(CurrentSymbolMarkUp);

            _currentOffsetTabs--;
            AppendWithOffset("<parameterList>\n");
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
                throw new Exception($"Ecpected var name but got {tokenType}");

            AppendWithOffset(CurrentVarNameMarkUp);

        }

        public void CompileExpression()
        {
            AppendWithOffset("<expression>\n");
            _currentOffsetTabs++;

            CompileTerm();

            while(LexicalTables.OperatorsTableString.Contains(NextToken))
            {
                AppendWithOffset(CurrentOperatorMarkUp);
                CompileTerm();
            }

            _currentOffsetTabs--;
            AppendWithOffset("</expression>\n");
        }

        public void CompileExpressionList()
        {
            AppendWithOffset("<expressionList>\n");
            _currentOffsetTabs++;

            _tokenizer.Next();
            if (CurrentToken != LexicalTables.OpenParenthesis)
                throw new Exception($"Expected {LexicalTables.OpenParenthesis} in expressionList");

            AppendWithOffset(CurrentSymbolMarkUp);

            while(NextToken != LexicalTables.CloseParenthesis)
            {
                CompileExpression();
                if(NextToken == LexicalTables.Coma)
                {
                    _tokenizer.Next();
                    AppendWithOffset(CurrentSymbolMarkUp);
                }
            }

            _tokenizer.Next();
            AppendWithOffset(CurrentSymbolMarkUp);

            _currentOffsetTabs--;
            AppendWithOffset("</expressionList>\n");
        }

        public void CompileLet()
        {
            AppendWithOffset("<letStatement>\n");
            _currentOffsetTabs++;

            AppendWithOffset(CurrentKeyWordMarkUp);

            _tokenizer.Next();
            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Identifier)
                throw new Exception($"Expected identifier as variable name, but got: {tokenType}");
            AppendWithOffset(CurrentVarNameMarkUp);

            _tokenizer.Next();
            tokenType = _tokenizer.GetTokenType();
            if(tokenType != TokenType.Symbol && CurrentToken != LexicalTables.Equal)
            {
                if (CurrentToken != LexicalTables.OpenSquareBracket)
                    throw new Exception($"Expected {LexicalTables.OpenSquareBracket}");
                AppendWithOffset(CurrentSymbolMarkUp);
                CompileExpression();
                if (CurrentToken != LexicalTables.CloseSquareBracket)
                    throw new Exception($"Expected {LexicalTables.CloseSquareBracket}");
                AppendWithOffset(CurrentSymbolMarkUp);
                _tokenizer.Next();
            }
            
            if (CurrentToken != LexicalTables.Equal)
                throw new Exception($"Expected {LexicalTables.Equal} in let statement");

            AppendWithOffset(CurrentSymbolMarkUp);

            CompileExpression();

            _currentOffsetTabs--;
            AppendWithOffset("</letStatement>\n");
        }

        public void CompileIf()
        {
            throw new NotImplementedException();
        }

        public void CompileWhile()
        {
            throw new NotImplementedException();
        }

        public void CompileDo()
        {
            throw new NotImplementedException();
        }

        public void CompileReturn()
        {
            throw new NotImplementedException();
        }

        public void CompileStatements()
        {
            AppendWithOffset("<statements>\n");
            _currentOffsetTabs++;

            _tokenizer.Next();
            while(_tokenizer.GetTokenType() != TokenType.Symbol && CurrentToken != LexicalTables.CloseBracket)
            {
                CompileStatement();
                _tokenizer.Next();
            }

            AppendWithOffset(CurrentSymbolMarkUp);
            _currentOffsetTabs--;
            AppendWithOffset("</statements>\n");
        }

        private void CompileStatement()
        {
            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.KeyWord)
                throw new Exception($"Statement compile expected keyword, type of statement, but get: {tokenType}");

            var keyWord = _tokenizer.GetKeyWordType();
            switch (keyWord)
            {
                case KeyWordType.Let:
                    CompileLet();
                    break;
                case KeyWordType.If:
                    CompileIf();
                    break;
                case KeyWordType.While:
                    break;
                case KeyWordType.Do:
                    break;
                case KeyWordType.Return:
                    break;

                default:
                    throw new Exception($"Not correct keyword for statementType : {keyWord}");
            }
        }

        public void CompileTerm()
        {
            AppendWithOffset("<term>\n");
            _currentOffsetTabs++;

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
                CompileTerm();
            }
            else if(NextToken == LexicalTables.OpenSquareBracket)
            {
                AppendWithOffset(CurrentVarNameMarkUp);
                _tokenizer.Next();

                AppendWithOffset(CurrentSymbolMarkUp);
                CompileExpression();
                _tokenizer.Next();
                AppendWithOffset(CurrentSymbolMarkUp);
            }
            else if (CurrentToken == LexicalTables.OpenParenthesis)
            {
                AppendWithOffset(CurrentSymbolMarkUp);
                CompileExpression();
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

            _currentOffsetTabs--;
            AppendWithOffset("</term>\n");
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
                    CompileExpressionList();
                    break;
                case "(":
                    AppendWithOffset(CurrentSubroutineName);
                    CompileExpressionList();
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
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    writer.Write(text);
                }
            }
        }
    }
}
