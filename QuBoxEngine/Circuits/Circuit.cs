namespace QuBoxEngine.Circuits;
/* C#
 -*- coding: utf-8 -*-
Circuit

Description: Module implementing the high-level circuit object.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 15/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/

using Gates;

/// <summary>
/// Class modelling the information contained in the quantum circuit.
/// </summary>
public class Circuit
{
    /// <summary>
    /// Memory allocation object containing information about declared registers and variables.
    /// </summary>
    public Register Allocation { get; }
    
    /// <summary>
    /// Grid of gates represented as a list of gate columns called towers.
    /// </summary>
    public List<Tower> GateGrid { get; }
    
    
    /// <summary>
    /// Constructor of circuit object from a register (memory). By default the circuit includes only gate placeholders.
    /// </summary>
    /// <param name="allocation" cref="Register">Memory allocation formed during compilation</param>
    internal Circuit(Register allocation)
    {
        Allocation = allocation;
        GateGrid = new List<Tower>{new (Allocation.QubitNumber + Allocation.CbitNumber)};
    }
    
    /// <summary>
    /// Method for adding a new gate to the circuit.
    /// </summary>
    /// <param name="gate" cref="IGate">Gate to be included in the circuit grid</param>
    public void AddGate(IGate gate)
    {   
        var tower = GateGrid.FirstOrDefault(t => t.AcceptGate(gate));

        if (tower == null) {
            tower = new Tower(Allocation.QubitNumber + Allocation.CbitNumber);
            tower.AcceptGate(gate);
            GateGrid.Add(tower);
        }

        if (gate.Type is GateType.Double or GateType.Toffoli or GateType.DoubleParam)
        {
            foreach (var t in GateGrid)
            {
                t.Locked = gate.TargetRange;
                if (t == tower) break;
            } 
        }

        if (gate.Type is GateType.Support &&
            ((SupportGate) gate).SupportType is SupportType.Barrier)
        {
            foreach (var t in GateGrid)
            {
                t.Locked = new Tuple<int, int>(0, Allocation.QubitNumber + Allocation.CbitNumber-1);
                if (t == tower) break;
            }
        }
    }
    
    
    /// <summary>
    /// Overriden method to stringify the circuit object in a meaningful way for debugging.
    /// </summary>
    /// <returns>String representation of circuit object</returns>
    public override string ToString()
    {
        return Allocation + "\n" + GateGrid.Aggregate("", (current, tower) => current + "|" + tower )+" |";
    }
}