using MathNet.Numerics;
using QuantumLanguage;
using QuboxSimulator.Gates;
using static QuantumLanguage.AST;

namespace QuboxSimulator.Circuits;

public class Generator
{
    public static Register? Reg { private get; set; }
    public @operator ast = @operator.NOP;

    private static bit FormBit(KeyValuePair<string, Tuple<int, int>> triplet)
    {
        var id = triplet.Key;
        var num = triplet.Value.Item1;
        return num == 1 ? bit.NewBitS(id) : 
            bit.NewBitA(new Tuple<string, int>(id,num));
    }

    private static @operator FormAssign(string s, arithExpr exp)
    {
        return @operator.NewAssign(new Tuple<string, arithExpr>(s, exp));
    }

    private static @operator FormAssign(string s, boolExpr exp)
    {
        return @operator.NewAssignB(new Tuple<string, boolExpr>(s, exp));
    }

    private static bit DestructBitRegister(Dictionary<string, Tuple<int, int>> dict)
    {
        var list = dict.ToList();
        list.Sort((kvp1, kvp2) => 
            kvp2.Value.Item2.CompareTo(kvp1.Value.Item2));
        var bitList = list.Select(FormBit).ToList();
        
        if (bitList.Count() > 1)
        {
            return bitList.Aggregate((current, next) =>
                bit.NewBitSeq(new Tuple<bit, bit>(next, current)));
        }
        return bitList.First();
    }
    
    private static List<@operator> DestructArithmetic(Dictionary<string, Tuple<arithExpr, int>> dict)
    {
        var list = dict.ToList();
        list.Sort((kvp1, kvp2) => 
            kvp2.Value.Item2.CompareTo(kvp1.Value.Item2));
        return list.Select(
            kvp => FormAssign(kvp.Key, kvp.Value.Item1)
            ).ToList();
    }

    private static List<@operator> DestructBoolean(Dictionary<string, Tuple<boolExpr, int>> dict)
    {
        var list = dict.ToList();
        list.Sort((kvp1, kvp2) => 
            kvp2.Value.Item2.CompareTo(kvp1.Value.Item2));
        return list.Select(
            kvp => FormAssign(kvp.Key, kvp.Value.Item1)
        ).ToList();
    }

    public static @operator DestructRegister(Register register)
    {
        Reg = register;
        var qalloc = DestructBitRegister(register.Qubits);
        var calloc = DestructBitRegister(register.Cbits);
        var alloc = @operator.NewAllocQC(new Tuple<bit, bit>(qalloc, calloc));

        var list = DestructArithmetic(register.ArithVariables);
        list.AddRange(DestructBoolean(register.BoolVariables));

        list.Reverse();
        if (list.Count == 0)
        {
            return alloc;
        }

        return list.Aggregate(alloc, (current, next) =>
            @operator.NewOrder(new Tuple<@operator, @operator>(next, current)));
    }

    private static bit RecoverBit(Register? register, int index)
    {
        KeyValuePair<string, Tuple<int, int>> goal = new KeyValuePair<string, Tuple<int, int>>();
        if (index < register.QubitNumber)
        {
            goal = register.Qubits.FirstOrDefault(kvp =>
                index < kvp.Value.Item1 + kvp.Value.Item2);
        }
        else
        {
            index -= register.QubitNumber;
            goal = register.Cbits.FirstOrDefault(kvp =>
                index < kvp.Value.Item1 + kvp.Value.Item2);
        }
        
        if (goal.Value.Item1 == 1)
        {
            return bit.NewBitS(goal.Key);
        }

        var i = index - goal.Value.Item2;
        return bit.NewBitA(new Tuple<string, int>(goal.Key, i));    
    }


    public static @operator DestructGateGrid(List<Tower> grid)
    {
        var list = grid.Select(DestructTower).ToList();
        list.Reverse();
        list.RemoveAll(tower => tower == null);
        if (list.Count == 0)
        {
            return @operator.NOP;
        }
        if (list.Count == 1)
        {
            return list.First();
        }
        return list.Aggregate((current, next) =>
            @operator.NewOrder(new(next, current)));
    }

    private static @operator? DestructTower(Tower tower)
    {
        var list = tower.Gates.Select(DestructGate).ToList();
        list.Reverse();
        list.RemoveAll(gate => gate == null);
        if (list.Count == 0)
        {
            return null;
        }
        if (list.Count == 1)
        {
            return list.First();
        }
        return list.Aggregate((current, next) => @operator.NewOrder(
            new Tuple<@operator, @operator>(next, current)));
    }

