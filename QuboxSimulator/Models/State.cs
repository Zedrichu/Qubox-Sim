/* C#
 -*- coding: utf-8 -*-
Global State

Description: Class defining the global state of the quantum system

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 15/02/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/
namespace Qubox_Simulator.Models;

public class State
{
    public int QubitNumber { get; private set; } = 1;
    public int CbitNumber { get; private set; } = 1;
    
    
}