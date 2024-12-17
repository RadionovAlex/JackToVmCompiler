namespace JackToVmCompiler.SymbolTable
{
    internal class Entry
    {
        internal readonly string Name;
        internal readonly int Index;
        internal readonly string Type;
        internal readonly SymbolKind Kind;
        
        internal Entry(string name, int index, string type, SymbolKind kind)
        {
            Name = name;
            Index = index;
            Type = type;
            Kind = kind;
        }
    }

    internal class SymbolTable
    {
        private Dictionary<string, Entry> _classSymbols = new ();
        private Dictionary<string, Entry> _routineSymbols = new ();

        internal SymbolTable() { }

        internal void HandleNewClass() => _classSymbols.Clear();

        internal void HandleNewRoutine() => _routineSymbols.Clear();

        internal void Define(string name, string type, SymbolKind symbolKind)
        {
            var index = ScopeVarCount(symbolKind);
            var newEntry = new Entry(name, index, type, symbolKind);
            switch (symbolKind) 
            {
                case SymbolKind.Field:
                case SymbolKind.Static:
                    if (!_classSymbols.TryAdd(name, newEntry))
                        throw new Exception($"Class symbol {name} already exists");
                    break;

                case SymbolKind.Arg:
                case SymbolKind.Var:
                    if (!_routineSymbols.TryAdd(name, newEntry))
                        throw new Exception($"Class symbol {name} already exists");
                    break;
            }
        }

        internal int ScopeVarCount(SymbolKind symbolKind)
        {
            switch (symbolKind)
            {
                case SymbolKind.Field:
                case SymbolKind.Static:
                    return _classSymbols.Values.Count(x => x.Kind == symbolKind);

                case SymbolKind.Arg:
                case SymbolKind.Var:
                    return _routineSymbols.Values.Count(x => x.Kind == symbolKind);
                default:
                    throw new Exception($"Cannot handle symbol kind {symbolKind}");
            }
        }

        internal SymbolKind KindOf(string name)
        {
            return SymbolKind.Var;
        }

        internal string TypeOf(string name)
        {
            return string.Empty;
        }

        internal int IndexOf(string name)
        {
            return 0;
        }
    }
}
