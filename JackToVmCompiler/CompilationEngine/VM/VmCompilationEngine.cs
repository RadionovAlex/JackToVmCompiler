using JackToVmCompiler.Jack;
using JackToVmCompiler.Tokenizer;
using System.Text;

namespace JackToVmCompiler.CompilationEngine.VM
{
    /// <summary>
    /// * implements ICompilaionEngine
    /// * writes/read to/from SymbolTable 
    /// </summary>
    internal class VmCompilationEngine : ICompilationEngine
    {
        private string _outputFilePath;
        private JackTokenizer _tokenizer;
        private StringBuilder _sb;
        private SymbolTable _symbolTable;
        private VMWriter.VMWriter _vmWriter;

        private string CurrentToken => _tokenizer.CurrentToken;

        private string NextToken => _tokenizer.NextToken;

        internal VmCompilationEngine(string sourcePath, JackTokenizer tokenizer)
        {
            _tokenizer = tokenizer;
            _sb = new StringBuilder(_tokenizer.Tokens.Count * 10);
            _vmWriter = new VMWriter.VMWriter(_sb);
            var pathDirectory = Path.GetDirectoryName(sourcePath);
            var fileName = Path.GetFileNameWithoutExtension(sourcePath);
            _symbolTable = new SymbolTable(fileName);
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
                        if (keywordType == KeyWordType.Class)
                        {
                            CompileClass();
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
            _tokenizer.Next();

            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Identifier)
                throw new Exception($"Expected class name, but got {tokenType}, {_tokenizer.GetIdentifier}");

            _symbolTable.HandleNewClass(CurrentToken);

            _tokenizer.Next();

            // expects {
            if (tokenType != TokenType.Symbol || _tokenizer.CurrentToken != LexicalTables.OpenBracket)
                throw new Exception($"Expected class open brackets, but got {tokenType}, {_tokenizer.CurrentToken}");

            _tokenizer.Next();

            while(_tokenizer.GetTokenType() != TokenType.Symbol && CurrentToken != LexicalTables.CloseBracket)
            {
                if (_tokenizer.GetTokenType() != TokenType.KeyWord)
                    throw new Exception($"Expected keywords in class declaration, but got: {_tokenizer.GetTokenType()} ");

                var keyWordType = _tokenizer.GetKeyWordType();

                if (JackTokenizerUtil.IsVariableDeclarationKeyWord(keyWordType))
                {
                    CompileClassVarDec();
                }
                else if (JackTokenizerUtil.IsProcedureDeclarationKeyword(keyWordType))
                {
                    CompileSubroutine();
                }

                _tokenizer.Next();
            }
        }

        public void CompileClassVarDec()
        {
            var declaredVariableType = _tokenizer.GetKeyWordType();
            var symbolKind = declaredVariableType == KeyWordType.Static ? SymbolKind.Static 
                : declaredVariableType == KeyWordType.Field ? SymbolKind.Field 
                : throw new Exception("Expected field or static as variable kind");

            _tokenizer.Next();

            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Identifier && tokenType != TokenType.KeyWord)
                throw new Exception($"Expected keyword or identifier as type of variables, but got: {_tokenizer.CurrentToken}");

            if (tokenType == TokenType.KeyWord && !JackTokenizerUtil.IsValidVariableKeyWord(_tokenizer.GetKeyWordType()))
                throw new Exception($"Ecpected var type as int, char or boolean, but {_tokenizer.GetKeyWordType()}");

            var typeName = CurrentToken;
            var variablesOfType = new List<string>();
            
            _tokenizer.Next();
            while (CurrentToken != LexicalTables.ComaAndDot)
            {
                tokenType = _tokenizer.GetTokenType();
                if (tokenType == TokenType.Symbol && CurrentToken == LexicalTables.Coma)
                {
                    // do noting
                }

                else if (tokenType == TokenType.Identifier)
                {
                    variablesOfType.Add(CurrentToken);
                }

                else
                {
                    throw new Exception($"Expected coma or variableName but got: {tokenType}");
                }

                _tokenizer.Next();
            }

            foreach(var variableName in variablesOfType)
                _symbolTable.Define(variableName, typeName, symbolKind);
        }

        public void CompileDo()
        {
            throw new NotImplementedException();
        }

        public void CompileExpression()
        {
            throw new NotImplementedException();
        }

        public void CompileExpressionList()
        {
            throw new NotImplementedException();
        }

        public void CompileIf()
        {
            throw new NotImplementedException();
        }

        public void CompileLet()
        {
            throw new NotImplementedException();
        }

        public void CompileParameter()
        {
            _tokenizer.Next();
            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Identifier && tokenType != TokenType.KeyWord)
                throw new Exception($"Expected keyword or identifier as type of variables, but got: {_tokenizer.CurrentToken}");

