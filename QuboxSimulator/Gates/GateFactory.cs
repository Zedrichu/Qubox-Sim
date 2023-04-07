using MathNet.Numerics.LinearAlgebra;
using Complex = System.Numerics.Complex;
using static QuantumLanguage.Tags;

namespace QuboxSimulator.Gates;

public static class GateFactory
{
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
        { BTag.CH, Matrix<Complex>.Build.DenseOfArray(
            new [,] {
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, Complex.ImaginaryOne }
            })},
        { BTag.CS, Matrix<Complex>.Build.DenseOfArray(
            new [,] {
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 1/Complex.Sqrt(2), 1/Complex.Sqrt(2) },
                { 0, 0, 1/Complex.Sqrt(2), -1/Complex.Sqrt(2) }
            })},
    };
    
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

    public static IMatrixGate CreateGate(UTag token, int target)
    {
        return new SingleQubitGate(SingleGates[token], token, target);
    }
    
    public static ParametricGate CreateGate(PTag token, int target, Tuple<double, string> phase)
    {
        return new ParamSingleGate(ParamGates(phase.Item1)[token], token, target, phase);
    }

    public static ISupportGate CreateGate(SupportType token, int target, int classic = -1)
    {
        return token switch
        {
            SupportType.None => new NoneGate(target),
            SupportType.Barrier => new BarrierGate(target),
            SupportType.Reset => new ResetGate(target),
            SupportType.Measure => new MeasureGate(target, classic),
            SupportType.PhaseDisk => new PhaseDisk(target),
            _ => throw new ArgumentOutOfRangeException(nameof(token), token, null)
        };
    }
    
    public static IMatrixGate CreateGate(BTag token, int target1, int target2)
    {
        return new DoubleQubitGate(DoubleGates[token], token, target1, target2);
    }
    
    public static IMatrixGate CreateGate(BPTag token, int target1, int target2, Tuple<double, string> phase)
    {
        return new ParamDoubleGate(DoubleParamGates(phase.Item1)[token], token, target1, target2, phase);
    }
    
    public static IMatrixGate CreateGate(Tuple<double, string>[] args, int target)
    {
        return new UnitaryGate(args, target);
    }
    
    public static IMatrixGate CreateGate(int target1, int target2, int target3)
    {
        return new ToffoliGate(target1, target2, target3);
    }

    public static ISupportGate CreatePlaceholder()
    {
        return new NoneGate(0);
    }
}