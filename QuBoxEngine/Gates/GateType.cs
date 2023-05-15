namespace QuBoxEngine.Gates;
/* C#
 -*- coding: utf-8 -*-
GateType

Description: Definition of enumeration types aiding the hierarchy of quantum gates.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 13/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/


/// <summary>
/// Enumeration type of the supported quantum gate classes.
/// </summary>
public enum GateType
{
    Single, Param, 
    Double, DoubleParam,
    Toffoli, Unitary, Support,
}

/// <summary>
/// Enumeration type of types of support commands
/// </summary>
public enum SupportType
{
    Barrier, Reset, Measure,
    None, PhaseDisk
}