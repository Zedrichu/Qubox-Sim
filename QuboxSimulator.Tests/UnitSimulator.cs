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
    public void HadamardMeasurement()
    {
        var code = "Qalloc q,r; Calloc c; H q; Reset r; Barrier q; Measure q -> c;";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.AreEqual(Interpreter.Error, AST.Error.Success);
        var simulator = new Simulator(circuit);
        var state = simulator.Run();
        
        // Check the resulting state vector
        Assert.That(state.StateVector[0].Real, Is.EqualTo(1).Within(0.0001));
        Assert.That(state.StateVector[1].Real, Is.EqualTo(0).Within(0.0001));
        // Check the resulting measurements
        Assert.That(state.ProbeVector[0], Is.EqualTo(0.5).Within(0.0001));
        Assert.That(state.ProbeVector[1], Is.EqualTo(0.5).Within(0.0001));
    }

    [Test]
    public void DoubleHadamard()
    {
        var code = "Qalloc q; Calloc c; H q; H q;";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.AreEqual(Interpreter.Error, AST.Error.Success);
        var simulator = new Simulator(circuit);
        var state = simulator.Run();
        Assert.That(state.StateVector[0].Real, Is.EqualTo(1).Within(0.0001));
        Assert.That(state.StateVector[1].Real, Is.EqualTo(0).Within(0.0001));
    }
    
    [Test]
    public void HadamardXHadamard()
    {
        var code = "Qalloc q,w; Calloc c; H q; H w;";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.AreEqual(Interpreter.Error, AST.Error.Success);
        var simulator = new Simulator(circuit);
        var state = simulator.Run();
        
        // Check the resulting state vector
        Assert.That(state.StateVector[0].Real, Is.EqualTo(0.5).Within(0.0001));
        Assert.That(state.StateVector[1].Real, Is.EqualTo(0.5).Within(0.0001));
        Assert.That(state.StateVector[2].Real, Is.EqualTo(0.5).Within(0.0001));
        Assert.That(state.StateVector[3].Real, Is.EqualTo(0.5).Within(0.0001));
        
        // Check the resulting measurements on classical channels
        Assert.That(state.ProbeVector[0], Is.EqualTo(0.25).Within(0.0001));
        Assert.That(state.ProbeVector[1], Is.EqualTo(0.25).Within(0.0001));
        Assert.That(state.ProbeVector[2], Is.EqualTo(0.25).Within(0.0001));
        Assert.That(state.ProbeVector[3], Is.EqualTo(0.25).Within(0.0001));
    }

    [Test]
    public void PauliXMeasurement()
    {
        var code = "Qalloc q; Calloc c; X q; Measure q -> c;";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.AreEqual(Interpreter.Error, AST.Error.Success);
        var simulator = new Simulator(circuit);
        var state = simulator.Run();
        Assert.That(state.StateVector[0].Real, Is.EqualTo(0).Within(0.0001));
        Assert.That(state.StateVector[1].Real, Is.EqualTo(1).Within(0.0001));
    }
    
    [Test]
    public void PhaseDiskHadamard()
    {
        var code = "Qalloc q; Calloc c; H q; PhaseDisk;";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.AreEqual(Interpreter.Error, AST.Error.Success);
        var simulator = new Simulator(circuit);
        var state = simulator.Run();
        
        // Check the resulting state vector
        Assert.That(state.StateVector[0].Real, Is.EqualTo(1/Math.Sqrt(2)).Within(0.0001));
        Assert.That(state.StateVector[1].Real, Is.EqualTo(1/Math.Sqrt(2)).Within(0.0001));  
        
        // Check the resulting measurement 
        Assert.That(state.ProbeVector[0], Is.EqualTo(0.5).Within(0.0001));
        Assert.That(state.ProbeVector[1], Is.EqualTo(0.5).Within(0.0001));

        // Check the resulting PhaseDisk records
        var records = simulator.PhaseDisks;
        Assert.That(records[0].Item1, Is.EqualTo(0.5).Within(0.0001));
        Assert.That(records[0].Item2, Is.EqualTo(0.5).Within(0.0001));
        Assert.That(records[1].Item1, Is.EqualTo(1).Within(0.0001));
        Assert.That(records[1].Item2, Is.EqualTo(0).Within(0.0001));
    }
    
    [Test]
    public void PauliYxSxHadamardXMeasurement()
    {
        var code = "Qalloc q,p; Calloc c[2]; Y q; S q; H p; Measure q -> c[0]; Measure p -> c[1];";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.AreEqual(Interpreter.Error, AST.Error.Success);
        var simulator = new Simulator(circuit);
        var state = simulator.Run();
        
        // Check the resulting state vector
        Assert.That(state.StateVector[0].Real, Is.EqualTo(0).Within(0.0001));
        Assert.That(state.StateVector[2].Real, Is.EqualTo(-1).Within(0.0001));  
        
        // Check the resulting measurements
        Assert.That(state.ProbeVector[0], Is.EqualTo(0).Within(0.0001));
        Assert.That(state.ProbeVector[1], Is.EqualTo(0).Within(0.0001));
        Assert.That(state.ProbeVector[2], Is.EqualTo(0.5).Within(0.0001));
        Assert.That(state.ProbeVector[3], Is.EqualTo(0.5).Within(0.0001));
    }
    
    [Test]
    public void PauliZRxRy()
    {
        var code = "Qalloc q[2]; Calloc c[2]; Z q[0]; RX (Pi/4) q[1]; RY (Pi/3) q[0];";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.AreEqual(Interpreter.Error, AST.Error.Success);
        var simulator = new Simulator(circuit);
        var state = simulator.Run();
        
        // Check the resulting state vector
        Assert.That(state.StateVector[0].Real, Is.EqualTo(0.8).Within(0.001));
        Assert.That(state.StateVector[1].Imaginary, Is.EqualTo(-0.331).Within(0.001));
        Assert.That(state.StateVector[2].Real, Is.EqualTo(0.462).Within(0.001));
        Assert.That(state.StateVector[3].Imaginary, Is.EqualTo(-0.191).Within(0.001));
        
        Console.WriteLine(state.ProbeVector);
        
        // Check the resulting measurements
        Assert.That(state.ProbeVector[0], Is.EqualTo(0.64).Within(0.01));
        Assert.That(state.ProbeVector[2], Is.EqualTo(0.21).Within(0.01));
        Assert.That(state.ProbeVector[1], Is.EqualTo(0.109).Within(0.001));
        Assert.That(state.ProbeVector[3], Is.EqualTo(0.036).Within(0.001));
    }
}