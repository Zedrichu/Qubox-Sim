using MathNet.Numerics.LinearAlgebra;
using Complex = System.Numerics.Complex;

namespace QuboxSimulator.Models.Gates;


public static class GateFactory
{
    private static readonly Dictionary<string, Matrix<Complex>> SingleGates = new()
    {
        {
            "H", Matrix<Complex>.Build.DenseOfArray(
                new Complex[,]
                {
                    { 1, 1 },
                    { 1, -1 }
                }) / Math.Sqrt(2)
        },
        { "I", Matrix<Complex>.Build.DenseIdentity(2, 2) },
        {
            "X", Matrix<Complex>.Build.DenseOfArray(
                new Complex[,]
                {
                    { 0, 1 },
                    { 1, 0 }
                })
        },
        {
            "Y", Matrix<Complex>.Build.DenseOfArray(
                new[,]
                {
                    { 0, -Complex.ImaginaryOne },
                    { Complex.ImaginaryOne, 0 }
                })
        },
        {
            "Z", Matrix<Complex>.Build.DenseOfArray(
                new Complex[,]
                {
                    { 1, 0 },
                    { 0, -1 }
                })
        },
        {
            "S", Matrix<Complex>.Build.DenseOfArray(
                new[,]
                {
                    { 1, 0 },
                    { 0, Complex.ImaginaryOne }
                })
        },
        {
            "SDG", Matrix<Complex>.Build.DenseOfArray(
                new[,]
                {
                    { 1, 0 },
                    { 0, -Complex.ImaginaryOne }
                })
        },
        {
            "T", Matrix<Complex>.Build.DenseOfArray(
                new[,]
                {
                    { 1, 0 },
                    { 0, Complex.Pow(Math.E, Complex.ImaginaryOne * Math.PI / 4) }
                })
        },
        {
            "TDG", Matrix<Complex>.Build.DenseOfArray(
                new[,]
                {
                    { 1, 0 },
                    { 0, Complex.Pow(Math.E, Complex.ImaginaryOne * -Math.PI / 4) }
                })
        },
        {
            "SX", Matrix<Complex>.Build.DenseOfArray(
                new[,]
                {
                    { 1 + Complex.ImaginaryOne, 1 - Complex.ImaginaryOne },
                    { 1 - Complex.ImaginaryOne, 1 + Complex.ImaginaryOne }
                }) / 2
        },
        {
            "SXDG", Matrix<Complex>.Build.DenseOfArray(
                new[,]
                {
                    { 1 - Complex.ImaginaryOne, 1 + Complex.ImaginaryOne },
                    { 1 + Complex.ImaginaryOne, 1 - Complex.ImaginaryOne }
                }) / 2
        },
    };

    private static Dictionary<string, Matrix<Complex>> ParamGates(double phase)
    {
        return new Dictionary<string, Matrix<Complex>>
        {
            { "P", Matrix<Complex>.Build.DenseOfArray(new[,]
            {
                {1,0},
                {0, Complex.Exp(Complex.ImaginaryOne * phase)}
            }) },
            {"RX", Matrix<Complex>.Build.DenseOfArray(new[,]
            {
                {Complex.Cos(phase / 2), -Complex.ImaginaryOne * Complex.Sin(phase / 2)},
                {-Complex.ImaginaryOne * Complex.Sin(phase / 2), Complex.Cos(phase / 2)}
            }) },
            {"RY", Matrix<Complex>.Build.DenseOfArray(new[,]
            {
                {Complex.Cos(phase / 2), -Complex.Sin(phase / 2)},
                {Complex.Sin(phase / 2), Complex.Cos(phase / 2)}
            }) },
            {"RZ", Matrix<Complex>.Build.DenseOfArray(new[,]
            {
                {Complex.Exp(-Complex.ImaginaryOne * phase / 2), 0},
                {0, Complex.Exp(Complex.ImaginaryOne * phase / 2)}
            }) }
            
        };
    }

    public static IMatrixGate GetSingleGate(string token, int target)
    {
        return new SingleQubitGate(SingleGates[token], target, token);
    }
    
    public static IMatrixGate GetParamGate(string token, int target, Tuple<double, string> phase)
    {
        return new ParamSingleGate(ParamGates(phase.Item1)[token], target, token, phase);
    }

    public static ISupportGate? GetSupportGate(string token, int target, int classic = -1)
    {
        return token switch
        {
            "NONE" => new NoneGate(target),
            "BARRIER" => new BarrierGate(target),
            "RESET" => new ResetGate(target),
            "MEASURE" => new MeasureGate(target, classic),
            "PHASEDISK" => new PhaseDisk(target),
            _ => null
        };
    }
    
    public static IMatrixGate? GetMultipleGate(string token, int target1, int target2,
                                    int target3 = -1, Tuple<double, string>? arg = null)
    {
        return token switch
        {
            "CNOT" => new CnotGate(target1, target2),
            "SWAP" => new SwapGate(target1, target2),
            "CCX" => new ToffoliGate(target1, target2, target3),
            "RZZ" => new RzzGate(target1, target2, arg),
            "RXX" => new RxxGate(target1, target2, arg),
            _ => null
        };
    }
    
    public static ParametricGate GetUnitaryGate(Tuple<double, string>[] args, int target)
    {
        return new UnitaryGate(args, target);
    }


}