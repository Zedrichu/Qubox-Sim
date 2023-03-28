namespace QuboxSimulator.Models.Gates;
/* C#
 -*- coding: utf-8 -*-
Gate Interface Hierarchy

Description: Declaration of the generic interface of gates and support gate interface.
Includes the declaration of the concrete support gates, supported by the simulator.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 13/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/

/// <summary>
/// Interface for generic quantum gates.
/// </summary>
public interface IGate
{
    public abstract Tuple<int, int> TargetRange { get; set; }

    public abstract string? Condition { get; set; }
    
    public abstract string Id { get; set; }
    
    

}

/// <summary>
/// Interface for support quantum gates (barrier, reset, measurement)
/// </summary>
public interface ISupportGate: IGate
{
    
}

internal abstract class SupportGate : ISupportGate
{
    public Tuple<int, int> TargetRange { get; set; }
    
    public string? Condition { get; set; }
    
    public string Id { get; set; }
    
    public override string ToString()
    {
        return $"Gate:{Id} Target: {TargetRange}";
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
        TargetRange = new Tuple<int, int>(target, target);
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
        TargetRange = new Tuple<int, int>(quantum, classic);
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
        TargetRange = new Tuple<int, int>(0, qubitNo);
    }
}