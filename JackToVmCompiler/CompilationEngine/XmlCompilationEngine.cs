using JackToVmCompiler.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackToVmCompiler.CompilationEngine
{
    internal class XmlCompilationEngine : ICompilationEngine
    {
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

            AppendWithOffset(CurrentIdentifierMarkUp);

            _tokenizer.Next();
            tokenType = _tokenizer.GetTokenType();
            if (tokenType != TokenType.Symbol || _tokenizer.CurrentToken != "{")
                throw new Exception($"Expected class open brackets, but got {tokenType}, {_tokenizer.CurrentToken}");

            AppendWithOffset(CurrentSymbolMarkUp);

            var subroutineDeclarationExists = false;
            _tokenizer.Next();

            while(_tokenizer.GetTokenType() != TokenType.Symbol && _tokenizer.GetSymbol() != '}')
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
            while(CurrentToken != ";")
            {
                tokenType = _tokenizer.GetTokenType();
                if (tokenType == TokenType.Symbol && CurrentToken == ",")
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

        public void CompileParameterList()
        {
            throw new NotImplementedException();
        }

        public void CompileReturn()
        {
            throw new NotImplementedException();
        }

        public void CompileStatements()
        {
            throw new NotImplementedException();
        }

        public void CompileSubroutine()
        {
            throw new NotImplementedException();
        }

        public void CompileTerm()
        {
            throw new NotImplementedException();
        }

        public void CompileVarDec()
        {
            throw new NotImplementedException();
        }

        public void CompileWhile()
        {
            throw new NotImplementedException();
        }

        private void AppendWithOffset(string markUp)
        {
            _sb.Append(Offset);
            _sb.Append(markUp);
        }

        private string GetTypeMarkUp(string value) =>
            $"<type> {value} </type>\n";

        private string GetKeyWordMarkup(string keyword) => 
            $"<keyword> {keyword} </keyword>\n";

        private string CurrentKeyWordMarkUp =>
            $"<keyword> {_tokenizer.CurrentToken} </keyword>\n";

        private string CurrentVarNameMarkUp =>
            $"<varName> {_tokenizer.CurrentToken} </varName>\n";

        private string CurrentIdentifierMarkUp =>
            $"<identifier {_tokenizer.CurrentToken} </identifier>\n";

        private string CurrentSymbolMarkUp =>
            $"<symbol> {_tokenizer.CurrentToken} </symbol>\n";

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
