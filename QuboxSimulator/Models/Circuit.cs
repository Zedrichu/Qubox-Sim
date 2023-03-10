namespace QuboxSimulator.Models;

public class Circuit
{
    public Register Allocation { get; private set; }
    
    public List<Tower> GateGrid { get; private set; }

    public Circuit(Register allocation, List<Tower> gateGrid)
    {
        Allocation = allocation;
        GateGrid = gateGrid;
    }

}