using QuboxSimulator;
using QuboxSimulator.Gates;

namespace QuBoxEngine.Tests;

public class UnitTestingGates
{
    [SetUp]
    public void Setup()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
    }

    [Test]
    public void TestSimpleCircuit()
    {
        var code = "Qalloc q; Calloc c; H q; Measure q -> c;";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.AreEqual(Interpreter.Error, AST.Error.Success);
        Assert.That(circuit.Allocation.QubitNumber, Is.EqualTo(1));
        Assert.That(circuit.Allocation.CbitNumber, Is.EqualTo(1));
        Assert.That(circuit.GateGrid.Count, Is.EqualTo(2));
        Assert.That(circuit.GateGrid[0].Gates.Count, Is.EqualTo(2));
        Assert.That(circuit.GateGrid[1].Gates.Count, Is.EqualTo(1));
        var hgate = circuit.GateGrid[0].Gates[0];
        Assert.That(hgate.TargetRange.Item1, Is.EqualTo(0));
        var mgate = circuit.GateGrid[1].Gates[0];
        Assert.That(mgate.TargetRange.Item1, Is.EqualTo(0));
        Assert.That(mgate.TargetRange.Item2, Is.EqualTo(1));
    }

    [Test]
    public void TestGateFactory()
    {
        var code = "Qalloc q; Calloc c; If (3>5) Reset q; Measure q -> c;";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.AreEqual(Interpreter.Error, AST.Error.Success);
        Assert.That(circuit.GateGrid[0].Gates[0].Condition is not null);
    }
    
    [Test]
    public void TestGateFactory2()
    {
        var code = "Qalloc q[2]; Calloc c; alpha := -Pi/4; beta := -Pi/2; H q[0]; RY(2*alpha) q[0]; RY(2*beta) q[1];";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.AreEqual(Interpreter.Error, AST.Error.Success);
        var ry = circuit.GateGrid[1].Gates[0];
        Assert.That(ry.Type, Is.EqualTo(GateType.Param));
        var ry2 = circuit.GateGrid[0].Gates[1];
        Console.WriteLine(ry);
        Console.WriteLine(ry2);
    }
    
    [Test]
    public void TestGateFactory3()
    {
        var code = "Qalloc q[2]; Calloc c; alpha := 0; beta := 3*Pi/8; H q[0]; RY(2*alpha) q[0]; RY(2*beta) q[1];";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.AreEqual(Interpreter.Error, AST.Error.Success);
        var ry = circuit.GateGrid[1].Gates[0];
        Assert.That(ry.Type, Is.EqualTo(GateType.Param));
        var ry2 = circuit.GateGrid[0].Gates[1];
        Console.WriteLine(ry);
        Console.WriteLine(ry2);
    }
}