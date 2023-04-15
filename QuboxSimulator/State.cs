using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Complex = System.Numerics.Complex;

namespace QuboxSimulator;

public class State
{
    // Dimension of the Hilbert space
    private readonly int _dimH;
    // Dimension of measurement register
    private readonly int _dimM;
    
    public Vector<Complex> StateVector { get; set; }
    public Vector<double> ProbeVector { get; set; }
    
    public State(Vector<Complex> stateVector, Vector<double> probeVector)
    {
        _dimH = (int) Math.Log2(stateVector.Count);
        _dimM = (int) Math.Log2(probeVector.Count);
        StateVector = stateVector;
        ProbeVector = probeVector;
    }
    
    public void ResetQubit(int qubit)
    {
        
        // Bit mask for the qubit to be reset
        var mask = 0b1 << (_dimH - qubit - 1);

        for (var i = 0; i < StateVector.Count; i++)
        {
            if ((i & mask) != 0) 
            {
                StateVector[i] = 0;
            }            
        }
    }

    public void Measure(int qubit, int cbit)
    {
        // Bit mask for the qubit to be measured
        var mask = 0b1 << (_dimH - qubit - 1);
        
        // Bit mask for the cbit to be measured
        var cMask = 0b1 << (_dimM - cbit + _dimH - 1);
        
        // The probability of the qubit to be measured as |1> - no click
        var pnc = 0.0;
        for (var i = 0; i < StateVector.Count; i++)
        {
            if ((i & mask) != 0) 
            {
                // Compute probability of state using Born rule
                var p = Math.Pow(StateVector[i].Magnitude, 2);
                // Add up the measuring probability
                pnc += p;
                // Reset the component if it never clicks (p = 1)
                if (Math.Abs(1-p) > 0.000001 ) StateVector[i] = 0;
            }            
        }

        // Update state vector - scaling up magnitudes by 1/(1-p)
        for (var i = 0; i < StateVector.Count; i++)
        {
            if ((i & mask) == 0)
            {
                var mag = Complex.Pow(StateVector[i].Magnitude, 2);
                var scaledMagnitude = mag / (1 - pnc);
                if (scaledMagnitude.IsNaN()) scaledMagnitude = mag;
                var phase = StateVector[i].Phase;
                StateVector[i] = Complex.Sqrt(scaledMagnitude) * Complex.Exp(Complex.ImaginaryOne * phase);
            }
        }

        // Calculate the probabilities of each measurement outcome
        for (var i = 0; i < ProbeVector.Count; i++)
        {
            if ((i & cMask) == 0)
            {
                ProbeVector[cMask + i] = Math.Abs(ProbeVector[i] * pnc);
                ProbeVector[i] *= Math.Abs(1 - pnc);
            }
        }
    }
    
    public void TrackPhase()
    {
        var phaser = Vector<double>.Build.Dense((_dimH + _dimM) * 2, 0);
        
        for (var i = 0; i < StateVector.Count; i++)
        {
            for (var j = 0; j < _dimH; j++)
            {
                if ((i & (0b1 << j)) != 0)
                {
                    phaser[j+1] += Math.Pow(StateVector[i].Magnitude, 2);
                }
                else
                {
                    phaser[j] += Math.Pow(StateVector[i].Magnitude, 2);
                }
            }
        }
        
        for (var i = 0; i < ProbeVector.Count; i++)
        {
            for (var j = 0; j < _dimM; j++)
            {
                if ((i & (0b1 << j)) != 0)
                {
                    phaser[2*_dimH + j + 1] += ProbeVector[i];
                }
                else
                {
                    phaser[2*_dimH + j + 0] += ProbeVector[i];
                }
            }
        }

        ProbeVector = phaser;
    }
}