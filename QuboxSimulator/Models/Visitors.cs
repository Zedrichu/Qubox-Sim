using static QuantumLanguage.VisitorPattern;
using static QuantumLanguage.AST;
using QuboxSimulator.Models.Gates;

namespace QuboxSimulator.Models;
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
public class OperatorVisitor: IVisitor<@operator, List<IGate>>
{
    private readonly Memory _memory; 
    private readonly ArithmeticVisitor _arithVisitor;
  
    public OperatorVisitor(Memory memory)
    {
        _memory = memory;
        _arithVisitor = new ArithmeticVisitor(memory.Arithmetic);
    }
    
    private Tuple<double, string> GetPhaseTuple(arithExpr ast)
    {
        var angle = ast.Accept(_arithVisitor);
        var str = ast.ToString();
        return new Tuple<double, string>(angle, str);
    }

    public List<IGate> Visit(@operator ast)
    {
        var gateList = new List<IGate>();
        switch (ast)
        {
            case @operator.Order pair:
                var list1 = pair.Item.Item1.Accept(this);
                var list2 = pair.Item.Item2.Accept(this);
                gateList.AddRange(list1);
                gateList.AddRange(list2);
                break;
            case @operator.H: case @operator.X:
            case @operator.I: case @operator.S:
            case @operator.SDG: case @operator.T:
            case @operator.TDG: case @operator.SX:
            case @operator.SXDG: case @operator.Y:
            case @operator.Z:
                var tokbit = ast.DestructSingle();
                var target= _memory.GetOrder(tokbit.Item2);
                gateList.Add(GateFactory.GetSingleGate(tokbit.Item1, target));
                break;
            case @operator.RX: case @operator.RY:
            case @operator.RZ: case @operator.P:
                var tokparbit = ast.DestructParam();
                target = _memory.GetOrder(tokparbit.Item3);
                var phase = GetPhaseTuple(tokparbit.Item2);
                var gate = GateFactory.GetParamGate(tokparbit.Item1, target, phase);
                gateList.Add(gate);
                break;
            case @operator.Condition pair:
                var condition = pair.Item.Item1.ToString();
                var list = pair.Item.Item2.Accept(this);
                list.ForEach(g => g.Condition = condition);
                gateList.AddRange(list); 
                break;
            case @operator.CCX triplet:
                target = _memory.GetOrder(triplet.Item.Item1);
                var target2 = _memory.GetOrder(triplet.Item.Item2);
                var target3 = _memory.GetOrder(triplet.Item.Item3);
                gate = GateFactory.GetMultipleGate("CCX", target, target2, target3);
                if (gate != null) gateList.Add(gate);
                break;
            case @operator.U quadruplet:
                var lambda = GetPhaseTuple(quadruplet.Item.Item1);
                var phi = GetPhaseTuple(quadruplet.Item.Item2);
                var theta = GetPhaseTuple(quadruplet.Item.Item3);
                target = _memory.GetOrder(quadruplet.Item.Item4);
                gateList.Add(GateFactory.GetUnitaryGate(
                    new[] { lambda, phi, theta }, target));
                break;
            case @operator.Reset op:
                target = _memory.GetOrder(op.Item);
                var supp = GateFactory.GetSupportGate("RESET", target);
                if (supp != null) gateList.Add(supp);
                break;
            case @operator.Barrier bar:
                target = _memory.GetOrder(bar.Item);
                supp = GateFactory.GetSupportGate("BARRIER", target);
                if (supp != null) gateList.Add(supp);
                break;
            case @operator.Measure pair:
                target = _memory.GetOrder(pair.Item.Item1);
                var reg = _memory.CountQuantum;
                supp = GateFactory.GetSupportGate("MEASURE", target, reg);
                if (supp != null) gateList.Add(supp);
                break;
            case @operator.SWAP pair:
                target = _memory.GetOrder(pair.Item.Item1);
                target2 = _memory.GetOrder(pair.Item.Item2);
                gate = GateFactory.GetMultipleGate("SWAP", target, target2);
                if (gate != null) gateList.Add(gate);
                break;
            case @operator.CNOT pair:
                target = _memory.GetOrder(pair.Item.Item1);
                target2 = _memory.GetOrder(pair.Item.Item2);
                gate = GateFactory.GetMultipleGate("CNOT", target, target2);
                if (gate != null) gateList.Add(gate);
                break;
            case @operator.RZZ triplet:
                phase = GetPhaseTuple(triplet.Item.Item1);
                target = _memory.GetOrder(triplet.Item.Item2);
                target2 = _memory.GetOrder(triplet.Item.Item3);
                gate = GateFactory.GetMultipleGate("RZZ", target, target2, -1, phase);
                if (gate != null) gateList.Add(gate);
                break;
            case @operator.RXX triplet:
                phase = GetPhaseTuple(triplet.Item.Item1);
                target = _memory.GetOrder(triplet.Item.Item2);
                target2 = _memory.GetOrder(triplet.Item.Item3);
                gate = GateFactory.GetMultipleGate("RXX", target, target2, -1, phase);
                if (gate != null) gateList.Add(gate);
                break;
            case var _ when ast.Equals(@operator.PhaseDisk):
                target = _memory.CountQuantum;
                supp = GateFactory.GetSupportGate("PHASEDISK", target-1);
                if (supp != null) gateList.Add(supp);
                break;
        }

        return gateList;
    }
}

public class ArithmeticVisitor : IVisitor<arithExpr, double>
{
    private Dictionary<string, arithExpr> _memory;
    
    public ArithmeticVisitor(IDictionary<string, arithExpr> memory)
    {
        _memory = new Dictionary<string, arithExpr>(memory);
    }

    public double Visit(arithExpr expr) {
        var value = 0.0;
        switch (expr)
        {
            case arithExpr.Num x:
                value = Convert.ToDouble(x.Item);
                break;
            case arithExpr.Float x:
                value = x.Item;
                break;
            case arithExpr.DivExpr pair:
                var left = pair.Item.Item1.Accept(this);
                var right = pair.Item.Item2.Accept(this);
                value = left / right;
                break;
            case arithExpr.MinusExpr pair:
                left = pair.Item.Item1.Accept(this);
                right = pair.Item.Item2.Accept(this);
                value = left - right;
                break;
            case arithExpr.PlusExpr pair:
                left = pair.Item.Item1.Accept(this);
                right = pair.Item.Item2.Accept(this);
                value = left + right;
                break;
            case arithExpr.TimesExpr pair:
                left = pair.Item.Item1.Accept(this);
                right = pair.Item.Item2.Accept(this);
                value = left * right;
                break;
            case arithExpr.VarA x:
                value = _memory[x.Item].Accept(this);
                break;
            case var _ when expr.Equals(arithExpr.Pi):
                value = Math.PI;
                break;
            case arithExpr.PowExpr pair:
                left = pair.Item.Item1.Accept(this);
                right = pair.Item.Item2.Accept(this);
                value = Math.Pow(left, right);
                break;
            case arithExpr.ModExpr pair:
                left = pair.Item.Item1.Accept(this);
                right = pair.Item.Item2.Accept(this);
                value = left % right;
                break;
            case arithExpr.UMinusExpr x:
                value = -x.Item.Accept(this);
                break;
            case arithExpr.UPlusExpr x:
                value = x.Item.Accept(this);
                break;
        }
        return value;
    }   
    
}
