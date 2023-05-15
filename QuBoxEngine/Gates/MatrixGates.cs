namespace QuBoxEngine.Gates;
/* C#
 -*- coding: utf-8 -*-
MatrixGates

Description: Implementation of different matrix associated quantum gates.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 15/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/

using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using static QuLangProcessor.Tags;


/// <summary>
/// Abstract class implementing the common fields of all matrix gates. Extends the IMatrixGate interface.
/// </summary>
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

/// <summary>
/// Class of single qubit quantum gates. Extends the MatrixGate abstract class.
/// </summary>
internal class SingleQubitGate : MatrixGate
{
    /// <summary>
    /// Distinctive unary tag of the gate.
    /// </summary>
    public UTag Tag { get; }
    
    /// <summary>
    /// Constructor of single qubit gates.
    /// </summary>
    /// <param name="matrix">Matrix associated with the gate</param>
    /// <param name="tag" cref="UTag">Distinctive unary tag</param>
    /// <param name="target">Index of target qubit</param>
    /// <param name="condition">Optional condition applied on the gate</param>
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

/// <summary>
/// Class of double qubit quantum gates. Extends the MatrixGate abstract class.
/// </summary>
internal class DoubleQubitGate : MatrixGate
{
    /// <summary>
    /// Distinctive binary tag of the gate.
    /// </summary>
    public BTag Tag { get; }
    /// <summary>
    /// Tuple preserving order of control/target qubits.
    /// </summary>
    public Tuple<int,int> Control { get; }
    
    /// <summary>
    /// Constructor of double qubit gates.
    /// </summary>
    /// <param name="matrix">Matrix associated with the gate</param>
    /// <param name="tag" cref="BTag">Distinctive binary tag</param>
    /// <param name="control">Index of first target qubit</param>
    /// <param name="target">Index of second target qubit</param>
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

/// <summary>
/// Class of Toffoli (CCX / CCNOT) triple qubit gate. Extends the MatrixGate abstract class.
/// </summary>
internal class ToffoliGate : MatrixGate
{
    /// <summary>
    /// Tuple preserving order of target qubits.
    /// </summary>
    public Tuple<int, int, int> Control { get; }
    
    /// <summary>
    /// Constructor of Toffoli gate.
    /// </summary>
    /// <param name="control1">Index of first qubit</param>
    /// <param name="control2">Index of second qubit</param>
    /// <param name="target">Index of third qubit</param>
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