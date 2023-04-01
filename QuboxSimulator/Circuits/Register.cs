using QuantumLanguage;

namespace QuboxSimulator.Circuits;

public class Register
{
    public int QubitNumber { get; private set; } = 0;
    public int CbitNumber { get; private set; } = 0;
    
    public Dictionary<string, Tuple<int, int>> Qubits { get; set; } = new();
    
    public Dictionary<string, Tuple<int, int>> Cbits { get; set; } = new();
    
    public Dictionary<string, Tuple<AST.boolExpr, int>> BoolVariables { get; set; } = new();
    
    public Dictionary<string, Tuple<AST.arithExpr, int>> ArithVariables { get; set; } = new();

    public Register(AST.Memory memory)
    {
        QubitNumber = memory.CountQuantum;
        CbitNumber = memory.CountClassical;

        Qubits = new Dictionary<string, Tuple<int, int>>(memory.Quantum);
        Cbits = new Dictionary<string, Tuple<int, int>>(memory.Classical);

        BoolVariables = new Dictionary<string, Tuple<AST.boolExpr, int>>(memory.Boolean);
        ArithVariables = new Dictionary<string, Tuple<AST.arithExpr, int>>(memory.Arithmetic);
    }

    private static string StringifyDictionary<T>(Dictionary<string, T> dictionary)
    {
        return dictionary.Aggregate("", (current, pair) => current + $"{pair.Key} {pair.Value} \n");
    }
    
    public override string ToString()
    {
        return $"Qubits: {QubitNumber} Bits: {CbitNumber} \n" +
                $"{StringifyDictionary(Qubits)} {StringifyDictionary(Cbits)} \n" +
                $"{StringifyDictionary(BoolVariables)} {StringifyDictionary(ArithVariables)}";
    }
}