namespace QuboxSimulator.Models;
/* C#
 -*- coding: utf-8 -*-
IGate Interface

Description: Declaration of the interface for various types of gates.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 13/03/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/


public interface IGate
{
    public abstract Tuple<int, int> TargetRange { get; set; }

    public abstract double[][] Matrix { get; set; }
    
    public abstract string Condition { get; set; }
    
    public abstract string Id { get; set; }
}

public class NoneGate : IGate
{
    public Tuple<int, int> TargetRange { get; set; }
    public double[][] Matrix { get; set; } = new double[2][]
    {
        new double[2] {1, 0},
        new double[2] {0, 1},
    };
    public string Condition { get; set; } = "NONE";
    
    public string Id { get; set; } = "NONE";
    
    public NoneGate(int start, int end)
    {
        TargetRange = new Tuple<int, int>(start, end);
    }
}