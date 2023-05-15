namespace QuBoxEngine.Circuits;
/* C#
 -*- coding: utf-8 -*-
Generator

Description: Module implementing the logic of decomposing the circuit into the AST structure from the QuLangProcessor package.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 01/04/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/

using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Gates;
using QuLangProcessor;
using static QuLangProcessor.AST;
using static QuLangProcessor.Tags;

/// <summary>
/// Class implementing the destructuring logic of the circuit into the AST structure.
/// </summary>
internal class Generator
{
    private Register Reg { get; set; } = new (Memory.empty);
    private List<Tower> Towers { get; set; } = new();
    /// <summary>
    /// Constructor of the generator object.
    /// </summary>
    /// <param name="circuit" cref="Circuit">Target circuit for destruction</param>
    internal Generator(Circuit circuit)
    {
        Reg = circuit.Allocation;
        Towers = circuit.GateGrid;
    }
    /// <summary>
    /// Method to decompose a bit into AST representation.
    /// </summary>
    /// <param name="triplet" cref="KeyValuePair">Information about the bit from the memory allocation</param>
    /// <returns cref="Bit">Bit typed structure in the AST</returns>
    private Bit FormBit(KeyValuePair<string, Tuple<int, int>> triplet)
    {
        var id = triplet.Key;
        var num = triplet.Value.Item1;
        return num == 1 ? Bit.NewBitS(id) : 
            Bit.NewBitA(new Tuple<string, int>(id,num));
    }
    /// <summary>
    /// Method to perform the decomposition of an arithmetic assignment command.
    /// </summary>
    /// <param name="s">Variable on the LHS of the assignment</param>
    /// <param name="exp">Arithmetic expression AST to be assigned</param>
    /// <returns cref="Statement">AST representation of assignment statement</returns>
    private Statement FormAssign(string s, ArithExpr exp)
    {
        return Statement.NewAssign(new Tuple<string, ArithExpr>(s, exp));
    }
    /// <summary>
    /// Method to perform the decomposition of an logical assignment command.
    /// </summary>
    /// <param name="s">Variable on the LHS of the assignment</param>
    /// <param name="exp">Logical expression AST to be assigned</param>
    /// <returns cref="Statement">AST representation of assignment statement</returns>
    private Statement FormAssign(string s, BoolExpr exp)
    {
        return Statement.NewAssignB(new Tuple<string, BoolExpr>(s, exp));
    }
    
    /// <summary>
    /// Method to obtain the quantum/classical register allocation into AST
    /// </summary>
    /// <param name="dict">Dictionary of register information</param>
    /// <returns cref="Bit">Bit typed AST</returns>
    private Bit DestructBitRegister(Dictionary<string, Tuple<int, int>> dict)
    {
        var list = dict.ToList();
        list.Sort((kvp1, kvp2) => 
            kvp2.Value.Item2.CompareTo(kvp1.Value.Item2));
        var bitList = list.Select(FormBit).ToList();
        
        if (bitList.Count() > 1)
        {
            return bitList.Aggregate((current, next) =>
                Bit.NewBitSeq(new Tuple<Bit, Bit>(next, current)));
        }
        return bitList.First();
    }
    
    /// <summary>
    /// Method to destruct the arithmetic variables recorded in memory into AST
    /// </summary>
    /// <returns>List of statements obtained in AST format</returns>
    private IEnumerable<Statement> DestructArithmetic()
    {
        var dict = Reg.ArithVariables;
        var list = dict.ToList();
        list.Sort((kvp1, kvp2) => 
            kvp1.Value.Item2.CompareTo(kvp2.Value.Item2));
        return list.Select(
            kvp => FormAssign(kvp.Key, kvp.Value.Item1)
            ).ToList();
    }
    
    /// <summary>
    /// Method to destruct the logical variables recorded in memory into AST
    /// </summary>
    /// <returns>List of statements obtained in AST format</returns>
    private IEnumerable<Statement> DestructBoolean()
    {
        var dict = Reg.BoolVariables;
        var list = dict.ToList();
        list.Sort((kvp1, kvp2) => 
            kvp1.Value.Item2.CompareTo(kvp2.Value.Item2));
        return list.Select(
            kvp => FormAssign(kvp.Key, kvp.Value.Item1)
        ).ToList();
    }
    
    /// <summary>
    /// Method to completely decompose the register object within the circuit.
    /// </summary>
    /// <returns>Tuple of AST allocation and schema</returns>
    private Tuple<Allocation, List<Statement>> DestructRegister() {
        var qalloc = DestructBitRegister(Reg.Qubits);
        var calloc = DestructBitRegister(Reg.Cbits);
        var alloc = Allocation.NewAllocQC(new Tuple<Bit, Bit>(qalloc, calloc));

        var list = DestructArithmetic().ToList();
        list.AddRange(DestructBoolean());
        return new Tuple<Allocation, List<Statement>>(alloc, list);
    }
    
    /// <summary>
    /// Method to recover the bit information from the register object.
    /// </summary>
    /// <param name="index">Index of bit the query is for</param>
    /// <returns cref="Bit">Bit typed AST</returns>
    private Bit RecoverBit(int index) {
        KeyValuePair<string, Tuple<int, int>> goal;
        if (index < Reg.QubitNumber)
        {
            goal = Reg.Qubits.FirstOrDefault(kvp =>
                index < kvp.Value.Item1 + kvp.Value.Item2);
        }
        else
        {
            index -= Reg.QubitNumber;
            goal = Reg.Cbits.FirstOrDefault(kvp =>
                index < kvp.Value.Item1 + kvp.Value.Item2);
        }
        
        if (goal.Value.Item1 == 1)
        {
            return Bit.NewBitS(goal.Key);
        }
        var i = index - goal.Value.Item2;
        return Bit.NewBitA(new Tuple<string, int>(goal.Key, i));    
    }
    
    
    /// <summary>
    /// Method to decompose the support gate into AST representation.
    /// </summary>
    /// <param name="gate" cref="ISupportGate">Target support gate for decomposition</param>
    /// <returns cref="Statement">Statement type in the AST</returns>
    private Statement? DestructSupport(ISupportGate gate)
    {
        var bit1 = gate.TargetRange.Item1;
        var bit2 = gate.TargetRange.Item2;
        switch (gate.SupportType)
        {
            case SupportType.Barrier:
                return Statement.NewBarrier(RecoverBit(bit1));
            case SupportType.Reset:
                return Statement.NewReset(RecoverBit(bit2));
            case SupportType.PhaseDisk:
                return Statement.PhaseDisk;
            case SupportType.Measure:
                return Statement.NewMeasure(new Tuple<Bit, Bit>( 
                    RecoverBit(bit1), RecoverBit(bit2)));
            default:
                return null;
        }
    }
    
    /// <summary>
    /// Method to decompose the generic gates into AST representation.
    /// </summary>
    /// <param name="gate" cref="IGate">Target quantum gate to be decomposed</param>
    /// <returns cref="Statement">Statement type in the AST</returns>
    private Statement? DestructGate(IGate gate)
    {
        var bit1 = gate.TargetRange.Item1;
        var cond = gate.Condition;
        Statement? op = null;
        switch (gate.Type)
        {
            case GateType.Support:
                op = DestructSupport((ISupportGate) gate);
                break;
            case GateType.Single:
                op = Statement.NewUnaryGate(new Tuple<UTag, Bit>(
                    ((SingleQubitGate) gate).Tag, RecoverBit(bit1)));
                break;
            case GateType.Double:
                var cast = (DoubleQubitGate) gate;
                op = Statement.NewBinaryGate(new Tuple<BTag, Bit, Bit>(
                    cast.Tag, RecoverBit(cast.Control.Item1), RecoverBit(cast.Control.Item2)));
                break;
            case GateType.Param:
                var cast2 = (ParamSingleGate) gate;
                var pars = Handler.parseArith(cast2.Theta.Item2);
                
                // String maintenance was tested to be correct
                op = Statement.NewParamGate(new Tuple<PTag, ArithExpr, Bit>(
                    cast2.Tag, pars.Item1.Value, RecoverBit(bit1)));
                break;
            case GateType.DoubleParam:
                var cast3 = (ParamDoubleGate) gate;
                pars = Handler.parseArith(cast3.Theta.Item2);
                
                // String maintenance was tested to be correct
                op = Statement.NewBinaryParamGate(new Tuple<BPTag, ArithExpr, Bit, Bit>(
                    cast3.Tag, pars.Item1.Value, RecoverBit(cast3.Control.Item1), RecoverBit(cast3.Control.Item2)));
                break;
            case GateType.Toffoli:
                var triplet = ((ToffoliGate) gate).Control;
                op = Statement.NewToffoli(new Tuple<Bit, Bit, Bit>(
                    RecoverBit(triplet.Item1), RecoverBit(triplet.Item2), RecoverBit(triplet.Item3)));
                break;
            case GateType.Unitary:
                var cast4 = (UnitaryGate) gate;
                var parsT = Handler.parseArith(cast4.Theta.Item2);
                var parsP = Handler.parseArith(cast4.Phi.Item2);
                var parsL = Handler.parseArith(cast4.Lambda.Item2);
                
                // String maintenance was tested to be correct
                op = Statement.NewUnitary(new Tuple<ArithExpr, ArithExpr, ArithExpr, Bit>(
                    parsT.Item1.Value, parsP.Item1.Value, parsL.Item1.Value, RecoverBit(bit1)));
                break;
        }
        if (cond == null)
        {
            return op;
        }
        var expr = Handler.parseBool(cond);
        
        if (expr.Item1 == FSharpOption<BoolExpr>.None)
        {
            return op;
        }
        return Statement.NewCondition(
            new Tuple<BoolExpr, Statement?>(expr.Item1.Value, op));
    }
    /// <summary>
    /// Method to deconstruct an entire tower structure into AST representation.
    /// </summary>
    /// <param name="tower" cref="Tower">Target tower within circuit to be decomposed</param>
    /// <returns>List of statements to be included in the schema AST</returns>
    private IEnumerable<Statement> DestructTower(Tower tower)
    {
        var statements = tower.Gates.Select(DestructGate).Where(op => op != null);
        return statements.ToList();
    }
    
    /// <summary>
    /// Method to deconstruct the entire circuit into AST representation. Interface method for the generator, accessible only within the package.
    /// </summary>
    /// <returns cref="Tuple{Allocatin, Schema}">Complete AST structure formed from allocation and schema types</returns>
    internal Tuple<Allocation, Schema> DestructCircuit()
    {
        var list = new List<Statement>();
        foreach (var tower in Towers)
        {
            var additional = DestructTower(tower).ToList();
            additional.RemoveAll(gate => gate == null);
            list.AddRange(additional);
        }
        list.RemoveAll(gate => gate == null);
        
        var alloc = DestructRegister();
        var finalList = alloc.Item2;
        finalList.AddRange(list);
        var flow = Schema.NewFlow(ListModule.OfSeq(finalList));
        
        return new Tuple<Allocation, Schema>(alloc.Item1, flow);
    }
}