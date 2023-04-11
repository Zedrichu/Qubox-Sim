using QuantumLanguage;
using QuboxSimulator;
using QuboxSimulator.Gates;

namespace QuboxSimulator.Tests;

public class GateTests
{
    [SetUp]
    public void Setup()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
    }

    [Test]
    public void Test1()
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

    public void Test2()
    {
        
    }
}