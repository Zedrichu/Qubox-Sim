namespace QuBoxEngine.Gates;
/* C#
 -*- coding: utf-8 -*-
ParametricGates

Description: Module implementing the parametric generic quantum gates.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 18/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/

using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using static QuLangProcessor.Tags;

/// <summary>
/// Abstract class implementing the common fields of all parametric gates. Extends the IMatrixGate interface.
/// </summary>
internal abstract class ParametricGate: IMatrixGate
{
    public Matrix<Complex> Matrix { get; set; }
    
    public Tuple<int, int> TargetRange { get; set; }
    
    public string Id { get; set; }

    public GateType Type { get; set; } = GateType.Param;
    
    public string? Condition { get; set; }
    
    
    // All parametric gates have a theta parameter
    internal Tuple<double, string> Theta { get; set; }

    public override string ToString()
    {
        return $"Gate:{Id} Target: {TargetRange} Theta: {Theta.Item1}";
    }
}

/// <summary>
/// Class of single qubit parametric gates. Extends the ParametricGate abstract class.
/// </summary>
internal class ParamSingleGate : ParametricGate
{
    /// <summary>
    /// Distinctive parametric tag of the gate.
    /// </summary>
    internal PTag Tag { get; }
    /// <summary>
    /// Constructor of single qubit parametric gates.
    /// </summary>
    /// <param name="matrix">Matrix associated with the matrix</param>
    /// <param name="tag" cref="PTag">Distinctive parametric tag</param>
    /// <param name="target">Index of qubit on which gate is applied</param>
    /// <param name="phase">Tuple of value/string for phase parameter</param>
    /// <param name="condition">Optional string condition on the gate</param>
    internal ParamSingleGate(Matrix<Complex> matrix, PTag tag, int target, Tuple<double, string> phase, string? condition = null)
    {
        Tag = tag;
        Matrix = matrix;
        TargetRange = new Tuple<int, int>(target, target);
        Id = tag.ToString();
        Condition = condition;
        Theta = phase;
    }
}

/// <summary>
/// Class of Unitary gate with 3 different phase parameters. Extends the ParametricGate abstract class.
/// </summary>
internal class UnitaryGate : ParametricGate
{
    /// <summary>
    /// Additional φ parameter of the gate.
    /// </summary>
    internal Tuple<double, string> Phi { get; }
    /// <summary>
    /// Additional λ parameters of the gate
    /// </summary>
    internal Tuple<double, string> Lambda { get; }
    
    /// <summary>
    /// Constructor for the Unitary gate.
    /// </summary>
    /// <param name="args">Array of argument tuples</param>
    /// <param name="target">Index of target qubit</param>
    internal UnitaryGate(Tuple<double, string>[] args, int target)
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

/// <summary>
/// Class of double qubit parametric gates. Extends the ParametricGate abstract class.
/// </summary>
internal class ParamDoubleGate : ParametricGate
{
    /// <summary>
    /// Tuple preserving the order of target qubits.
    /// </summary>
    internal Tuple<int, int> Control { get; set; }
    /// <summary>
    /// Distinctive binary parametric tag of the gate.
    /// </summary>
    internal BPTag Tag { get; }
    
    /// <summary>
    /// Constructor of double qubit parametric gates.
    /// </summary>
    /// <param name="matrix">Matrix associated with the gate</param>
    /// <param name="tag" cref="BPTag">Distinctive binary parametric tag</param>
    /// <param name="target1">Index of first target qubit</param>
    /// <param name="target2">Index of second target qubit</param>
    /// <param name="phase">Tuple of value/string for the phase parameter</param>
    internal ParamDoubleGate(Matrix<Complex> matrix, BPTag tag, int target1, int target2, Tuple<double, string> phase)
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