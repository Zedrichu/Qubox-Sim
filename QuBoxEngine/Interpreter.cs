namespace QuBoxEngine;
/* C#
 -*- coding: utf-8 -*-
Interpreter

Description: Module implementing the interface interpretation of QuLang to circuits.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 18/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/

using static QuLangProcessor.Handler;
using Circuits;
using static QuLangProcessor.AST;

/// <summary>
/// Class that handles the interpretation of QuLang to circuits. Acts as a facade for the entire QuBoxEngine package.
/// </summary>
public static class Interpreter
{
    private static Memory _memory = Memory.empty;
    
    private static Tuple<Allocation, Schema>? _circuit;

    private static Circuit Circuit { get; set; }

    public static Error Error { get; private set; } = Error.Success;
    
    /// <summary>
    /// API method that performs the compilation of QuLang input to optimized AST structure.
    /// </summary>
    /// <param name="quLangCode">Input string in QuLang format</param>
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
        
        var optimal = optimizeAST(ast.Item1, ast.Item2, _memory);
        if (!optimal.Item3.Equals(Error.Success))
        {
            Error = optimal.Item3;
            return;
        }
        
        Error = Error.Success;
        _memory = optimal.Item2;
        _circuit = new Tuple<Allocation, Schema>(ast.Item1, optimal.Item1.Item2);
        
        Console.WriteLine("AST Optimized | AST: " + _circuit);
    }

    /// <summary>
    /// API method that interprets the compiled AST into a complete Circuit object.
    /// </summary>
    /// <returns cref="Circuit"></returns>
    public static Circuit? Interpret()
    {
        if (!Error.Equals(Error.Success)) return null;
        var register = new Register(_memory);
        var visitor = new StatementVisitor(_memory);
        var gates = _circuit.Item2.Item.Select(
                statement => statement.Accept(visitor)).ToList();
        gates.RemoveAll(gate => gate == null);
        Circuit = CircuitFactory.BuildCircuit(gates, register);
        return Circuit;
    }
    
    /// <summary>
    /// API method to deconstruct the circuit object into an AST structure.
    /// </summary>
    /// <param name="circuit" cref="Circuit">Target circuit to be decomposed</param>
    /// <returns cref="Tuple{Allocation, Schema}">AST structure formed out of an Allocation type and a Schema type</returns>
    public static Tuple<Allocation, Schema> DecomposeCircuit(Circuit circuit)
    {
        var generator = new Generator(circuit);
        _circuit = generator.DestructCircuit();
        return _circuit;
    }

    /// <summary>
    /// API method to handle the back-compilation of AST to QuLang format.
    /// </summary>
    /// <returns>String representation in QuLang format</returns>
    public static string BackCompileAst()
    {
        if (_circuit == null) return "";
        return backCompileCircuit(_circuit.Item1, _circuit.Item2);
    }

    /// <summary>
    /// API method to handle the translation of AST to Q# format.
    /// </summary>
    /// <returns>String representation in Q# format</returns>
    public static string TranslateQs ()
    {
        if (_circuit == null) return "";
        return translateCircuit(_circuit.Item1, _circuit.Item2);
    }

}