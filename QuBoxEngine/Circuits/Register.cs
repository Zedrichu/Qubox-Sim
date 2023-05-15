namespace QuBoxEngine.Circuits;
/* C#
 -*- coding: utf-8 -*-
Register

Description: Manages the memory context for which the circuit is defined.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 02/04/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/

using QuLangProcessor;

/// <summary>
/// Class that models the memory context for which the circuit is defined.
/// </summary>
public class Register
{
    public int QubitNumber { get; } = 0;
    public int CbitNumber { get; } = 0;
    
    public Dictionary<string, Tuple<int, int>> Qubits { get; } = new();
    
    public Dictionary<string, Tuple<int, int>> Cbits { get; } = new();
    
    public Dictionary<string, Tuple<AST.BoolExpr, int>> BoolVariables { get; } = new();
    
    public Dictionary<string, Tuple<AST.ArithExpr, int>> ArithVariables { get; } = new();

    /// <summary>
    /// Constructor for the register class given a memory type obtained during compilation.
    /// </summary>
    /// <param name="memory" cref="AST.Memory">Memory type from the QuLangProcessor package</param>
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

    /// <summary>
    /// Method to retrieve the name string for a given index of the bit.
    /// </summary>
    /// <param name="index">Index of bit the query is for</param>
    /// <returns>String name of the queried bit index</returns>
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
    
    /// <summary>
    /// Method to stringify the register class for debugging purposes.
    /// </summary>
    /// <returns>String representation of the register object</returns>
    public override string ToString()
    {
        return $"Qubits: {QubitNumber} Bits: {CbitNumber} \n" +
                $"{StringifyDictionary(Qubits)} {StringifyDictionary(Cbits)} \n" +
                $"{StringifyDictionary(BoolVariables)} {StringifyDictionary(ArithVariables)}";
    }
}