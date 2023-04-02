namespace QuboxSimulator.Gates;

public enum GateType
{
    Single, Param, 
    Double, DoubleParam,
    Toffoli, Unitary, None,
    Barrier, Measure,
    Reset, PhaseDisk,
}