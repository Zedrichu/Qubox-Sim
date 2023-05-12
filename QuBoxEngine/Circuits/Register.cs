using QuLangProcessor;

namespace QuBoxEngine.Circuits;

public class Register
{
    public int QubitNumber { get; } = 0;
    public int CbitNumber { get; } = 0;
    
    public Dictionary<string, Tuple<int, int>> Qubits { get; } = new();
    
    public Dictionary<string, Tuple<int, int>> Cbits { get; } = new();
    
    public Dictionary<string, Tuple<AST.BoolExpr, int>> BoolVariables { get; } = new();
    
    public Dictionary<string, Tuple<AST.ArithExpr, int>> ArithVariables { get; } = new();

    public Register(AST.Memory memory)
    {
        QubitNumber = memory.CountQuantum;
        CbitNumber = memory.CountClassical;

        Qubits = new Dictionary<string, Tuple<int, int>>(memory.Quantum);
        Cbits = new Dictionary<string, Tuple<int, int>>(memory.Classical);

        BoolVariables = new Dictionary<string, Tuple<AST.BoolExpr, int>>(memory.Boolean);
        ArithVariables = new Dictionary<string, Tuple<AST.ArithExpr, int>>(memory.Arithmetic);
    }

    private static string StringifyDictionary<T>(Dictionary<string, T> dictionary)
    {
        return dictionary.Aggregate("", (current, pair) => current + $"{pair.Key} {pair.Value} \n");
    }

    public string GetBitName(int index)
    {
        KeyValuePair<string, Tuple<int, int>> goal;
        if (index < QubitNumber)
        {
            goal = Qubits.FirstOrDefault(kvp =>
                index < kvp.Value.Item1 + kvp.Value.Item2);
        }
        else
        {
            index -= QubitNumber;
            goal = Cbits.FirstOrDefault(kvp =>
                index < kvp.Value.Item1 + kvp.Value.Item2);
        }

        if (goal.Value.Item1 == 1)
        {
            return goal.Key;
        }
        return $"{goal.Key}[{index - goal.Value.Item2}]";

    }
    
    public override string ToString()
    {
        return $"Qubits: {QubitNumber} Bits: {CbitNumber} \n" +
                $"{StringifyDictionary(Qubits)} {StringifyDictionary(Cbits)} \n" +
                $"{StringifyDictionary(BoolVariables)} {StringifyDictionary(ArithVariables)}";
    }
}