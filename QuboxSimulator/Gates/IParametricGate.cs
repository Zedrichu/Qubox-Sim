using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using static QuantumLanguage.Tags;

namespace QuboxSimulator.Gates;

public abstract class ParametricGate: IMatrixGate
{
    public Matrix<Complex> Matrix { get; set; }
    
    public Tuple<int, int> TargetRange { get; set; }
    
    public string Id { get; set; }

    public GateType Type { get; set; } = GateType.Param;
    
    public string? Condition { get; set; }
    
    public Tuple<double, string> Theta { get; protected set; }
}


public class ParamSingleGate : ParametricGate
{
    public PTag Tag { get; }
    public ParamSingleGate(Matrix<Complex> matrix, PTag tag, int target, Tuple<double, string> phase, string? condition = null)
    {
        Tag = tag;
        Matrix = matrix;
        TargetRange = new Tuple<int, int>(target, target);
        Id = tag.ToString();
        Condition = condition;
        Theta = phase;
    }
}

public class UnitaryGate : ParametricGate
{
    public Tuple<double, string> Phi { get; }
    public Tuple<double, string> Lambda { get; }
    public UnitaryGate(Tuple<double, string>[] args, int target)
    {
        Theta = args[0];
        Phi = args[1];
        Lambda = args[2];
        Type = GateType.Unitary;
        Id = "U";
        var theta = args[0].Item1;
        var phi = args[1].Item1;
        var lambda = args[2].Item1;
        Matrix = Matrix<Complex>.Build.DenseOfArray(
            new [,]{
                { Complex.Cos(phi / 2), -Complex.Exp(Complex.ImaginaryOne * lambda) * Complex.Sin(phi / 2) },
                { Complex.Exp(Complex.ImaginaryOne * phi) * Complex.Sin(phi / 2), Complex.Exp(Complex.ImaginaryOne * (phi + lambda)) * Complex.Cos(phi / 2)}
            });
        TargetRange = new Tuple<int, int>(target, target);
    }
}

public class ParamDoubleGate : ParametricGate
{
    public Tuple<int, int> Control { get; set; }
    
    public BPTag Tag { get; }
    
    public ParamDoubleGate(Matrix<Complex> matrix, BPTag tag, int target1, int target2, Tuple<double, string> phase)
    {
        var min = Math.Min(target1, target2);
        var max = Math.Max(target1, target2);
        TargetRange = new Tuple<int, int>(min, max);
        Tag = tag;
        Control = new(target1, target2);
        Id = tag.ToString();
        Type = GateType.DoubleParam;
        Theta = phase;
        Matrix = matrix;
    }
}