    private static @operator? DestructGate(IGate gate)
    {
        var bit1 = gate.TargetRange.Item1;
        var bit2 = gate.TargetRange.Item2;
        var cond = gate.Condition;
        var op = @operator.NOP;
        switch (gate.Type)
        {
            case GateType.PHASEDISK:
                op = @operator.PhaseDisk;
                break;
            case GateType.RESET:
                op = @operator.NewReset(
                    RecoverBit(Reg,bit1));
                break;
            case GateType.BARRIER:
                op = @operator.NewBarrier(
                    RecoverBit(Reg,bit1));
                break;
            case GateType.NONE:
                break;
            case GateType.MEASURE:
                op = @operator.NewMeasure(
                    new Tuple<bit, bit>(
                        RecoverBit(Reg, bit1),
                        RecoverBit(Reg, bit2)));
                break;
            default:
                op = DestructMatrix((IMatrixGate)gate);
                break;
        }

        if (op.Equals(@operator.NOP))
        {
            return null;
        }
        if (cond == null)
        {
            return op;
        }
        var expr = Handler.parseBool(cond);
        if (!expr.Item2.Equals(error.Success))
        {
            throw new Exception("String build-up in condition is erroneous");
        }
        return @operator.NewCondition(
            new Tuple<boolExpr, @operator>(expr.Item1, op));
    }

    private static @operator DestructMatrix(IMatrixGate gate)
    {
        var bit1 = gate.TargetRange.Item1;
        var bit2 = gate.TargetRange.Item2;
        switch (gate.Type)
        {
            case GateType.H:
                return @operator.NewH(RecoverBit(Reg, bit1));
            case GateType.S:
                return @operator.NewS(RecoverBit(Reg, bit1));
            case GateType.T:
                return @operator.NewT(RecoverBit(Reg, bit1));
            case GateType.X:
                return @operator.NewX(RecoverBit(Reg, bit1));
            case GateType.Y:
                return @operator.NewY(RecoverBit(Reg, bit1));
            case GateType.Z:
                return @operator.NewZ(RecoverBit(Reg, bit1));
            case GateType.ID:
                return @operator.NewI(RecoverBit(Reg, bit1));
            case GateType.SX:
                return @operator.NewSX(RecoverBit(Reg, bit1));
            case GateType.SDG:
                return @operator.NewSDG(RecoverBit(Reg, bit1));
            case GateType.TDG:
                return @operator.NewTDG(RecoverBit(Reg, bit1));
            case GateType.SXDG:
                return @operator.NewSXDG(RecoverBit(Reg, bit1));
            case GateType.CNOT:
                var ctrl = ((CnotGate)gate).Control;
                return @operator.NewCNOT(
                    new Tuple<bit, bit>(
                        RecoverBit(Reg, ctrl.Item1),
                        RecoverBit(Reg, ctrl.Item2)));
            case GateType.SWAP:
                return @operator.NewCNOT(
                    new Tuple<bit, bit>(
                        RecoverBit(Reg, bit1),
                        RecoverBit(Reg, bit2)));
            case GateType.CCX:
                var ctrl2 = ((ToffoliGate)gate).Control;
                return @operator.NewCCX(
                    new Tuple<bit, bit, bit>(
                        RecoverBit(Reg, ctrl2.Item1),
                        RecoverBit(Reg, ctrl2.Item2),
                        RecoverBit(Reg, ctrl2.Item3)));
            default:
                return DestructParametric((ParametricGate)gate);
        }
    }
    
    private static @operator DestructParametric(ParametricGate gate)
    {
        var args = gate.Phase.Select(tup 
            => Handler.parseArith(tup.Item2).Item1).ToList();
        var bit1 = gate.TargetRange.Item1;
        switch (gate.Type)
        {
            case GateType.P:
                return @operator.NewP(new (
                    args[0], RecoverBit(Reg, bit1)));
            case GateType.U:
                return @operator.NewU(new (
                    args[0], args[1], args[2], RecoverBit(Reg, bit1)));
            case GateType.RXX:
                var pair = ((RxxGate)gate).Control;
                return @operator.NewRXX(new (args[0], 
                    RecoverBit(Reg, pair.Item1), 
                    RecoverBit(Reg, pair.Item2)));
            case GateType.RZZ:
                var pair2 = ((RzzGate)gate).Control;
                return @operator.NewRZZ(new (args[0], 
                    RecoverBit(Reg, pair2.Item1), 
                    RecoverBit(Reg, pair2.Item2)));
            case GateType.RX:
                return @operator.NewRX(new (
                    args[0], RecoverBit(Reg, bit1)));
            case GateType.RY:
                return @operator.NewRY(new (
                    args[0], RecoverBit(Reg, bit1)));
            case GateType.RZ:
                return @operator.NewRZ(new(
                    args[0], RecoverBit(Reg, bit1)));
            default:
                return @operator.NOP;
        }
    }
}