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


public interface IGate
{
    public abstract Tuple<int, int> TargetRange { get; set; }

    public abstract string? Condition { get; set; }
    
    public abstract string Id { get; set; }
}

public interface ISupportGate: IGate
{
    
}

public class NoneGate : ISupportGate
{
    public Tuple<int, int> TargetRange { get; set; }

    public string? Condition { get; set; } = null;
    public string Id { get; set; } = "NONE";
    
    public NoneGate(int target)
    {
        TargetRange = new Tuple<int, int>(target, target);
    }
}

public class BarrierGate : ISupportGate
{
    public Tuple<int, int> TargetRange { get; set; }

    public string? Condition { get; set; } = null;
    public string Id { get; set; } = "BARRIER";
    
    public BarrierGate(int target)
    {
        TargetRange = new Tuple<int, int>(target, target);
    }
}

public class ResetGate : ISupportGate
{
    public Tuple<int, int> TargetRange { get; set; }
    
    public string? Condition { get; set; } = null;
    
    public string Id { get; set; } = "RESET";
    
    public ResetGate(int target)
    {
        TargetRange = new Tuple<int, int>(target, target);
    }
}

public class MeasureGate : ISupportGate
{
    public Tuple<int, int> TargetRange { get; set; }
    
    public string? Condition { get; set; } = null;
    
    public string Id { get; set; } = "MEASURE";
    
    public MeasureGate(int quantum, int classic)
    {
        TargetRange = new Tuple<int, int>(quantum, classic);
    }
}

public class PhaseDisk : ISupportGate
{
    public Tuple<int, int> TargetRange { get; set; }
    
    public string? Condition { get; set; } = null;
    
    public string Id { get; set; } = "PHASEDISK";
    
    public PhaseDisk(int qubitNo)
    {
        TargetRange = new Tuple<int, int>(0, qubitNo);
    }
}