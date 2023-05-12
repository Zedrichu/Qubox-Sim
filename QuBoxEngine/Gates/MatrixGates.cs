using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using static QuLangProcessor.Tags;

namespace QuBoxEngine.Gates;

internal abstract class MatrixGate : IMatrixGate
{
    public Matrix<Complex> Matrix { get; set; }
    
    public Tuple<int, int> TargetRange { get; set; }
    
    public string Id { get; set; }
    
    public GateType Type { get; set; }

    public string Condition { get; set; } = "";

    public override string ToString()
    {
        return $"Gate:{Id} Target: {TargetRange}";
    }
}

internal class SingleQubitGate : MatrixGate
{
    public UTag Tag { get; }
    
    public SingleQubitGate(Matrix<Complex> matrix, UTag tag, int target, string? condition = null)
    {
        Type = GateType.Single;
        base.Matrix = matrix;
        Id = tag.ToString();
        Tag = tag;
        TargetRange = new Tuple<int, int>(target, target);
        Condition = condition ?? "";
    }    
}

internal class DoubleQubitGate : MatrixGate
{
    public BTag Tag { get; }
    public Tuple<int,int> Control { get; }
    
    public DoubleQubitGate(Matrix<Complex> matrix, BTag tag, int control, int target)
    {
        Matrix = matrix;
        TargetRange = new Tuple<int, int>(control < target? control : target, 
                                            control < target? target : control);
        Control =  new Tuple<int, int>(control, target);
        Tag = tag;
        Id = tag.ToString();
        Type = GateType.Double;
    }
}

internal class ToffoliGate : MatrixGate
{
    public Tuple<int, int, int> Control { get; }
    
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
        Type = GateType.Toffoli;
    }
}