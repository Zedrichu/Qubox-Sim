namespace QuBoxEngine;
/* C#
 -*- coding: utf-8 -*-
SVGProvider

Description: Module implementing the generation of SVG visualisations 
                resembling the circuit schematic in a file at given path

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 15/05/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 
@__Status --> DEV
*/

using Gates;
using Circuits;
using SkiaSharp;

/// <summary>
/// Static class to encapsulate the entire logic of SVG figure generation
/// </summary>
public static class SvgProvider
{
    
    // Definition of constants used in the drawing process
    private const int InterGatePadding = 20;
    private const int VerticalOffset = 10;
    private const int HorizontalOffset = 10;
    private const int LineOffset = 30;
    private const int InterTowerPadding = 30;
    
    /// <summary>
    /// Method to draw a box shape with a text identifier in the middle
    /// </summary>
    /// <param name="svg" cref="SKCanvas">SVG canvas on which figure is drawn</param>
    /// <param name="x">X-coordinate of region for the shape</param>
    /// <param name="y">Y-coordinate of region for the shape</param>
    /// <param name="width">Width of the shape</param>
    /// <param name="height">Height of the shape</param>
    /// <param name="id">Text identifier to pe placed on box</param>
    private static void DrawBox(SKCanvas svg, int x, int y, int width, int height, string id)
    {
        var redPaint = new SKPaint // Definition of red paint
        {
            Color = new SKColor(141,11,61),
            StrokeWidth = 3,
            IsAntialias = true,
        };
        
        var whitePaint = new SKPaint // Definition of white paint
        {
            Color = SKColors.White,
            StrokeWidth = 3,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true,
        };
        
        // Create rectangle with rounded corners in the input region
        var rect = new SKRoundRect(SKRect.Create(x, y, width, height), 10);
        svg.DrawRoundRect(rect, redPaint);
        // Place info text in the middle of the box
        svg.DrawText(id, x+width/2, y+height/2, 
            new SKFont(SKTypeface.FromFile("cascadiaFont.otf"), 18F), whitePaint);
    }
    
    /// <summary>
    /// Method to draw a tower of gates in the defined region
    /// </summary>
    /// <param name="svg" cref="SKCanvas">SVG canvas to draw on</param>
    /// <param name="x">X-coordinate of region for tower</param>
    /// <param name="y">Y-coordinate of region for tower</param>
    /// <param name="height">Height of the tower structure</param>
    /// <param name="width">Width of the tower structure</param>
    /// <param name="tower" cref="Tower">Tower object to be represented on SVG</param>
    private static void DrawTower(SKCanvas svg, int x, int y, int height, int width, Tower tower)
    {
        var grayPaint = new SKPaint // Definition of gray paint
        {
            Color = new SKColor(219, 199,175,70),
            StrokeWidth = 3,
            IsAntialias = true,
        };
        
        var vioPaint = new SKPaint // Definition of violet paint for the wires
        {
            Color = SKColors.BlueViolet,
            StrokeWidth = 3,
            StrokeCap = SKStrokeCap.Round,
            Style = SKPaintStyle.Stroke
        };

        var rect = new SKRoundRect(SKRect.Create(x, y, width, height), 10);
        svg.DrawRoundRect(rect, grayPaint);
        
        var pad = InterGatePadding;

        height -= 2 * VerticalOffset + (tower.Height - 1) * pad;
        var gateWidth = width - 2 * HorizontalOffset;
        
        var gateHeight = height / tower.Height;
        
        // Draw the horizontal lines
        for (int i = 0; i < tower.Height; i++)
        {
            float p = ((float) i*2 +1) / 2 * gateHeight + i * pad + VerticalOffset;
            svg.DrawLine(x-LineOffset, y+p, x+width+LineOffset, y+p, vioPaint);
        }
        
        // Draw each gate in the tower under each other
        foreach (var gate in tower.Gates)
        {
            DrawGate(svg, gateWidth, gateHeight, x+HorizontalOffset, y+VerticalOffset, gate);
        }
    }
    
    /// <summary>
    /// Method to draw a gate shape in the defined region
    /// </summary>
    /// <param name="svg" cref="SKCanvas">SVG canvas to draw on</param>
    /// <param name="gWidth">Gate shape width</param>
    /// <param name="gHeight">Gate shape height</param>
    /// <param name="x">X-coordinate of shape</param>
    /// <param name="y">Y-coordinate of shape</param>
    /// <param name="gate" cref="IGate"></param>
    private static void DrawGate(SKCanvas svg, int gWidth, int gHeight, int x, int y, IGate gate)
    {
        if (gate.Type == GateType.Support 
            && ((ISupportGate) gate).SupportType == SupportType.None) return;
        var size = gate.TargetRange.Item2 - gate.TargetRange.Item1 + 1;
        var gateHeight = size * (gHeight + InterGatePadding) - InterGatePadding;
        var gateY = y + gate.TargetRange.Item1 * (gHeight + InterGatePadding);
        DrawBox(svg, x, gateY, gWidth, gateHeight, gate.Id);
    }
    
    /// <summary>
    /// Method to draw a grid of towers in the defined region
    /// </summary>
    /// <param name="svg" cref="SKCanvas">SVG canvas to draw on</param>
    /// <param name="leftOffset">Horizontal offset of grid region</param>
    /// <param name="height">Height of schema</param>
    /// <param name="width">Width of schema</param>
    /// <param name="grid">List of Towers to be drawn on the schema</param>
    public static void DrawTowerGrid(SKCanvas svg, int leftOffset, int height, int width, List<Tower> grid)
    {
        var towerWidth = width / grid.Count - 2 * InterTowerPadding;
        var towerX = leftOffset + InterTowerPadding;
        foreach (var tower in grid)
        {
            DrawTower(svg, towerX, 0, height, towerWidth, tower);
            towerX += towerWidth + InterTowerPadding;
        }        
    }
    
    /// <summary>
    /// Method to draw a circuit on an SVG canvas and store it in a file
    /// </summary>
    /// <param name="path">SVG file relative path</param>
    /// <param name="circuit" cref="Circuit">Target circuit object to be drawn</param>
    public static void DrawCircuitSvg(string path, Circuit circuit)
    {
        var bounds = new SKRect(0, 0, 700, 500);

        // Save the canvas to an SVG file
        var stream = new SKFileWStream(path);
        using var svg = SKSvgCanvas.Create(bounds, stream);

        var bitNumber = circuit.Allocation.QubitNumber + circuit.Allocation.CbitNumber;
        
        var grayPaint = new SKPaint
        {
            Color = new SKColor(255, 242,189),
            StrokeWidth = 3,
            IsAntialias = true,
        };
        
        var vioPaint = new SKPaint
        {
            
            Color = SKColors.BlueViolet,
            StrokeWidth = 3,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        var verticalOffset = 10;
        var pad = InterGatePadding;

        var height = 500 - 2 * verticalOffset - (bitNumber - 1) * pad;
        
        var gateHeight = height / bitNumber;
        
        // Draw the horizontal lines
        for (int i = 0; i < bitNumber; i++)
        {
            float p = ((float) i*2 +1) / 2 * gateHeight + i * pad + verticalOffset;
            svg.DrawLine(0, p, 50, p, vioPaint);
            svg.DrawRoundRect(SKRect.Create(0, p-20, 50, 40), 10, 10, grayPaint);
            svg.DrawText(circuit.Allocation.GetBitName(i), 5, p+5, 
                new SKFont(SKTypeface.FromFile("cascadiaFont.otf"), 18F), vioPaint);
        }
        
        DrawTowerGrid(svg, 50, 500, 700, circuit.GateGrid);
    }   

}