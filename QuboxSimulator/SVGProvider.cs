
using QuboxSimulator.Gates;
using QuboxSimulator.Circuits;
using SkiaSharp;

namespace QuboxSimulator;

public class SvgProvider
{
    private const int InterGatePadding = 20;
    
    private static void DrawBox(SKCanvas svg, int x, int y, int width, int height, string id)
    {
        var redPaint = new SKPaint
        {
            Color = new SKColor(141,11,61),
            StrokeWidth = 3,
            IsAntialias = true,
        };
        
        var whitePaint = new SKPaint
        {
            Color = SKColors.White,
            StrokeWidth = 3,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true,
        };
        
        var rect = new SKRoundRect(SKRect.Create(x, y, width, height), 10);
        svg.DrawRoundRect(rect, redPaint);
        svg.DrawText(id, x+width/2, y+height/2, 
            new SKFont(SKTypeface.FromFile("cascadiaFont.otf"), 18F), whitePaint);
    }
    private static void DrawTower(SKCanvas svg, int x, int y, int height, int width, Tower tower)
    {
        var grayPaint = new SKPaint
        {
            Color = new SKColor(219, 199,175,70),
            StrokeWidth = 3,
            IsAntialias = true,
        };
        
        // Create an SKPaint object to draw the lines
        var vioPaint = new SKPaint
        {
            Color = SKColors.BlueViolet,
            StrokeWidth = 3,
            StrokeCap = SKStrokeCap.Round,
            Style = SKPaintStyle.Stroke
        };

        var rect = new SKRoundRect(SKRect.Create(x, y, width, height), 10);
        svg.DrawRoundRect(rect, grayPaint);

        const int verticalOffset = 10;
        const int horizontalOffset = 10;
        const int lineOffset = 30;
        var pad = InterGatePadding;

        height -= 2 * verticalOffset + (tower.Height - 1) * pad;
        var gateWidth = width - 2 * horizontalOffset;
        
        var gateHeight = height / tower.Height;
        
        // Draw the horizontal lines
        for (int i = 0; i < tower.Height; i++)
        {
            float p = ((float) i*2 +1) / 2 * gateHeight + i * pad + verticalOffset;
            svg.DrawLine(x-lineOffset, y+p, x+width+lineOffset, y+p, vioPaint);
        }

        foreach (var gate in tower.Gates)
        {
            DrawGate(gate, gateWidth, gateHeight, x+horizontalOffset, y+verticalOffset, svg);
        }
    }
    private static void DrawGate(IGate gate, int gWidth, int gHeight, int x, int y, SKCanvas svg)
    {
        if (gate.Type == GateType.Support 
            && ((ISupportGate) gate).SupportType == SupportType.None) return;
        var size = gate.TargetRange.Item2 - gate.TargetRange.Item1 + 1;
        var gateHeight = size * (gHeight + InterGatePadding) - InterGatePadding;
        var gateY = y + gate.TargetRange.Item1 * (gHeight + InterGatePadding);
        DrawBox(svg, x, gateY, gWidth, gateHeight, gate.Id);
    }
    public static void DrawTowerGrid(List<Tower> grid, int leftOffset, int height, int width, SKCanvas svg)
    {
        const int InterTowerPadding = 30;
        var towerWidth = width / grid.Count - 2 * InterTowerPadding;
        var towerX = leftOffset + InterTowerPadding;
        foreach (var tower in grid)
        {
            DrawTower(svg, towerX, 0, height, towerWidth, tower);
            towerX += towerWidth + InterTowerPadding;
        }        
    }
    
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
        
        DrawTowerGrid(circuit.GateGrid, 50, 500, 700, svg);
    }   

}