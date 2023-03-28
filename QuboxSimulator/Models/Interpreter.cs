using QuantumLanguage;
using static QuantumLanguage.Handler;
using QuboxSimulator.Models.Circuits;

namespace QuboxSimulator.Models;

public static class Interpreter
{
    private static AST.Memory _memory = AST.Memory.empty;
    private static AST.@operator _operator = AST.@operator.NOP;
    public static AST.error Error { get; private set; } = AST.error.Success;
    
    public static void HandleLang(string? quLangCode)
    {
        var ast = parseQuLang(quLangCode);
        if (!ast.Item2.Equals(AST.error.Success))
        {
            Error = ast.Item2;
            return;
        }
        Console.WriteLine("QuLang Parsed | AST: " + ast.Item1);
        
        var sem = analyzeSemantics(ast.Item1.Item1, ast.Item1.Item2);
        if (!sem.Item2.Equals(AST.error.Success))
        {
            Error = sem.Item2;
            return;
        }
        _memory = sem.Item1;
        Console.WriteLine("Semantics Analyzed | Memory: " + _memory);
        
        var optimal = optimizeAST(ast.Item1.Item2, _memory);
        if (!optimal.Item3.Equals(AST.error.Success))
        {
            Error = optimal.Item3;
            return;
        }
        
        Error = AST.error.Success;
        _memory = optimal.Item2;
        _operator = optimal.Item1;
        
        Console.WriteLine("AST Optimized | AST: " + _operator);
        Console.Error.WriteLine(Error.ToString());
    }

    public static Circuit Interpret()
    {
        var register = new Register(_memory);
        var visitor = new OperatorVisitor(_memory);
        var gates = _operator.Accept(visitor);

        return CircuitFactory.BuildCircuit(gates, register);
    }
    
}