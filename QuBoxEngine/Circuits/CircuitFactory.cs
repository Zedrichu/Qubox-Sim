using QuBoxEngine.Gates;

namespace QuBoxEngine.Circuits;
/* C#
 -*- coding: utf-8 -*-
CircuitFactory

Description: Factory for creating user-defined and configured circuits.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 24/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/

/// <summary>
/// Factory class handling the creation of user-defined and configured circuits.
/// </summary>
internal static class CircuitFactory
{
    /// <summary>
    /// Factory method to build a circuit from a given list of gates and formed register.
    /// </summary>
    /// <param name="gates">List of IGate objects to be placed on the circuit grid</param>
    /// <param name="allocation" cref="Register">Memory allocation object giving context to the circuit</param>
    /// <returns cref="Circuit">Built and configured circuit object</returns>
    internal static Circuit BuildCircuit(List<IGate> gates, Register allocation)
    {
        var circuit = new Circuit(allocation);
        // Iterate through the list of gates and add them one by one to the circuit grid
        foreach (var gate in gates)
        {
            circuit.AddGate(gate);
        }

        return circuit;
    }
}

internal enum CircuitType
{
    CSHS, 
    BellState,
    GHZState,
    WState,
}