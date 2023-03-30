using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
namespace QuboxSimulator.Models.Gates;

public abstract class ParametricGate: IMatrixGate
{
    public Matrix<Complex> Matrix { get; set; }
    
    public Tuple<int, int> TargetRange { get; set; }
    
    public string Id { get; set; }
    
    public string? Condition { get; set; }
    
    protected Tuple<double, string>[] Phase { get; set; }
}


public class ParamSingleGate : ParametricGate
{
    public ParamSingleGate(Matrix<Complex> matrix, int target, string id, Tuple<double, string> phase, string? condition = null)
    {
        Matrix = matrix;
        TargetRange = new Tuple<int, int>(target, target);
        Id = id;
        Condition = condition;
        Phase = new []{phase};
    }
}

public class UnitaryGate : ParametricGate
{
    public UnitaryGate(Tuple<double, string>[] args, int target)
    {
        Phase = args;
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

public class RxxGate : ParametricGate
{

    public RxxGate(int target1, int target2, Tuple<double, string> phase)
    {
        var min = Math.Min(target1, target2);
        var max = Math.Max(target1, target2);
        TargetRange = new Tuple<int, int>(min, max);
        Id = "RXX";
        Phase = new [] {phase};
        Matrix = Matrix<Complex>.Build.DenseOfArray(new [,]
        {
            {Complex.One, Complex.Zero, Complex.Zero, Complex.Zero},
            {Complex.Zero, Complex.One, Complex.Zero, Complex.Zero},
            {Complex.Zero, Complex.Zero, Complex.Cos(Phase[0].Item1), -Complex.ImaginaryOne * Complex.Sin(Phase[0].Item1)},
            {Complex.Zero, Complex.Zero, -Complex.ImaginaryOne * Complex.Sin(Phase[0].Item1), Complex.Cos(Phase[0].Item1)}
        });
    }
}

public class RzzGate : ParametricGate
{
    public RzzGate(int target1, int target2, Tuple<double, string> phase)
    {
        var min = Math.Min(target1, target2);
        var max = Math.Max(target1, target2);
        TargetRange = new Tuple<int, int>(min, max);
        Id = "RZZ";
        Phase = new [] {phase};
        Matrix = Matrix<Complex>.Build.DenseOfArray(new [,]
        {
            {Complex.Cos(Phase[0].Item1), Complex.Zero, Complex.Zero, -Complex.ImaginaryOne * Complex.Sin(Phase[0].Item1)},
            {Complex.Zero, Complex.Cos(Phase[0].Item1), Complex.ImaginaryOne * Complex.Sin(Phase[0].Item1), Complex.Zero},
            {Complex.Zero, Complex.ImaginaryOne * Complex.Sin(Phase[0].Item1), Complex.Cos(Phase[0].Item1), Complex.Zero},
            {-Complex.ImaginaryOne * Complex.Sin(Phase[0].Item1), Complex.Zero, Complex.Zero, Complex.Cos(Phase[0].Item1)}
        });
    }
}