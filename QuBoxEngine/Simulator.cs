namespace QuBoxEngine;
/* C#
 -*- coding: utf-8 -*-
Simulator Engine

Description: Declaration of the simulator engine module that can run circuit evolutions

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 06/04/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/

using MathNet.Numerics.LinearAlgebra;
using Complex = System.Numerics.Complex;
using Circuits;
using Gates;

/// <summary>
/// Class that represents the simulator engine
/// </summary>
public class Simulator
{
    private readonly Circuit _circuit;
    private readonly int _qubits;

    private bool nom = true;
    
    private State _state;
    public List<Tuple<double, double>> PhaseDisks { get; } = new ();

    /// <summary>
    /// Constructor of a simulator engine for a specific circuit
    /// </summary>
    /// <param name="circuit" cref="Circuit">Target circuit on which simulation will be performed</param>
    public Simulator(Circuit circuit)
    {
        _circuit = circuit;
        _qubits = _circuit.Allocation.QubitNumber;
        
        // Initialize the state vector and the probability vector
        var probability = Vector<double>.Build.
                Dense(0b1 << circuit.Allocation.CbitNumber, 0);
        probability[0] = 1;
        var stateVector = Vector<Complex>.Build.Dense(0b1 << _qubits, 0);
        stateVector[0] = 1;
        // Initialize the state object with given vectors
        _state = new State(stateVector, probability);
    }
    
    /// <summary>
    /// Executes a step of the circuit simulation by constructing the tensor product of gates in a tower. Global operation is applied on the quantum system state.
    /// </summary>
    /// <param name="tower" cref="Tower">Target column of gates in the circuit</param>
    private void TowerOperation(Tower tower)
    {
        var tensor = Matrix<Complex>.Build.DiagonalIdentity(1);
        foreach (var gate in  tower.Gates)
        {
            if (gate.TargetRange.Item1 >= _qubits) break;
            var matrix = Matrix<Complex>.Build.DenseIdentity(
                (gate.TargetRange.Item2 - gate.TargetRange.Item1 + 1) * 2);
            if (gate.Type == GateType.Support)
            {
                var supportGate = (ISupportGate) gate;
                var tempResult = _state.ProbeVector;
                supportGate.SupportState(_state);
                if (supportGate.SupportType is SupportType.Measure)
                {
                    matrix = Matrix<Complex>.Build.DenseIdentity((int) Math.Pow(2, _qubits - gate.TargetRange.Item1));
                    nom = false;
                }

                if (supportGate.SupportType is SupportType.PhaseDisk)
                {
                    matrix = Matrix<Complex>.Build.DenseIdentity((_qubits - gate.TargetRange.Item1) * 2);

                    for (var i = 0; i < _state.ProbeVector.Count; i+=2 )
                    {
                        PhaseDisks.Add(new Tuple<double, double>(
                            _state.ProbeVector[i], _state.ProbeVector[i + 1]));   
                    }
                    _state.ProbeVector = tempResult;
                }
            }
            else
            {
                matrix = ((IMatrixGate) gate).Matrix;
            }
            tensor = tensor.KroneckerProduct(matrix);
        }
        
        _state.StateVector = tensor * _state.StateVector;
    }
    
    /// <summary>
    /// Method to trigger/run the simulation of the circuit registered in the simulator engine
    /// </summary>
    /// <returns cref="State">Global state after circuit unitary evolution</returns>
    public State Run()
    {
        foreach (var tower in _circuit.GateGrid)
        {
            if (!tower.IsEmpty())
                TowerOperation(tower);
        }

        if (nom)
        {
            var temp = Vector<Complex>.Build.Dense(_state.StateVector.Count);
            _state.StateVector.PointwisePower(2, temp);
            _state.ProbeVector = temp.Real().PointwiseAbs();
        }

        return _state;
    }
    
}