using QuboxSimulator.Models;
namespace QuboxSimulator;

public class ConsoleDemo
{
    public static void Demo()
    {
        // Testing visitor 
        var quLangCode = Console.ReadLine();
        Interpreter.HandleLang(quLangCode);
        var circuit = Interpreter.Interpret();
        Console.WriteLine(circuit);
    }
 }