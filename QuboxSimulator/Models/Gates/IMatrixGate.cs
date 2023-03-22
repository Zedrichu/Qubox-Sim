using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex32;

namespace QuboxSimulator.Models.Gates;

public interface IMatrixGate : IGate
{
    public abstract Matrix<Complex> Matrix { get; set; }
}


public class SingleQubitGate : IMatrixGate
{
    public Matrix<Complex> Matrix { get; set; }
    
    public Tuple<int, int> TargetRange { get; set; }
    
    public string Id { get; set; }
    
    public string? Condition { get; set; }
    
    public SingleQubitGate(Matrix<Complex> matrix, int target, string id, string? condition = null)
    {
        Matrix = matrix;
        TargetRange = new Tuple<int, int>(target, target);
        Id = id;
        Condition = condition;
    }    
}

public class CnotGate : IMatrixGate
{
    public Matrix<Complex> Matrix { get; set; }
    
    public Tuple<int, int> TargetRange { get; set; }
    
    public string Id { get; set; }

    public int Control { get; set; }

    public string? Condition { get; set; }
    
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
        Control = control;
        Id = "CNOT";
    }
}

public class SwapGate : IMatrixGate
{
    public Matrix<Complex> Matrix { get; set; }
    
    public Tuple<int, int> TargetRange { get; set; }
    
    public string Id { get; set; }
    
    public string? Condition { get; set; }
    
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
    }
}

public class ToffoliGate : IMatrixGate
{
    public Matrix<Complex> Matrix { get; set; }
    
    public Tuple<int, int> TargetRange { get; set; }
    
    public string Id { get; set; }
    
    public Tuple<int, int> Control { get; set; }

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
        Control = new Tuple<int, int>(control1, control2);
        TargetRange = new Tuple<int, int>(min, max);
        Id = "CCX";
    }
}