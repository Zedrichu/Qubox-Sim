namespace QuBoxEngine.Gates;
/* C#
 -*- coding: utf-8 -*-
SupportGates

Description: Module implementing the support gates (commands) for the quantum circuit. Included commands are
            the measurement of quantum bits, reset of quantum state, barrier for optimization and phase disk tracker.
@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 18/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/


/// <summary>
/// Abstract class implementing the common fields of all support gates. Extends the ISupportGate interface.
/// </summary>
internal abstract class SupportGate : ISupportGate
{
    public Tuple<int, int> TargetRange { get; set; }
    
    public string? Condition { get; set; }
    
    public string Id { get; set; }
    
    public SupportType SupportType { get; set; }

    public GateType Type { get;} = GateType.Support;
    
    public override string ToString()
    {
        return $"Gate:{Id} Target: {TargetRange}";
    }
    
    /// <summary>
    /// Abstract method for supporting a given state with the gate. By default the state is unchanged.
    /// </summary>
    /// <param name="state" cref="State">Currently used state object</param>
    public virtual void SupportState(State state)
    {
    }
}

/// <summary>
/// Gate placeholder in the circuit grid (no operation)
/// </summary>
internal class NoneGate : SupportGate
{
    /// <summary>
    /// Constructor for the placeholder gate
    /// </summary>
    /// <param name="target">Index of target qubit</param>
    public NoneGate(int target)
    {
        Id = "NONE";
        SupportType = SupportType.None;
        TargetRange = new Tuple<int, int>(target, target);
    }
}

/// <summary>
/// Barrier for gate optimization triggers
/// </summary>
internal class BarrierGate : SupportGate
{
    /// <summary>
    /// Constructor for Barrier gate
    /// </summary>
    /// <param name="target">Index of affected qubit</param>
    public BarrierGate(int target)
    {
        Id = "BARRIER";
        SupportType = SupportType.Barrier;
        TargetRange = new Tuple<int, int>(target, target);
    }
}

/// <summary>
/// Qubit initializer to computational state |0> = (1,0)
/// </summary>
internal class ResetGate : SupportGate
{
    /// <summary>
    /// Constructor of a reset gate
    /// </summary>
    /// <param name="target">Index of qubit for which quantum state is reset to |0> </param>
    public ResetGate(int target)
    {
        Id = "RESET";
        SupportType = SupportType.Reset;
        TargetRange = new Tuple<int, int>(target, target);
    }
    
    /// <summary>
    /// Overriden implementation of supporting given state with a reset
    /// </summary>
    /// <param name="state" cref="State">Currently used state object</param>
    public override void SupportState(State state)
    {
        state.ResetQubit(TargetRange.Item1);
    }
}

/// <summary>
/// Measurement gate from quantum to classical register
/// </summary>
internal class MeasureGate : SupportGate
{   
    /// <summary>
    /// Constructor of a measurement gate
    /// </summary>
    /// <param name="quantum">Index of qubit from which state probability is recorded</param>
    /// <param name="classic">Index of classical bit to store the outcome probability</param>
    public MeasureGate(int quantum, int classic)
    {
        Id = "MEASURE";
        SupportType = SupportType.Measure;
        TargetRange = new Tuple<int, int>(quantum, classic);
    }
    
    /// <summary>
    /// Overriden implementation of performing the state measurement
    /// </summary>
    /// <param name="state" cref="State">Currently used state object</param>
    public override void SupportState(State state)
    {
        state.Measure(TargetRange.Item1, TargetRange.Item2);
    }
}

/// <summary>
/// Visualizer for the phase distribution at any point in the circuit.
/// </summary>
internal class PhaseDisk : SupportGate
{
    /// <summary>
    /// Constructor of a phase disk tracker
    /// </summary>
    /// <param name="qubitNo">Number of qubits in the circuit(system)</param>
    public PhaseDisk(int qubitNo)
    {
        Id = "PHASEDISK";
        SupportType = SupportType.PhaseDisk;
        TargetRange = new Tuple<int, int>(0, qubitNo);
    }
    
    /// <summary>
    /// Overriden implementation of performing the phase disk tracking
    /// </summary>
    /// <param name="state">Currently used state object</param>
    public override void SupportState(State state)
    {
        state.TrackPhase();
    }
}