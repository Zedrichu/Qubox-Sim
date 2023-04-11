using QuantumLanguage;
using static QuantumLanguage.AST;
using static QuantumLanguage.Handler;
using QuboxSimulator.Circuits;

namespace QuboxSimulator;

public static class Interpreter
{
    private static Memory _memory = Memory.empty;
    
    private static Tuple<Allocation, Schema>? _circuit;

    public static Error Error { get; private set; } = Error.Success;
    
    public static void HandleLang(string? quLangCode)
    {
        var astOption = parseQuLang(quLangCode);
        if (!astOption.Item2.Equals(Error.Success))
        {
            Error = astOption.Item2;
            return;
        }

        var ast = astOption.Item1.Value;
        
        Console.WriteLine("QuLang Parsed | AST: " + ast);
        
        var sem = analyzeSemantics(ast.Item1, ast.Item2);
        if (!sem.Item2.Equals(Error.Success))
        {
            Error = sem.Item2;
            return;
        }
        _memory = sem.Item1;
        Console.WriteLine("Semantics Analyzed | Memory: " + _memory);
        
        var optimal = optimizeAST(ast.Item2, _memory);
        if (!optimal.Item3.Equals(Error.Success))
        {
            Error = optimal.Item3;
            return;
        }
        
        Error = Error.Success;
        _memory = optimal.Item2;
        _circuit = new Tuple<Allocation, Schema>(ast.Item1, optimal.Item1);
        
        Console.WriteLine("AST Optimized | AST: " + _circuit);
    }

    public static Circuit? Interpret()
    {
        if (!Error.Equals(Error.Success)) return null;
        var register = new Register(_memory);
        var visitor = new StatementVisitor(_memory);
        var gates = _circuit.Item2.Item.Select(
                statement => statement.Accept(visitor)).ToList();
        gates.RemoveAll(gate => gate == null);
        return CircuitFactory.BuildCircuit(gates, register);
    }
    
}