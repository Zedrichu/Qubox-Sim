using QuboxSimulator.Models.Gates;

namespace QuboxSimulator.Models.Circuits;

public class Circuit
{
    public Register Allocation { get; private set; }
    
    public List<Tower> GateGrid { get; private set; }

    public Circuit(Register allocation, List<Tower> gateGrid)
    {
        Allocation = allocation;
        GateGrid = gateGrid;
    }
    
    public void AddGate(IGate gate)
    {
        var tower = GateGrid.FirstOrDefault(t => t.AcceptGate(gate));
        if (tower == null)
        {
            tower = new Tower(Allocation.QubitNumber + 1);
            tower.AcceptGate(gate);
            GateGrid.Add(tower);
        }
    }
}