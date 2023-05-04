using QuboxSimulator.Gates;

namespace QuboxSimulator.Circuits;

public class Circuit
{
    public Register Allocation { get; }
    
    public List<Tower> GateGrid { get; }

    internal Circuit(Register allocation)
    {
        Allocation = allocation;
        GateGrid = new List<Tower>{new (Allocation.QubitNumber + Allocation.CbitNumber)};
    }
    
    public void AddGate(IGate gate)
    {   
        var tower = GateGrid.FirstOrDefault(t => t.AcceptGate(gate));
        
        if (tower != null) return;
            
        
        tower = new Tower(Allocation.QubitNumber + Allocation.CbitNumber);
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

        if (gate.Type is GateType.Support && ((SupportGate) gate).SupportType is SupportType.Barrier)
        {
            foreach (var t in GateGrid)
            {
                t.Locked = new Tuple<int, int>(0, Allocation.QubitNumber + Allocation.CbitNumber-1);
                if (t == tower) break;
            }
        }
    }

    public override string ToString()
    {
        return Allocation + "\n" + GateGrid.Aggregate("", (current, tower) => current + "|" + tower )+" |";
    }
}