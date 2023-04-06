namespace QuboxSimulator.Gates;

public enum GateType
{
    Single, Param, 
    Double, DoubleParam,
    Toffoli, Unitary, Support,
}

public enum SupportType
{
    Barrier, Reset, Measure,
    None, PhaseDisk
}