namespace QuboxSimulator.Models;

public class Tower
{
    public int Height { get; private set; } = 0;
    public List<IGate> Gates { get; private set; }

    public Tower(int height)
    {
        Height = height;
        Gates = new List<IGate>();
        for (int i = 0; i < height; i++)
        {
            Gates.Add(new NoneGate(i, i));
        }
    }
    
    public Tower(List<IGate> gates)
    {
        Height = gates.Count;
        Gates = gates;
    }
    
    public bool IsEmpty()
    {
        return Gates.All(gate => gate.Id == "None");
    }
    
    public bool AcceptGate(IGate gate)
    {   
        var compatible = Gates.All(g => _isCompatible(gate, g));
        
        if (compatible)
        {
            Gates.RemoveAll(g => _isCompatible(gate, g));
            Gates.Add(gate);
            return true;
        }
        return false;
    }

    private readonly Func<IGate, IGate, bool> _isCompatible = (gate, free) => 
        free.Id == "NONE" || free.TargetRange.Item2 < gate.TargetRange.Item1
                              || free.TargetRange.Item1 > gate.TargetRange.Item2;
    
    private readonly Func<IGate, IGate, bool> _isSubstitute = (gate, free) =>
        free.Id == "NONE" && (free.TargetRange.Item1 >= gate.TargetRange.Item1 &&
                              free.TargetRange.Item2 <= gate.TargetRange.Item2);
}

//IGate -> ISupportGate , IMatrixGate 
//IMatrixGate -> SingleQubitGate , MultiQubitGate, ParametricGate