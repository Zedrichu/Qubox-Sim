using MathNet.Numerics.LinearAlgebra;
using Complex = System.Numerics.Complex;

namespace QuboxSimulator;
using Circuits;
using Gates;

public class Simulator
{
    private readonly Circuit _circuit;
    private readonly int _matrixSize;
    private readonly int _qubits;
    private Dictionary<int, Tuple<double, double>> _results = new();
    public Simulator(Circuit circuit)
    {
        _circuit = circuit;
        _qubits = circuit.Allocation.QubitNumber;
        _matrixSize = (int) Math.Pow(2, _qubits);
    }
    
    private Vector<Complex> TowerOperation(Tower tower, Vector<Complex> stateVector)
    {
        var tensor = Matrix<Complex>.Build.DiagonalIdentity(1);
        foreach (var gate in  tower.Gates)
        {
            if (gate.TargetRange.Item1 >= _qubits) break;
            var matrix = Matrix<Complex>.Build.DenseIdentity(2);
            if (gate.Type == GateType.Support)
            {
                var supportGate = (ISupportGate) gate;
                stateVector = supportGate.SupportState(stateVector, _results);
            }
            else
            {
                matrix = ((IMatrixGate) gate).Matrix;
            }
            tensor = tensor.KroneckerProduct(matrix);
        }
        
        return tensor * stateVector;
    }
    
    public Vector<Complex> Run()
    {
        var vector = Vector<Complex>.Build.Dense(_matrixSize, 0);
        vector[0] = 1;

        foreach (var tower in _circuit.GateGrid)
        {
            vector = TowerOperation(tower, vector);
        }

        return vector.PointwisePower(2);
    }
    
    
    

}