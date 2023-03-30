using QuboxSimulator.Models.Gates;

namespace QuboxSimulator.Models.Circuits;

public class Circuit
{
    public Register Allocation { get; private set; }
    
    public List<Tower> GateGrid { get; private set; }

    public Circuit(Register allocation)
    {
        Allocation = allocation;
        GateGrid = new List<Tower>{new (Allocation.QubitNumber + 1)};
    }
    
    public void AddGate(IGate gate)
    {   
        var tower = GateGrid.FirstOrDefault(t => t.AcceptGate(gate));
        
        if (tower != null) return;
            
        
        tower = new Tower(Allocation.QubitNumber + 1);
        tower.AcceptGate(gate);
        GateGrid.Add(tower);
        
        if (gate.Id is "CNOT" or "CCX" or "RZZ" or "RXX" or "SWAP")
        {
            foreach (var t in GateGrid)
            {
                t.Locked = gate.TargetRange;
                if (t == tower) break;
            } 
        }

        if (gate.Id is "Barrier")
        {
            foreach (var t in GateGrid)
            {
                t.Locked = new Tuple<int, int>(0, Allocation.QubitNumber);
                if (t == tower) break;
            }
        }
    }

    public override string ToString()
    {
        return Allocation + GateGrid.Aggregate("", (current, tower) => current + "|" + tower )+" |";
    }
}