            if (tokenType == TokenType.KeyWord && !JackTokenizerUtil.IsValidVariableKeyWord(_tokenizer.GetKeyWordType()))
                throw new Exception($"Ecpected var type as int, char or boolean, but {_tokenizer.GetKeyWordType()}");

            var argumentType = CurrentToken;

            _tokenizer.Next();
            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Identifier)
                throw new Exception($"Expected var name but got {tokenType}");

            var argumentName = CurrentToken;
            _symbolTable.Define(argumentName, argumentType, SymbolKind.Arg);

        }

        public void CompileParameterList()
        {
            _tokenizer.Next();
            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol && CurrentToken != LexicalTables.OpenParenthesis)
                throw new Exception("Expected open parameters list brackets");

            if (NextToken != LexicalTables.CloseParenthesis)
            {
                while (NextToken != LexicalTables.CloseParenthesis)
                {
                    CompileParameter();
                    if (NextToken == LexicalTables.Coma)
                        _tokenizer.Next();
                }
                _tokenizer.Next();
            }
            else
            {
                _tokenizer.Next();
            }
        }

        public void CompileReturn()
        {
            throw new NotImplementedException();
        }

        public void CompileStatements()
        {
            while (NextToken != LexicalTables.CloseBracket)
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
                    CompileLet();
                    break;
                case KeyWordType.If:
                    CompileIf();
                    break;
                case KeyWordType.While:
                    CompileWhile();
                    break;
                case KeyWordType.Do:
                    CompileDo();
                    break;
                case KeyWordType.Return:
                    CompileReturn();
                    break;

                default:
                    throw new Exception($"Not correct keyword for statementType : {keyWord}");
            }
        }

        public void CompileSubroutine()
        {            
            _tokenizer.Next();
            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.KeyWord && tokenType != TokenType.Identifier)
                throw new Exception("Expected keyword or iendifier");
            if (tokenType == TokenType.KeyWord && !JackTokenizerUtil.IsValidProcedureTypeKeyWord(_tokenizer.GetKeyWordType()))
                throw new Exception($"Expected var type as void, but {_tokenizer.GetKeyWordType()}");

            var subroutineReturnType = CurrentToken; // it is required in case return type is void
                                                     // - I should return 0 ( and maybe pop into temp 0 )

            _tokenizer.Next();
            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Identifier)
                throw new Exception("Expected identifier as name of function");

            var subroutineName = CurrentToken;
            _symbolTable.HandleNewRoutine(subroutineName);
            _vmWriter.WriteLabel(_symbolTable.CurrentClass);

            CompileParameterList();
            _tokenizer.Next();
            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol || CurrentToken != LexicalTables.OpenBracket)
                throw new Exception("Expected open bracket { of procedure ");

            CompileSubroutineBody();
            // WrapIntoMarkup(SubroutineBodyMarkupName, CompileSubroutineBody);
        }

        private void CompileSubroutineBody()
        {
            // AppendWithOffset(CurrentSymbolMarkUp);

            while (NextToken == "var")
                CompileVarDec();

            CompileStatements();

            // WrapIntoMarkup(StatementsMarkupName, CompileStatements);
        }

        public void CompileSubroutineCall()
        {
            throw new NotImplementedException();
        }

        public void CompileTerm()
        {
            throw new NotImplementedException();
        }

        public void CompileVarDec()
        {
            _tokenizer.Next();
            if (!LexicalTables.IsVar(CurrentToken))
                throw new Exception("Expected var keyword");
            _tokenizer.Next();

            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.KeyWord && tokenType != TokenType.Identifier)
                throw new Exception("Expected keyword or iendifier");
            if (tokenType == TokenType.KeyWord && !JackTokenizerUtil.IsValidVariableKeyWord(_tokenizer.GetKeyWordType()))
                throw new Exception($"Expected variableKeyWord but got: {_tokenizer.GetKeyWordType()}");
            
            var variablesType = CurrentToken;
            var variableNamesList = new List<string>();

            _tokenizer.Next();
            while (CurrentToken != ";")
            {
                tokenType = _tokenizer.GetTokenType();
                if (tokenType == TokenType.Symbol && CurrentToken == LexicalTables.Coma)
                {
                    // do nothing
                }
                else if (tokenType == TokenType.Identifier)
                {
                    variableNamesList.Add(CurrentToken);
                }
                else
                {
                    throw new Exception($"Expected coma or variableName but got: {tokenType}");
                }

                _tokenizer.Next();
            }

            foreach (var variabeName in variableNamesList)
                _symbolTable.Define(variabeName, variablesType, SymbolKind.Var);
        }

        public void CompileWhile()
        {
            throw new NotImplementedException();
        }

        private void Write(string text)
        {
            // Create a FileStream and use StreamWriter to write text to the file
            using (FileStream fileStream = new FileStream(_outputFilePath, FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(fileStream))
                writer.Write(text);
        }
    }
}