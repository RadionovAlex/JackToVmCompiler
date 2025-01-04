using JackToVmCompiler.Jack;
using JackToVmCompiler.Tokenizer;
using JackToVmCompiler.VMWriter;
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

        private int _currentSubroutineLocalsNumber;
        private int _currentSubroutineIfNumber;
        private int _currentSubroutineWhileNumber;

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
            _outputFilePath = $"{Path.Combine(pathDirectory, fileName)}.vm";
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

            _vmWriter.Close();
        }

        public void CompileClass()
        {
            _tokenizer.Next();

            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Identifier)
                throw new Exception($"Expected class name, but got {tokenType}, {_tokenizer.GetIdentifier}");

            _symbolTable.HandleNewClass(CurrentToken);

            _tokenizer.Next();

            tokenType = _tokenizer.GetTokenType();
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
                if (tokenType == TokenType.Symbol && CurrentToken == LexicalTables.Coma){}

                else if (tokenType == TokenType.Identifier)
                    variablesOfType.Add(CurrentToken);
                else
                    throw new Exception($"Expected coma or variableName but got: {tokenType}");

                _tokenizer.Next();
            }

            foreach(var variableName in variablesOfType)
                _symbolTable.Define(variableName, typeName, symbolKind);
        }

        public void CompileDo()
        {
            _tokenizer.Next();

            CompileSubroutineCall();

            _tokenizer.Next();

            if (CurrentToken != LexicalTables.ComaAndDot)
                throw new Exception("Expected coma and dot");

            _vmWriter.WritePop(SegmentKind.Temp, 0);
        }

        public void CompileExpression()
        {
            CompileTerm();

            while (LexicalTables.OperatorsTableString.Contains(NextToken))
            {
                _tokenizer.Next();
                var operation = CommandKindExtensions.JackCommandToVmCommandMap[CurrentToken];
                if (CommandKindExtensions.JackCommandToVmCommandMap.TryGetValue(NextToken, out var secondCompareOperator))
                {
                    _tokenizer.Next();
                    operation |= secondCompareOperator;
                }
                    
                CompileTerm();
                _vmWriter.WriteArithmetic(operation);
            }
        }

        public int CompileExpressionList()
        {
            int argumentsCount = 0;
            _tokenizer.Next();
            if (CurrentToken != LexicalTables.OpenParenthesis)
                throw new Exception($"Expected {LexicalTables.OpenParenthesis} in expressionList");

            while (NextToken != LexicalTables.CloseParenthesis)
            {
                CompileExpression();
                argumentsCount++;
                if (NextToken == LexicalTables.Coma)
                    _tokenizer.Next();
            }

            _tokenizer.Next();

            return argumentsCount;
        }

        public void CompileParameter()
        {
            _tokenizer.Next();
            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Identifier && tokenType != TokenType.KeyWord)
                throw new Exception($"Expected keyword or identifier as type of variables, but got: {_tokenizer.CurrentToken}");

            if (tokenType == TokenType.KeyWord && !JackTokenizerUtil.IsValidVariableKeyWord(_tokenizer.GetKeyWordType()))
                throw new Exception($"Expected var type as int, char or boolean, but {_tokenizer.GetKeyWordType()}");

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

        public void CompileIf()
        {
            _currentSubroutineIfNumber++;
            var label = $"{_symbolTable.CurrentRoutineName}IF{_currentSubroutineIfNumber}";
            var ifFinishedLabel = $"{_symbolTable.CurrentRoutineName}IFFinished{_currentSubroutineIfNumber}";

            _tokenizer.Next(); // skip if word

            if (CurrentToken != LexicalTables.OpenParenthesis)
                throw new Exception("Expected open parenthessis");

            CompileExpression();
                
            _vmWriter.WriteArithmetic(CommandKind.Not);

            _vmWriter.WriteIfGoTo(label); // - go to the end of the statements

            _tokenizer.Next();

            if (CurrentToken != LexicalTables.CloseParenthesis)
                throw new Exception("Expected close parenthessis");

            _tokenizer.Next();
            if (CurrentToken != LexicalTables.OpenBracket)
                throw new Exception("Expected open bracket");

            CompileStatements();

            _tokenizer.Next();

            if (CurrentToken != LexicalTables.CloseBracket)
                throw new Exception("Expected close bracket");

            _vmWriter.WriteGoTo(ifFinishedLabel);

            _vmWriter.WriteLabel(label);

            if (_tokenizer.GetNextTokenType() != TokenType.KeyWord || 
                _tokenizer.GetNextKeywordType() != KeyWordType.Else)
                return;

            _tokenizer.Next(); // go to else
            _tokenizer.Next();
            if (CurrentToken != LexicalTables.OpenBracket)
                throw new Exception("Expected open bracket");

            CompileStatements();

            _tokenizer.Next();

            if (CurrentToken != LexicalTables.CloseBracket)
                throw new Exception("Expected close bracket");

            _vmWriter.WriteLabel(ifFinishedLabel);

        }

        // logic is next: program remembers all the information
        // about variable/field/statc/argument that will be modified
        // and only after all expression computation called
        // ( all the requrired values are pushed, computed ) 
        // pop "info" should be called;
        public void CompileLet()
        {
            _tokenizer.Next();
            var tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Identifier)
                throw new Exception($"Expected identifier as variable name, but got: {tokenType}");

            var variableName = CurrentToken;
            var isArrayCellChange = false;

            var entry = _symbolTable.GetEntry(variableName);

            _tokenizer.Next();
            tokenType = _tokenizer.GetTokenType();
           
            if (tokenType != TokenType.Symbol || CurrentToken != LexicalTables.Equal)
            {
                if (CurrentToken != LexicalTables.OpenSquareBracket)
                    throw new Exception($"Expected {LexicalTables.OpenSquareBracket}");

                // in case it is array`s cell, information about address should be wrote into temp1;
                isArrayCellChange = true; 
                CompileExpression();

                // information about cell should be wrote in the Temp1
                // push {get info from variable name}
                // add ( add with compile expression result, which is index ) 
                // pop temp 1
                _vmWriter.WritePush(entry.Kind.ToSegmentKind(), entry.Index);
                _vmWriter.WriteArithmetic(VMWriter.CommandKind.Add);
                _vmWriter.WritePop(VMWriter.SegmentKind.Temp, 1);
                
                _tokenizer.Next();
                if (CurrentToken != LexicalTables.CloseSquareBracket)
                    throw new Exception($"Expected {LexicalTables.CloseSquareBracket}");
                _tokenizer.Next();
            }

            if (CurrentToken != LexicalTables.Equal)
                throw new Exception($"Expected {LexicalTables.Equal} in let statement");

            CompileExpression();

            // there is, if it is an isArrayCellChange, temp1 should be read and next ->
            // pop point1 to have correct that 
            if (isArrayCellChange)
            {
                _vmWriter.WritePush(SegmentKind.Temp, 1);
                _vmWriter.WritePop(SegmentKind.Pointer, 1);
                _vmWriter.WritePop(SegmentKind.That, 0);
            }
            else
            {
                _vmWriter.WritePop(entry.Kind.ToSegmentKind(), entry.Index);
            }

            _tokenizer.Next();
        }

        public void CompileWhile()
        {
            _tokenizer.Next();
            _currentSubroutineWhileNumber++;
            var labelAfterWhile = $"{_symbolTable.CurrentRoutineName}NOTWHILE{_currentSubroutineWhileNumber}";
            var labelWhile= $"{_symbolTable.CurrentRoutineName}WHILE{_currentSubroutineWhileNumber}";

            if (CurrentToken != LexicalTables.OpenParenthesis)
                throw new Exception("Expected open parenthesis");

            _vmWriter.WriteLabel(labelWhile);
            CompileExpression();

            _tokenizer.Next();
            if (CurrentToken != LexicalTables.CloseParenthesis)
                throw new Exception("Expected close parenthesis");

            _vmWriter.WriteArithmetic(CommandKind.Not);

            _vmWriter.WriteIfGoTo(labelAfterWhile);

            _tokenizer.Next();
            if (CurrentToken != LexicalTables.OpenBracket)
                throw new Exception("Expected open bracket");

            CompileStatements();

            _vmWriter.WriteGoTo(labelWhile);

            _tokenizer.Next();

            if (CurrentToken != LexicalTables.CloseBracket)
                throw new Exception("Expected close bracket");

            _vmWriter.WriteLabel(labelAfterWhile);
        }

        public void CompileReturn()
        {
            if(NextToken != LexicalTables.ComaAndDot)
                CompileExpression();
            else
                _vmWriter.WritePush(SegmentKind.Const, 0);

            _vmWriter.WriteReturn();

            _tokenizer.Next();
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

            _tokenizer.Next();
            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Identifier)
                throw new Exception("Expected identifier as name of function");

            var subroutineName = CurrentToken;
            _symbolTable.HandleNewRoutine(subroutineName);
            var funcLabel = VmTranslationUtil.MethodNameLabel
                (_symbolTable.CurrentClass, _symbolTable.CurrentRoutineName);

            // paste funcName without NLocals
            _vmWriter.WriteFunctionWithNumberTemplate(funcLabel); 

            _currentSubroutineLocalsNumber = 0;
            _currentSubroutineIfNumber = 0;
            _currentSubroutineWhileNumber = 0;

            CompileParameterList();
            _tokenizer.Next();
            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol || CurrentToken != LexicalTables.OpenBracket)
                throw new Exception("Expected open bracket { of procedure ");

            CompileSubroutineBody();

            // replace NLocals for funcName 
            _sb.Replace(VMWriter.VMWriter.TemplateName(funcLabel), _currentSubroutineLocalsNumber.ToString());

            _tokenizer.Next();

            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol || CurrentToken != LexicalTables.CloseBracket)
                throw new Exception("Expected ckise bracket } of procedure ");
        }

        private void CompileSubroutineBody()
        {
            while (NextToken == "var")
                CompileVarDec();

            CompileStatements();
        }

        public void CompileSubroutineCall()
        {
            string typeName;
            string routineFullNameToCall;
            int argumentsCount;
            switch (NextToken)
            {
                case ".":
                    var entry = _symbolTable.GetEntrySafe(CurrentToken);
                    var tokenForEntry = CurrentToken;

                    _tokenizer.Next(); // go to dot (.)
                    _tokenizer.Next(); // go to func name

                    typeName = entry?.Type != null ? entry.Type : tokenForEntry;
                    routineFullNameToCall = VmTranslationUtil.MethodNameLabel(typeName, CurrentToken);

                    // in case it`s not Func call, but Method call ( entry is not null ) ,
                    // entry`s pointer should be put onto stack first
                    if(entry != null)
                        _vmWriter.WritePush(entry.Kind.ToSegmentKind(), entry.Index);

                    argumentsCount = CompileExpressionList(); // compile all arguments and put them onto the stack
                    if (entry != null)
                        argumentsCount++;

                    // call routineFullNameToCall with nArguments = CompileExpressionList amount ( +1 in case entry isn`t null )
                    _vmWriter.WriteCall(routineFullNameToCall, argumentsCount);

                    break;

                case "(":
                    typeName = _symbolTable.CurrentClass;
                    routineFullNameToCall = VmTranslationUtil.MethodNameLabel(typeName, CurrentToken);
                    _vmWriter.WritePush(SegmentKind.This, 0);

                    argumentsCount = CompileExpressionList() + 1; // +1 because we call local instance method

                    _vmWriter.WriteCall(routineFullNameToCall, argumentsCount);
                    
                    break;
                default:
                    throw new Exception("Expected . or ( in subroutine call");

            }
        }

        public void CompileTerm()
        {
            _tokenizer.Next();

            if (LexicalTables.IsIntegerConstant(CurrentToken))
            {
                _vmWriter.WritePush(SegmentKind.Const, _tokenizer.GetIntConst());
            }
            else if (LexicalTables.IsStringConstant(CurrentToken))
            {
                var stringValue = LexicalTables.StringConstantRegex.Match(CurrentToken).Value;
                _vmWriter.WritePush(SegmentKind.Const, stringValue.Length - 2); // -2 because of ""
                _vmWriter.WriteCall("String.new", 1);
                _vmWriter.WritePop(SegmentKind.Pointer, 0);
                var bytes = Encoding.ASCII.GetBytes(stringValue);
                for (var i = 1; i < bytes.Length -1 ; i++) // start and end is " and " 
                {
                    _vmWriter.WritePush(SegmentKind.Pointer, 0);
                    _vmWriter.WritePush(SegmentKind.Const, bytes[i]);
                    _vmWriter.WriteCall("String.appendChar", 2);
                    if (i != bytes.Length - 1)
                        _vmWriter.WritePop(SegmentKind.Temp, 0);
                }
                _vmWriter.WritePush(SegmentKind.Pointer, 0);    
            }
            else if (LexicalTables.IsConstantKeyWord(CurrentToken))
            {
                VmTranslationUtil.WritePushKeywordConstant(CurrentToken, _vmWriter);
            }
            else if (_tokenizer.GetTokenType() == TokenType.Symbol
                && LexicalTables.IsUnaryOperator(_tokenizer.GetSymbol()))
            {
                var unaryOperatorKind = LexicalTables.UnaryOperatorsMap[CurrentToken];
                CompileTerm();

                VmTranslationUtil.WriteUnaryOperator(unaryOperatorKind, _vmWriter);
            }
            else if (NextToken == LexicalTables.OpenSquareBracket)
            {
                var arrayEntry = _symbolTable.GetEntry(CurrentToken);
                _tokenizer.Next();
                CompileExpression();
                _tokenizer.Next();

                // there is compiler should read value from that and push it into stack
                // so before that, in compile expression, push should be made
                // and than, array entry`s address should be added 
                // and than pointer 1 ( that )  should point on this address 
                _vmWriter.WritePush(arrayEntry.Kind.ToSegmentKind(), arrayEntry.Index);
                _vmWriter.WriteArithmetic(CommandKind.Add);
                _vmWriter.WritePop(SegmentKind.Pointer, 1);
                _vmWriter.WritePush(SegmentKind.That, 0);
            }
            else if (CurrentToken == LexicalTables.OpenParenthesis)
            {
                CompileExpression();
                _tokenizer.Next();
            }
            else if (NextToken == LexicalTables.OpenParenthesis || NextToken == LexicalTables.Dot)
            {
                CompileSubroutineCall();
            }
            else if (_tokenizer.GetTokenType() == TokenType.Identifier)
            {
                var entry = _symbolTable.GetEntry(CurrentToken);
                _vmWriter.WritePush(entry.Kind.ToSegmentKind(), entry.Index);
            }
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
                if (tokenType == TokenType.Symbol && CurrentToken == LexicalTables.Coma) { }

                else if (tokenType == TokenType.Identifier)
                    variableNamesList.Add(CurrentToken);

                else
                    throw new Exception($"Expected coma or variableName but got: {tokenType}");

                _tokenizer.Next();
            }

            foreach (var variabeName in variableNamesList)
                _symbolTable.Define(variabeName, variablesType, SymbolKind.Var);

            _currentSubroutineLocalsNumber += variableNamesList.Count;
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