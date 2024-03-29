namespace QuBoxEngine;
/* C#
 -*- coding: utf-8 -*-
QuBoxEngineDemo

Description:  Demo application for QuBoxEngine library

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 14/04/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> TEST
*/

using Circuits;
public class ConsoleDemo
{
    // Main method for the demo application
    public static void Main(string[] args)
    {
        // Testing Interpreter
        var quLangCode = Console.ReadLine();
        Interpreter.HandleLang(quLangCode);
        var circuit = Interpreter.Interpret();
        Console.WriteLine(circuit);
        
        // Testing Generator
        if (circuit == null) return;
        var generator = new Generator(circuit);
        var ast = generator.DestructCircuit();
        Console.WriteLine(ast.Item1);
        Console.WriteLine(ast.Item2);
        
        // Testing SVG Provider
        SvgProvider.DrawCircuitSvg("grid.svg", circuit);
        
        // Testing Simulator
        var simulator = new Simulator(circuit);
        Console.WriteLine(simulator.Run());
        
    }
 }