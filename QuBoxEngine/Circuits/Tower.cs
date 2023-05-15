namespace QuBoxEngine.Circuits;
/* C#
 -*- coding: utf-8 -*-
Tower

Description: Modelling of the column a gates defined in a single circuit step.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 16/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/

using Gates;

/// <summary>
/// Class modelling the column of gates defined in a single circuit step.
/// </summary>
public class Tower
{
    /// <summary>
    /// Height of the tower.
    /// </summary>
    public int Height { get; }
 
    /// <summary>
    /// List of quantum gates contained in the tower.
    /// </summary>
    public List<IGate> Gates { get; }

    /// <summary>
    /// Tuple preserving the range of gate positions that are locked and cannot host new gates.
    /// </summary>
    public Tuple<int, int> Locked { get; set; } = new (-1, -1);

    /// <summary>
    /// Constructor of the tower object.
    /// </summary>
    /// <param name="height">Integer size of the column</param>
    public Tower(int height)
    {
        Height = height;
        Gates = new List<IGate>();
        for (var i = 0; i < height; i++)
        {
            Gates.Add(new NoneGate(i));
        }
    }
    
    /// <summary>
    /// Method to verify if the tower in question is empty. Empty is defined as containing only NONE placeholders.
    /// </summary>
    /// <returns cref="bool">Boolean value denoting the answer</returns>
    public bool IsEmpty()
    {
        return Gates.All(gate => gate.Id == "NONE");
    }
    
    
    /// <summary>
    /// Method to attempt the addition of a gate to the tower. If the gate cannot be added (not compatible with the tower) a false boolean flag is returned
    /// </summary>
    /// <param name="gate" cref="IGate">Generic quantum gate to be added to the tower if possible</param>
    /// <returns cref="bool">Boolean flag to reflect whether the operation was successful or not</returns>
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

    /// <summary>
    /// Lambda function to verify if a given gate is compatible with the tower i.e. if range of incoming gate is occupied only by NONE placeholders and the range is not locked for additions.
    /// </summary>
    private readonly Func<Tower, IGate, IGate, bool> _isCompatible = (t, gate, free) => 
        (t.Locked.Item1 > gate.TargetRange.Item2 || t.Locked.Item2 < gate.TargetRange.Item1) &&
        (free.Id == "NONE" || free.TargetRange.Item2 < gate.TargetRange.Item1 
                          || free.TargetRange.Item1 > gate.TargetRange.Item2);
    
    /// <summary>
    /// Lambda function to detect the placeholders to be substituted by the incoming gate for insertion.
    /// </summary>
    private readonly Func<IGate, IGate, bool> _isSubstitute = (gate, free) =>
        free.Id == "NONE" && (free.TargetRange.Item1 >= gate.TargetRange.Item1 &&
                              free.TargetRange.Item2 <= gate.TargetRange.Item2);
    
    /// <summary>
    /// Method to stringify the tower object in a meaningful way.
    /// </summary>
    /// <returns>String representation of the tower object</returns>
    public override string ToString()
    {
        return Gates.Aggregate("<", (current, gate) => gate.Id == "NONE"? 
            current : current + "_" + gate) + ">";
    }
}

