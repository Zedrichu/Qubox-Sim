using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Complex = System.Numerics.Complex;

namespace QuBoxEngine;

public class State
{
    // Dimension of the Hilbert space
    private readonly int _dimH;
    // Dimension of measurement vector space (real registers)
    private readonly int _dimM;
    
    // State vector in the Hilbert space
    public Vector<Complex> StateVector { get; set; }
    // Result vector in measurement vector space
    public Vector<double> ProbeVector { get; set; }
    
    public State(Vector<Complex> stateVector, Vector<double> probeVector)
    {
        // Get the dimensions from the input vector lengths (logarithmic)
        _dimH = (int) Math.Log2(stateVector.Count);
        _dimM = (int) Math.Log2(probeVector.Count);
        // Set the properties
        StateVector = stateVector;
        ProbeVector = probeVector;
    }
    
    public void ResetQubit(int qubit)
    {
        
        // Bit mask for the qubit to be reset
        var mask = 0b1 << (_dimH - qubit - 1);

        // Iterate over components of the state vector
        for (var i = 0; i < StateVector.Count; i++) 
        {
            // Find the components that have the qubit set to |1>
            if ((i & mask) != 0) 
            {
                // Set probability amplitude of component to 0
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
        // Iterate over state vector components
        for (var i = 0; i < StateVector.Count; i++)
        {
            // Components with |1> in the qubit position
            if ((i & mask) != 0) // evil floating point bit level hacking
            {
                // Compute probability of state using Born rule on amplitude
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
            // Components with |0> in the qubit position
            if ((i & mask) == 0)
            {
                // Compute the scaled magnitude (normalization)
                var mag = Complex.Pow(StateVector[i].Magnitude, 2);
                var scaledMagnitude = mag / (1 - pnc);
                // Set the magnitude to previous value if it never clicks (p = 0)
                if (scaledMagnitude.IsNaN()) scaledMagnitude = mag;
                // Retrieve the original phase and keep it in the final normalized state
                var phase = StateVector[i].Phase;
                StateVector[i] = Complex.Sqrt(scaledMagnitude) * Complex.Exp(Complex.ImaginaryOne * phase);
            }
        }

        // Calculate the probabilities of each measurement outcome
        for (var i = 0; i < ProbeVector.Count; i++)
        {   
            // Get components with |0> in the cbit position
            if ((i & cMask) == 0)
            {
                // Set the |1> component to probability of measuring |1>
                // multiplied with previous value of the |0> component
                ProbeVector[cMask + i] = Math.Abs(ProbeVector[i] * pnc);
                // Scale the |0> component by the probability of measuring |0>
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
                // evil floating point bit level hacking
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
                // evil floating point bit level hacking
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