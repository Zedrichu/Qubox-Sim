using QuantumLanguage;

namespace QuboxSimulator.Models;

public class Register
{
    private int QubitNumber { get; set; } = 0;
    private int CbitNumber { get; set; } = 0;
    
    public Dictionary<string, int> Qubits { get; set; } = new();
    
    public Dictionary<string, int> Cbits { get; set; } = new();
    
    public Dictionary<string, AST.boolExpr> BoolVariables { get; set; } = new();
    
    public Dictionary<string, AST.arithExpr> ArithVariables { get; set; } = new();

    public Register(AST.Memory memory)
    {
        QubitNumber = memory.countQuantum;
        CbitNumber = memory.countClassical;

        Qubits = new Dictionary<string, int>(memory.Quantum);
        Cbits = new Dictionary<string, int>(memory.Classical);

        BoolVariables = new Dictionary<string, AST.boolExpr>(memory.Boolean);
        ArithVariables = new Dictionary<string, AST.arithExpr>(memory.Arithmetic);
    }
}