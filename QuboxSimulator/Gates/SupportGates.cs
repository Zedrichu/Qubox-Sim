using MathNet.Numerics.LinearAlgebra;
using Complex = System.Numerics.Complex;

namespace QuboxSimulator.Gates;


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

    public virtual void SupportState(State state)
    {
    }
}

/// <summary>
/// Gate placeholder in the circuit grid (no operation)
/// </summary>
internal class NoneGate : SupportGate
{
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
    public ResetGate(int target)
    {
        Id = "RESET";
        SupportType = SupportType.Reset;
        TargetRange = new Tuple<int, int>(target, target);
    }
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
    public MeasureGate(int quantum, int classic)
    {
        Id = "MEASURE";
        SupportType = SupportType.Measure;
        TargetRange = new Tuple<int, int>(quantum, classic);
    }
    
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
    public PhaseDisk(int qubitNo)
    {
        Id = "PHASEDISK";
        SupportType = SupportType.PhaseDisk;
        TargetRange = new Tuple<int, int>(0, qubitNo);
    }

    public override void SupportState(State state)
    {
        state.TrackPhase();
    }
}