using QuboxSimulator.Gates;

namespace QuboxSimulator.Circuits;
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


internal static class CircuitFactory
{
    internal static Circuit BuildCircuit(List<IGate> gates, Register allocation)
    {
        var circuit = new Circuit(allocation);
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