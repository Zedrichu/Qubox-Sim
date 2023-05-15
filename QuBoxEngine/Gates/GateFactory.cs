using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using QuLangProcessor;

namespace QuBoxEngine.Gates;
/* C#
 -*- coding: utf-8 -*-
GateFactory

Description: Factory class responsible for handling the creation of quantum gates based on specific parameters.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 29/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/

using static Tags;

/// <summary>
/// Factory class for creation of parameterised quantum gates
/// </summary>
internal static class GateFactory
{   
    /// <summary>
    /// Dictionary of single qubit gate tags and associated matrices
    /// </summary>
    private static readonly Dictionary<UTag, Matrix<Complex>> SingleGates = new() {
        {
            UTag.H, Matrix<Complex>.Build.DenseOfArray(
                new Complex[,]
                {
                    { 1, 1 },
                    { 1, -1 }
                }) / Math.Sqrt(2) }, 
        { UTag.ID, Matrix<Complex>.Build.DenseIdentity(2, 2) }, {
            UTag.X, Matrix<Complex>.Build.DenseOfArray(
                new Complex[,]
                {
                    { 0, 1 },
                    { 1, 0 }
                }) }, {
            UTag.Y, Matrix<Complex>.Build.DenseOfArray(
                new[,]
                {
                    { 0, -Complex.ImaginaryOne },
                    { Complex.ImaginaryOne, 0 }
                }) }, {
            UTag.Z,  Matrix<Complex>.Build.DenseOfArray(
                new Complex[,]
                {
                    { 1, 0 },
                    { 0, -1 }
                }) }, {
            UTag.S,  Matrix<Complex>.Build.DenseOfArray(
                new[,]
                {
                    { 1, 0 },
                    { 0, Complex.ImaginaryOne }
                }) }, {
            UTag.SDG, Matrix<Complex>.Build.DenseOfArray(
                new[,]
                {
                    { 1, 0 },
                    { 0, -Complex.ImaginaryOne }
                }) }, {
            UTag.T, Matrix<Complex>.Build.DenseOfArray(
                new[,]
                {
                    { 1, 0 },
                    { 0, Complex.Exp(Complex.ImaginaryOne * Math.PI / 4) }
                }) }, {
            UTag.TDG, Matrix<Complex>.Build.DenseOfArray(
                new[,]
                {
                    { 1, 0 },
                    { 0, Complex.Exp(Complex.ImaginaryOne * -Math.PI / 4) }
                }) }, {
            UTag.SX, Matrix<Complex>.Build.DenseOfArray(
                new[,]
                {
                    { 1 + Complex.ImaginaryOne, 1 - Complex.ImaginaryOne },
                    { 1 - Complex.ImaginaryOne, 1 + Complex.ImaginaryOne }
                }) / 2 }, {
            UTag.SXDG, Matrix<Complex>.Build.DenseOfArray(
                new[,]
                {
                    { 1 - Complex.ImaginaryOne, 1 + Complex.ImaginaryOne },
                    { 1 + Complex.ImaginaryOne, 1 - Complex.ImaginaryOne }
                }) / 2 },
    };
    
    /// <summary>
    /// Method to generate the dictionary of parametric gate tags and corresponding matrices based on the Θ argument
    /// </summary>
    /// <param name="phase">Phase argument for matrix construction</param>
    /// <returns cref="IDictionary{PTag, Matrix}">Dictionary of parametric matrices</returns>
    private static Dictionary<PTag, Matrix<Complex>> ParamGates(double phase)
    {
        return new Dictionary<PTag, Matrix<Complex>>
        {
            { PTag.P, Matrix<Complex>.Build.DenseOfArray(new[,]
            {
                {1,0},
                {0, Complex.Exp(Complex.ImaginaryOne * phase)}
            }) },
            {PTag.RX, Matrix<Complex>.Build.DenseOfArray(new[,]
            {
                {Complex.Cos(phase / 2), -Complex.ImaginaryOne * Complex.Sin(phase / 2)},
                {-Complex.ImaginaryOne * Complex.Sin(phase / 2), Complex.Cos(phase / 2)}
            }) },
            {PTag.RY, Matrix<Complex>.Build.DenseOfArray(new[,]
            {
                {Complex.Cos(phase / 2), -Complex.Sin(phase / 2)},
                {Complex.Sin(phase / 2), Complex.Cos(phase / 2)}
            }) },
            {PTag.RZ, Matrix<Complex>.Build.DenseOfArray(new[,]
            {
                {Complex.Exp(-Complex.ImaginaryOne * phase / 2), 0},
                {0, Complex.Exp(Complex.ImaginaryOne * phase / 2)}
            }) }
        };
    }
    
    /// <summary>
    /// Dictionary of double qubit gates and associated matrices
    /// </summary>
    private static readonly Dictionary<BTag, Matrix<Complex>> DoubleGates = new() {
        { BTag.CNOT, Matrix<Complex>.Build.DenseOfArray(
            new Complex[,]{
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 0, 1 },
                { 0, 0, 1, 0 }
            })},
        { BTag.SWAP, Matrix<Complex>.Build.DenseOfArray(
            new Complex[,]
            {
                { 1, 0, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 0, 1 }
            })},
        { BTag.CS, Matrix<Complex>.Build.DenseOfArray(
            new [,] {
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, Complex.ImaginaryOne }
            })},
        { BTag.CH, Matrix<Complex>.Build.DenseOfArray(
            new [,] {
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 1/Complex.Sqrt(2), 1/Complex.Sqrt(2) },
                { 0, 0, 1/Complex.Sqrt(2), -1/Complex.Sqrt(2) }
            })},
    };
    
    /// <summary>
    /// Method to generate the dictionary of double qubit parametric gate tags and corresponding matrices based on the Θ argument
    /// </summary>
    /// <param name="phase">Phase argument for matrix construction</param>
    /// <returns cref="IDictionary{BPTag,Matrix}">Dictionary of double parametric matrices</returns>
    private static Dictionary<BPTag, Matrix<Complex>> DoubleParamGates(double phase)
    {
        return new Dictionary<BPTag, Matrix<Complex>>
        {
            { BPTag.RXX, Matrix<Complex>.Build.DenseOfArray(new [,] {
                {Complex.Cos(phase/2), Complex.Zero, Complex.Zero, -Complex.ImaginaryOne * Complex.Sin(phase/2)},
                {Complex.Zero, Complex.Cos(phase/2), -Complex.ImaginaryOne * Complex.Sin(phase/2), Complex.Zero},
                {Complex.Zero, -Complex.ImaginaryOne * Complex.Sin(phase/2), Complex.Cos(phase/2), Complex.Zero},
                {-Complex.ImaginaryOne * Complex.Sin(phase/2), Complex.Zero, Complex.Zero, Complex.Cos(phase/2)}
            }) },
            { BPTag.RYY, Matrix<Complex>.Build.DenseOfArray(new [,] {
                {Complex.Cos(phase/2), Complex.Zero, Complex.Zero, Complex.ImaginaryOne * Complex.Sin(phase/2)},
                {Complex.Zero, Complex.Cos(phase/2), -Complex.ImaginaryOne * Complex.Sin(phase/2), Complex.Zero},
                {Complex.Zero, -Complex.ImaginaryOne * Complex.Sin(phase/2), Complex.Cos(phase/2), Complex.Zero},
                {Complex.ImaginaryOne * Complex.Sin(phase/2), Complex.Zero, Complex.Zero, Complex.Cos(phase/2)}
            }) },
            { BPTag.RZZ, Matrix<Complex>.Build.DenseOfArray(new [,] {
                {Complex.Exp(-Complex.ImaginaryOne*phase/2), Complex.Zero, Complex.Zero, Complex.Zero},
                {Complex.Zero, Complex.Exp(Complex.ImaginaryOne*phase/2), Complex.Zero, Complex.Zero},
                {Complex.Zero, Complex.Zero, Complex.Exp(Complex.ImaginaryOne*phase/2), Complex.Zero},
                {Complex.Zero, Complex.Zero, Complex.Zero, Complex.Exp(-Complex.ImaginaryOne*phase/2)}
            }) }
            
        };
    }
    
    /// <summary>
    /// Factory method for creation of single qubit gate class
    /// </summary>
    /// <param name="token" cref="UTag">Unary tag determining the concrete gate</param>
    /// <param name="target">Index of qubit on which gate is applied</param>
    /// <returns cref="IMatrixGate">Created quantum gate object</returns>
    internal static IMatrixGate CreateGate(UTag token, int target)
    {
        return new SingleQubitGate(SingleGates[token], token, target);
    }
    
    /// <summary>
    /// Factory method for creation in single qubit parametric gate class 
    /// </summary>
    /// <param name="token" cref="PTag">Parametric tag determining concrete gate</param>
    /// <param name="target">Index of qubit on which gate is applied</param>
    /// <param name="phase">Tuple of value/string for the Θ phase argument</param>
    /// <returns cref="IMatrixGate">Created quantum gate object</returns>
    internal static IMatrixGate CreateGate(PTag token, int target, Tuple<double, string> phase)
    {
        return new ParamSingleGate(ParamGates(phase.Item1)[token], token, target, phase);
    }
    
    /// <summary>
    /// Factory method for creation of support blocks (gates without matrix representation)
    /// </summary>
    /// <param name="token" cref="SupportType">Token for the type of support</param>
    /// <param name="target">Index of qubit on which gate is applied</param>
    /// <param name="classic">Optional index of cbit on which measurement is recorded</param>
    /// <returns cref="ISupportGate">Created quantum support block</returns>
    internal static ISupportGate CreateGate(SupportType token, int target, int classic = -1)
    {
        return token switch
        {
            SupportType.None => new NoneGate(target),
            SupportType.Barrier => new BarrierGate(target),
            SupportType.Reset => new ResetGate(target),
            SupportType.Measure => new MeasureGate(target, classic),
            SupportType.PhaseDisk => new PhaseDisk(target),
        };
    }
    
    /// <summary>
    /// Factory method for creation of double qubit gate class
    /// </summary>
    /// <param name="token" cref="BTag">Binary tag determining concrete gate</param>
    /// <param name="target1">Index of first target qubit (often controlled)</param>
    /// <param name="target2">Index of second target qubit</param>
    /// <returns cref="IMatrixGate">Created quantum gate object</returns>
    internal static IMatrixGate CreateGate(BTag token, int target1, int target2)
    {
        return new DoubleQubitGate(DoubleGates[token], token, target1, target2);
    }
    
    /// <summary>
    /// Factory method for creation of double qubit parametric gate class
    /// </summary>
    /// <param name="token" cref="BPTag"></param>
    /// <param name="target1">Index of first target qubit</param>
    /// <param name="target2">Index of second target qubit</param>
    /// <param name="phase">Tuple of Θ phase argument value/string</param>
    /// <returns cref="IMatrixGate">Created quantum gate object</returns>
    internal static IMatrixGate CreateGate(BPTag token, int target1, int target2, Tuple<double, string> phase)
    {
        return new ParamDoubleGate(DoubleParamGates(phase.Item1)[token], token, target1, target2, phase);
    }
    
    /// <summary>
    /// Factory method for creation of Unitary gate
    /// </summary>
    /// <param name="args">Array of gate arguments(3 different phases)</param>
    /// <param name="target">Index of target qubit</param>
    /// <returns cref="IMatrixGate">Created quantum gate object</returns>
    internal static IMatrixGate CreateGate(Tuple<double, string>[] args, int target)
    {
        return new UnitaryGate(args, target);
    }
    
    /// <summary>
    /// Factory method for creation of Toffoli (CCX or CCNOT) gate
    /// </summary>
    /// <param name="target1">Index of first control qubit</param>
    /// <param name="target2">Index of second control qubit</param>
    /// <param name="target3">Index of target qubit</param>
    /// <returns cref="IMatrixGate">Created quantum gate object</returns>
    internal static IMatrixGate CreateGate(int target1, int target2, int target3)
    {
        return new ToffoliGate(target1, target2, target3);
    }
}