using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex32;

namespace QuboxSimulator.Gates;

public interface IMatrixGate : IGate
{
    public abstract Matrix<Complex> Matrix { get; set; }
}

internal abstract class MatrixGate : IMatrixGate
{
    public Matrix<Complex> Matrix { get; set; }
    
    public Tuple<int, int> TargetRange { get; set; }
    
    public string Id { get; set; }
    
    public GateType Type { get; set; }
    
    public string? Condition { get; set; }

    public override string ToString()
    {
        return $"Gate:{Id} Target: {TargetRange}";
    }
}

internal class SingleQubitGate : MatrixGate
{
    public SingleQubitGate(Tuple<GateType, Matrix<Complex>> type, int target, string id, string? condition = null)
    {
        Type = type.Item1;
        Matrix = type.Item2;
        TargetRange = new Tuple<int, int>(target, target);
        Id = id;
        Condition = condition;
    }    
}

internal class CnotGate : MatrixGate
{
    public Tuple<int,int> Control { get; set; }
    
    public CnotGate(int control, int target)
    {
        Matrix = Matrix<Complex>.Build.DenseOfArray(
            new Complex[,]{
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 0, 1 },
                { 0, 0, 1, 0 }
            });
        TargetRange = new Tuple<int, int>(control < target? control : target, 
                                            control < target? target : control);
        Control =  new Tuple<int, int>(control, target);
        Id = "CNOT";
        Type = GateType.CNOT;
    }
}

internal class SwapGate : MatrixGate
{

    public SwapGate(int qubit1, int qubit2)
    {
        Matrix = Matrix<Complex>.Build.DenseOfArray(
            new Complex[,]{
                { 1, 0, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 0, 1 }
            });
        TargetRange = new Tuple<int, int>(qubit1 < qubit2? qubit1:qubit2, 
                                            qubit1 < qubit2? qubit2:qubit1);
        Id = "SWAP";
        Type = GateType.SWAP;
    }
}

internal class ToffoliGate : IMatrixGate
{
    public Matrix<Complex> Matrix { get; set; }
    
    public Tuple<int, int> TargetRange { get; set; }
    
    public string Id { get; set; }
    
    public GateType Type { get; set; }
    
    public Tuple<int, int, int> Control { get; set; }

    public string? Condition { get; set; }
    
    public ToffoliGate(int control1, int control2, int target)
    {
        Matrix = Matrix<Complex>.Build.DenseOfArray(
            new Complex[,]{
                { 1, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 1, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 1, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 1, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 1, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 1 },
                { 0, 0, 0, 0, 0, 0, 1, 0 }
            });
        var min = Math.Min(control1, Math.Min(control2, target));
        var max = Math.Max(control1, Math.Max(control2, target));
        Control = new Tuple<int, int, int>(control1, control2, target);
        TargetRange = new Tuple<int, int>(min, max);
        Id = "CCX";
        Type = GateType.CCX;
    }
}