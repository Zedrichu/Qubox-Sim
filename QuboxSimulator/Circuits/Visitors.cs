using QuboxSimulator.Gates;
using static QuantumLanguage.VisitorPattern;
using static QuantumLanguage.AST;

namespace QuboxSimulator.Circuits;
/* C#
 -*- coding: utf-8 -*-
AST Structure Visitors

Description: Implementation of the Visitor Pattern for the AST structure.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 15/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/


/// <summary>
/// Visitor class for the base operator type of the AST.
/// </summary>
public class StatementVisitor: IVisitor<Statement, IGate>
{
    private readonly Memory _memory; 
    private readonly ArithmeticVisitor _arithVisitor;
  
    public StatementVisitor(Memory memory)
    {
        // Establish the contextual memory
        _memory = memory;
        // Create arithmetic visitor with arithmetic memory
        _arithVisitor = new ArithmeticVisitor(memory.Arithmetic);
    }
    
    /// <summary>
    /// Helper method to get the phase angle and the string representation from arithmetic AST.
    /// </summary>
    /// <param name="ast">Converted arithmetic expression</param>
    /// <returns>Tuple of computed value and formed string</returns>
    private Tuple<double, string> GetPhaseTuple(ArithExpr ast)
    {
        var angle = ast.Accept(_arithVisitor);
        var str = ast.ToString();
        return new Tuple<double, string>(angle, str);
    }
    
    /// <summary>
    /// Interface method implementation interpreting operators to gate objects.
    /// </summary>
    /// <param name="ast">Operator Abstract Syntax Tree</param>
    /// <returns>List of gates obtained in interpreter</returns>
    public IGate Visit(Statement ast)
    {
        switch (ast)
        {
            case Statement.UnaryGate pair:
                // Retrieve order of the target qubit in circuit mapping
                var target1= _memory.GetQOrder(pair.Item.Item2);
                // Add gate created by factory to list
                return GateFactory.CreateGate(pair.Item.Item1, target1);
            case Statement.ParamGate triple:
                // Retrieve order of the target qubit in circuit mapping
                target1 = _memory.GetQOrder(triple.Item.Item3);
                // Get phase angle and string representation
                var phase = GetPhaseTuple(triple.Item.Item2);
                // Add gate created by factory to list
                return GateFactory.CreateGate(triple.Item.Item1, target1, phase);
            case Statement.BinaryGate pair:
                target1 = _memory.GetQOrder(pair.Item.Item2);
                var target2 = _memory.GetQOrder(pair.Item.Item3);
                return GateFactory.CreateGate(pair.Item.Item1, target1, target2);
            case Statement.Condition pair:
                var condition = pair.Item.Item1.ToString();
                var gate = pair.Item.Item2.Accept(this);
                gate.Condition = condition;
                return gate;
            case Statement.Toffoli triplet:
                target1 = _memory.GetQOrder(triplet.Item.Item1);
                target2 = _memory.GetQOrder(triplet.Item.Item2);
                var target3 = _memory.GetQOrder(triplet.Item.Item3);
                return GateFactory.CreateGate(target1, target2, target3);
            case Statement.Unitary quadruplet:
                var lambda = GetPhaseTuple(quadruplet.Item.Item1);
                var phi = GetPhaseTuple(quadruplet.Item.Item2);
                var theta = GetPhaseTuple(quadruplet.Item.Item3);
                target1 = _memory.GetQOrder(quadruplet.Item.Item4);
                return GateFactory.CreateGate(
                    new[] { lambda, phi, theta }, target1);
            case Statement.Reset op:
                target1 = _memory.GetQOrder(op.Item);
                return GateFactory.CreateGate(SupportType.Reset, target1);
            case Statement.Barrier bar:
                target1 = _memory.GetQOrder(bar.Item);
                return GateFactory.CreateGate(SupportType.Barrier, target1);
            case Statement.Measure pair:
                target1 = _memory.GetQOrder(pair.Item.Item1);
                target2 = _memory.GetCOrder(pair.Item.Item2) + _memory.CountQuantum;
                return GateFactory.CreateGate(SupportType.Measure, target1, target2);
            case Statement.BinaryParamGate quadruplet:
                phase = GetPhaseTuple(quadruplet.Item.Item2);
                target1 = _memory.GetQOrder(quadruplet.Item.Item3);
                target2 = _memory.GetQOrder(quadruplet.Item.Item4);
                return GateFactory.CreateGate(quadruplet.Item.Item1, target1, target2, phase);
            case var _ when ast.Equals(Statement.PhaseDisk):
                target1 = _memory.CountQuantum;
                return GateFactory.CreateGate(SupportType.PhaseDisk, target1-1);
            default:
                return GateFactory.CreatePlaceholder();
        }
    }
}

/// <summary>
/// Visitor class for arithmetic expressions
/// </summary>
public class ArithmeticVisitor : IVisitor<ArithExpr, double>
{
    private readonly Dictionary<string, Tuple<ArithExpr, int>> _memory;
    
    public ArithmeticVisitor(IDictionary<string, Tuple<ArithExpr, int>> memory)
    {
        _memory = new Dictionary<string, Tuple<ArithExpr, int>>(memory);
    }
    
    /// <summary>
    /// Interface method implementation for visiting arithmetic AST
    /// </summary>
    /// <param name="expr">Expression to be computed</param>
    /// <returns>Result of arithmetic computation</returns>
    public double Visit(ArithExpr expr) {
        var value = 0.0;
        switch (expr)
        {
            case ArithExpr.Num x:
                value = Convert.ToDouble(x.Item);
                break;
            case ArithExpr.Float x:
                value = x.Item;
                break;
            case ArithExpr.BinaryOp pair:
                var left = pair.Item.Item1.Accept(this);
                var right = pair.Item.Item3.Accept(this);
                value = pair.Item.Item2 switch
                {
                    var x when x.Equals(AOp.Add) => left + right,
                    var x when x.Equals(AOp.Sub) => left - right,
                    var x when x.Equals(AOp.Mul) => left * right,
                    var x when x.Equals(AOp.Div) => left / right,
                    var x when x.Equals(AOp.Pow) => Math.Pow(left, right),
                    var x when x.Equals(AOp.Mod) => left % right,
                    _ => value
                };
                break;
            case ArithExpr.VarA x:
                value = _memory[x.Item].Item1.Accept(this);
                break;
            case var _ when expr.Equals(ArithExpr.Pi):
                value = Math.PI;
                break;
            case ArithExpr.UnaryOp x:
                value = x.Item.Item2.Accept(this);
                if (x.Item.Item1.Equals(AOp.Minus))
                    value = -value;
                break;
        }
        return value;
    }   
    
}
