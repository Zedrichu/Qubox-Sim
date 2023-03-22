using QuantumLanguage;

namespace QuboxSimulator.Models;

public class Register
{
    public int QubitNumber { get; private set; } = 0;
    public int CbitNumber { get; private set; } = 0;
    
    public Dictionary<string, Tuple<int, int>> Qubits { get; set; } = new();
    
    public Dictionary<string, Tuple<int, int>> Cbits { get; set; } = new();
    
    public Dictionary<string, AST.boolExpr> BoolVariables { get; set; } = new();
    
    public Dictionary<string, AST.arithExpr> ArithVariables { get; set; } = new();

    public Register(AST.Memory memory)
    {
        QubitNumber = memory.CountQuantum;
        CbitNumber = memory.CountClassical;

        Qubits = new Dictionary<string, Tuple<int, int>>(memory.Quantum);
        Cbits = new Dictionary<string, Tuple<int, int>>(memory.Classical);

        BoolVariables = new Dictionary<string, AST.boolExpr>(memory.Boolean);
        ArithVariables = new Dictionary<string, AST.arithExpr>(memory.Arithmetic);
    }
}