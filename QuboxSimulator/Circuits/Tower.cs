using QuboxSimulator.Gates;

namespace QuboxSimulator.Circuits;

public class Tower
{
    public int Height { get; }
    public List<IGate> Gates { get; }

    public Tuple<int, int> Locked { get; set; } = new (-1, -1);

    public Tower(int height)
    {
        Height = height;
        Gates = new List<IGate>();
        for (var i = 0; i < height; i++)
        {
            Gates.Add(new NoneGate(i));
        }
    }

    public bool IsEmpty()
    {
        return Gates.All(gate => gate.Id == "NONE");
    }
    
    public bool AcceptGate(IGate gate)
    {   
        var compatible = Gates.All(g => _isCompatible(this, gate, g));
        
        if (compatible)
        {
            Gates.RemoveAll(g => _isSubstitute(gate, g));
            Gates.Add(gate);
            Gates.Sort((x, y) => x.TargetRange.Item1.CompareTo(y.TargetRange.Item1));
            return true;
        }
        return false;
    }


    private readonly Func<Tower, IGate, IGate, bool> _isCompatible = (t, gate, free) => 
        (t.Locked.Item1 > gate.TargetRange.Item2 || t.Locked.Item2 < gate.TargetRange.Item1) &&
        (free.Id == "NONE" || free.TargetRange.Item2 < gate.TargetRange.Item1 
                          || free.TargetRange.Item1 > gate.TargetRange.Item2);
    
    private readonly Func<IGate, IGate, bool> _isSubstitute = (gate, free) =>
        free.Id == "NONE" && (free.TargetRange.Item1 >= gate.TargetRange.Item1 &&
                              free.TargetRange.Item2 <= gate.TargetRange.Item2);

    public override string ToString()
    {
        return Gates.Aggregate("<", (current, gate) => gate.Id == "NONE"? 
            current : current + "_" + gate) + ">";
    }
}

