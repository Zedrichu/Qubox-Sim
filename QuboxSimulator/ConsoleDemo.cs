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
        
        //Test out generation
        var reg = circuit.Allocation;
        Generator.Reg = reg;
        var alloc = Generator.DestructRegister(reg);
        var gates = Generator.DestructGateGrid(circuit.GateGrid);
        Console.WriteLine(alloc);
        Console.WriteLine(gates);
    }
 }