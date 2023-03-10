using QuantumLanguage;

namespace QuboxSimulator.Models;

public class Register
{
    public int QubitNumber { get; private set; } = 0;
    public int CbitNumber { get; private set; } = 0;
    
    public Dictionary<string, int> qubits { get; set; } = new();
    
    public Dictionary<string, int> cbits { get; set; } = new();
    
    public Dictionary<string, AST.boolExpr> boolVariables { get; set; } = new();
    
    public Dictionary<string, AST.arithExpr> arithVariables { get; set; } = new();

    public Register(int qubitNumber, int cbitNumber)
    {
        QubitNumber = qubitNumber;
        CbitNumber = cbitNumber;
    }
}