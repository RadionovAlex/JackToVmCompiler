namespace JackToVmCompiler.Jack
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

        private string _currentClassName;
        private string _currentRoutineName;

        public string CurrentClass => _currentClassName;
        public string CurrentRoutineName => _currentRoutineName;

        internal SymbolTable(string className)
        {
           _currentClassName = className;
        }

        internal void HandleNewClass(string className)
        {
            _currentClassName = className;
            _classSymbols.Clear();
        } 

        internal void HandleNewRoutine(string routineName)
        {
            _currentRoutineName = routineName;
            _routineSymbols.Clear();
        }
        
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

        internal SymbolKind KindOf(string name) => GetEntry(name).Kind;

        internal string TypeOf(string name) => GetEntry(name).Type;

        internal int IndexOf(string name) => GetEntry(name).Index;

        internal Entry GetEntry(string name)
        {
            if (_routineSymbols.TryGetValue(name, out var routineEntry))
                return routineEntry;
            if (_classSymbols.TryGetValue(name, out var classEntry))
                return classEntry;

            throw new Exception($"Cannot find {name} anyWhere in class {_currentClassName} and method {_currentRoutineName} ");
        }

        internal Entry GetEntrySafe(string name)
        {
            if (_routineSymbols.TryGetValue(name, out var routineEntry))
                return routineEntry;
            if (_classSymbols.TryGetValue(name, out var classEntry))
                return classEntry;

            return null;
        }
    }
}