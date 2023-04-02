using QuboxSimulator.Circuits;
namespace QuboxSimulator;

public class ConsoleDemo
{
    public static void Main(string[] args)
    {
        // Testing interpreter 
        var quLangCode = Console.ReadLine();
        Interpreter.HandleLang(quLangCode);
        var circuit = Interpreter.Interpret();
        Console.WriteLine(circuit);
        
        // Testing generation
        if (circuit == null) return;
        var _ = new Generator(circuit);
        var ast = Generator.DestructCircuit();
        Console.WriteLine(ast.Item1);
        Console.WriteLine(ast.Item2);
    }
 }