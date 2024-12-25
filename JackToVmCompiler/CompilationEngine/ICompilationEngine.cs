namespace JackToVmCompiler.CompilationEngine
{
    public interface ICompilationEngine
    {
        void Compile();
        /// <summary>
        /// Compiles a complete class 
        /// </summary>
        void CompileClass();

        /// <summary>
        /// Compiles a static declaration of field declaration
        /// </summary>
        void CompileClassVarDec();

        /// <summary>
        /// Compiles a complete method, function or constructor
        /// </summary>
        void CompileSubroutine();

        void CompileSubroutineCall();

        /// <summary>
        /// Compiles a (possible empty) parameter list, not including the enclosing "()"
        /// </summary>
        void CompileParameterList();

        /// <summary>
        /// Compiles a parameter
        /// </summary>
        void CompileParameter();

        /// <summary>
        /// Compiles a var delaration
        /// </summary>
        void CompileVarDec();

        /// <summary>
        /// Compiles a sequence of statements, not including the enclosing "{}"
        /// </summary>
        void CompileStatements();

        /// <summary>
        /// Compiles a do statement
        /// </summary>
        void CompileDo();

        /// <summary>
        /// Compiles a let statement
        /// </summary>
        void CompileLet();

        /// <summary>
        /// Compiles a while statement
        /// </summary>
        void CompileWhile();

        /// <summary>
        /// Compiles a return statement
        /// </summary>
        void CompileReturn();

        /// <summary>
        /// Compiles an if statement, possibly with a trailing else clause
        /// </summary>
        void CompileIf();

        /// <summary>
        /// Compiles an expression
        /// </summary>
        void CompileExpression();

        /// <summary>
        /// Compiles a term. This routine is faced with a slight difficulty 
        /// when trying to decide between some of the alternative parsing rules. 
        /// Specifically,if the current token is an identifier, the routine must distinguish 
        /// between a variable, an erray entry, and a subroutine call. 
        /// A single look-ahead token, which may be one of "[", "(", or "." suffices to distinguish
        /// between three possibilities. Any other token is not part of this term should not be 
        /// advanced over
        /// </summary>
        void CompileTerm();

        /// <summary>
        /// Compiles a (possibly empty) comma-separated list of expressions
        /// </summary>
        int CompileExpressionList();
    }
}