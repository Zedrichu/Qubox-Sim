using QuBoxEngine;
using QuBoxEngine.Circuits;
using QuBoxEngine.Gates;

namespace QuBoxEngine.Tests;


public class IntegrationTestingCircuit
{
    [SetUp]
    public void Setup()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
    }

    [Test]
    public void InvalidRegisters()
    {
        var code = "Qalloc q, r; Calloc c,r;";
        Interpreter.HandleLang(code);
        var err = Interpreter.Error;
        var circuit = Interpreter.Interpret();
        Assert.That(Interpreter.Error.Tag, Is.EqualTo(AST.Error.Tags.SemanticError));
        Assert.That(circuit, Is.Null);
    }
    
    [Test]
    public void MissingRegisters()
    {
        var code = "Qalloc q, r; H q;";
        Interpreter.HandleLang(code);
        var err = Interpreter.Error;
        var circuit = Interpreter.Interpret();
        Assert.That(Interpreter.Error.Tag, Is.EqualTo(AST.Error.Tags.SyntaxError));
        Assert.That(circuit, Is.Null);
    }
    
    [Test]
    public void InvalidMeasurement()
    {
        var code = "Qalloc q, r; Calloc c; H q; c = M q";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        var err = Interpreter.Error;
        Assert.That(Interpreter.Error.Tag, Is.EqualTo(AST.Error.Tags.SyntaxError));
        Assert.That(circuit, Is.Null);
    }
    
    [Test]
    public void UndefinedVariable()
    {
        var code = "Qalloc q, r; Calloc c; x:=2+2; P (a+3) q;";
        Interpreter.HandleLang(code);
        var err = Interpreter.Error;
        Console.WriteLine(err);
        Assert.That(Interpreter.Error.Tag, Is.EqualTo(AST.Error.Tags.EvaluationError));
        var circuit = Interpreter.Interpret();
        Assert.That(circuit, Is.Null);
    }

    [Test]
    public void BitNameValidation()
    {
        var code = "Qalloc q[2], r, x[1]; Calloc c[2]; x:= 2+2*5;";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.That(Interpreter.Error, Is.EqualTo(AST.Error.Success));
        var register = circuit.Allocation;
        Console.WriteLine(register);
        Assert.That(register.CbitNumber, Is.EqualTo(2));
        Assert.That(register.QubitNumber, Is.EqualTo(4));
        Assert.That(register.GetBitName(4), Is.EqualTo("c[0]"));
        Assert.That(register.GetBitName(3), Is.EqualTo("x"));
        Assert.That(register.GetBitName(2), Is.EqualTo("r"));
    }

    [Test]
    public void TowerTesting()
    {
        var code = "Qalloc q[2]; Calloc c; Reset q[0]; X q[1]; Barrier q[1]; CNOT q[0], q[1]; Measure q[0] -> c;";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.That(Interpreter.Error, Is.EqualTo(AST.Error.Success));
        Assert.That(circuit.GateGrid.Count, Is.EqualTo(4));
        Assert.That(circuit.GateGrid[0].Gates.Count, Is.EqualTo(3));
        Console.WriteLine(circuit.ToString());
        Console.WriteLine(circuit.GateGrid[1]);
        Assert.That(circuit.GateGrid.TrueForAll(tower => tower.Height == 3));
        Assert.That(circuit.GateGrid[2].IsEmpty, Is.EqualTo(false));
    }

    [Test]
    public void VisitorTesting()
    {
        var code = "Qalloc q[3]; Calloc c; PhaseDisk; RXX(Pi/2) q[0], q[1]; RX (Pi/2) q[2]; CCX q[0], q[1], q[2];";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.That(Interpreter.Error, Is.EqualTo(AST.Error.Success));
        Assert.That(circuit.GateGrid.Count, Is.EqualTo(3));
        Assert.That(circuit.GateGrid[0].Gates.Count(gate => gate.Type is GateType.Support 
                    && ((ISupportGate) gate).SupportType is SupportType.PhaseDisk), Is.EqualTo(1));
        Assert.That(circuit.GateGrid[1].Gates.Count(gate => gate.Type is GateType.DoubleParam) , Is.EqualTo(1));
        var lastGates = circuit.GateGrid[2].Gates;
        Assert.That(lastGates[1].Type == GateType.Support && lastGates[1].Id == "NONE");
        Assert.That(lastGates[0].Type == GateType.Toffoli);
    }

    [Test]
    public void VisitConditionalUnitary()
    {
        var code = "Qalloc q; Calloc c; If (true and c |> Click) U (1,2,3) q;";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.That(Interpreter.Error, Is.EqualTo(AST.Error.Success));
        Assert.That(circuit.GateGrid.Count, Is.EqualTo(1));
        var gate = circuit.GateGrid[0].Gates[0];
        Assert.That(gate.Type == GateType.Unitary);
        Assert.That(gate.Condition is not null);
        Console.WriteLine(gate.Condition);
    }

    [Test]
    public void ArithmeticAssignments()
    {
        var code = "Qalloc q; Calloc c; x:=3+5*2-9.0+Pi; y:=-(x+1.0)/2; RX(-Pi) q; z:=y^2 % 5; w:=z+Pi/3.0; P (w+3-(-y)+x/Pi*z) q;";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.That(Interpreter.Error, Is.EqualTo(AST.Error.Success));
        var register = circuit.Allocation;
        Assert.That(register.ArithVariables["x"].Item1, Is.EqualTo(AST.ArithExpr.NewBinaryOp(
            new Tuple<AST.ArithExpr, AST.AOp, AST.ArithExpr>(
                AST.ArithExpr.Pi, AST.AOp.Add, AST.ArithExpr.NewFloat(4.0))
            )));
    }

    [Test]
    public void ArithmeticVisitor()
    {
        var code = "5.0 + 2.3 * 4 - 5^2 + (+x) + Pi/3 - (-Pi)";
        var ast = Handler.parseArith(code);
        Assert.That(ast.Item2, Is.EqualTo(AST.Error.Success));
        var memory = new Dictionary<string, Tuple<AST.ArithExpr, int>>
            {{"x", new Tuple<AST.ArithExpr, int>(AST.ArithExpr.NewFloat(3.0), 0)}};
        var visitor = new ArithmeticVisitor(memory);
        var result = visitor.Visit(ast.Item1.Value);
        Assert.That(result, Is.EqualTo(-3.6112097952136102));
    }

    [Test]
    public void GenerationTest1()
    {
        var code = "Qalloc q; Calloc c; If (true and c |> Click) U (1,2,3) q;";
        var ast = Handler.parseQuLang(code);
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.That(Interpreter.Error, Is.EqualTo(AST.Error.Success));
        var result = Interpreter.DecomposeCircuit(circuit);
        var code2 = "Qalloc q; Calloc c; If (c |> Click) U (1,2,3) q;";
        var ast2 = Handler.parseQuLang(code2);
        Assert.That(result, Is.EqualTo(ast2.Item1.Value));
    }

    [Test]
    public void GenerationTest2()
    {
        var code = "Qalloc q[2]; Calloc c; x:=3+4.0; y:=2.0+x; Reset q[0]; RX(y+Pi) q[0]; X q[1]; CNOT q[0], q[1]; Measure q[0] -> c;";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.That(Interpreter.Error, Is.EqualTo(AST.Error.Success));
        var result = Interpreter.DecomposeCircuit(circuit);
        var code2 = "Qalloc q[2]; Calloc c; x:=7.0; y:=9.0; Reset q[0]; X q[1]; RX(Pi+9) q[0]; CNOT q[0], q[1]; Measure q[0] -> c;";
        var ast = Handler.parseQuLang(code2);
        Assert.That(result, Is.EqualTo(ast.Item1.Value));
    }
    
    [Test]
    public void GenerationTest3()
    {
        var code = "Qalloc q[2], r; Calloc c; true =| x; 3 < 2 =| y; If (~x or ~y and c |> Click) CCX q[0], q[1], r;";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.That(Interpreter.Error, Is.EqualTo(AST.Error.Success));
        var result = Interpreter.DecomposeCircuit(circuit);
        var code2 = "Qalloc q[2], r; Calloc c; true =| x; false =| y; If (true) CCX q[0], q[1], r;";
        var ast = Handler.parseQuLang(code2);
        Assert.That(result, Is.EqualTo(ast.Item1.Value));
    }
    
    [Test]
    public void GenerationTest4()
    {
        var code = "Qalloc q[2], r; Calloc c; Barrier q[0]; PhaseDisk; RXX (Pi/2) q[0], q[1]; Barrier q[0];";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        Assert.That(Interpreter.Error, Is.EqualTo(AST.Error.Success));
        var result = Interpreter.DecomposeCircuit(circuit);
        var ast = Handler.parseQuLang(code);
        Assert.That(result, Is.EqualTo(ast.Item1.Value));
    }

}
