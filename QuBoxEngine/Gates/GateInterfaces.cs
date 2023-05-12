using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
namespace QuBoxEngine.Gates;

/* C#
 -*- coding: utf-8 -*-
Gate Interface Hierarchy

Description: Declaration of the generic interface of gates and support gate interface.
Includes the declaration of the concrete support gates, supported by the simulator.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 13/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/

/// <summary>
/// Interface for generic quantum gates.
/// </summary>
public interface IGate
{
    public Tuple<int, int> TargetRange { get; }

    public string? Condition { get; set; }
    
    public string Id { get; }

    public GateType Type { get; }
}

/// <summary>
/// Interface for support quantum gates (barrier, reset, measurement)
/// </summary>
public interface ISupportGate: IGate
{
    public SupportType SupportType { get; }
    public void SupportState(State state);
}

public interface IMatrixGate : IGate
{
    public Matrix<Complex> Matrix { get; }
}