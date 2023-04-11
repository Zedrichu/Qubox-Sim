namespace QuboxSimulator.Tests;


public class UnitCircuits
{
    [SetUp]
    public void Setup()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
    }

    [Test]
    public void Test1()
    {
        var code = "Qalloc q, r; Calloc c,r;";
        Interpreter.HandleLang(code);
        var err = Interpreter.Error;
        var circuit = Interpreter.Interpret();
        Assert.That(Interpreter.Error.Tag, Is.EqualTo(AST.Error.Tags.SemanticError));
        Assert.That(circuit, Is.Null);
    }
    
    [Test]
    public void Test2()
    {
        var code = "Qalloc q, r; H q;";
        Interpreter.HandleLang(code);
        var err = Interpreter.Error;
        var circuit = Interpreter.Interpret();
        Assert.That(Interpreter.Error.Tag, Is.EqualTo(AST.Error.Tags.SyntaxError));
        Assert.That(circuit, Is.Null);
    }
    
    [Test]
    public void Test3()
    {
        var code = "Qalloc q, r; Calloc c; H q; c = M q";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        var err = Interpreter.Error;
        Assert.That(Interpreter.Error.Tag, Is.EqualTo(AST.Error.Tags.SyntaxError));
        Assert.That(circuit, Is.Null);
    }
    
    [Test]
    public void Test4()
    {
        var code = "Qalloc q, r; Calloc c; x:=2+2; P (a+3) q;";
        Interpreter.HandleLang(code);
        var circuit = Interpreter.Interpret();
        var err = Interpreter.Error;
        Assert.That(Interpreter.Error.Tag, Is.EqualTo(AST.Error.Tags.EvaluationError));
        Assert.That(circuit, Is.Null);
    }

}