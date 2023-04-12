using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using Complex = System.Numerics.Complex;

namespace QuboxSimulator.Tests;

public class SimulatorTests
{
    [SetUp]
    public void Setup()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
    }
    
    [Test] 
    public void TestSimpleCircuit()
    {
        var code = "Qalloc q,r; Calloc c; H q; Reset r; Barrier q; Measure q -> c;";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.AreEqual(Interpreter.Error, AST.Error.Success);
        var simulator = new Simulator(circuit);
        var state = simulator.Run();
        var results = simulator.GetResults();
    }
}