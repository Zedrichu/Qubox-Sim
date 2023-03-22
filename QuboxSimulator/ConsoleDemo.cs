using QuboxSimulator.Models;
using QuboxSimulator.Models.Gates;
using static QuantumLanguage.Handler;
namespace QuboxSimulator;

public class ConsoleDemo
{
    public static void Demo()
    {
        // Testing visitor 
        var quLangCode = Console.ReadLine();
        var ast = parseQuLang(quLangCode);
        Console.WriteLine(ast.Item1.ToString());
        Console.WriteLine(ast.Item2.ToString());
        var sem = analyzeSemantics(ast.Item1.Item1, ast.Item1.Item2);
        var optim = optimizeAST(ast.Item1.Item2, sem.Item1);
        Console.WriteLine(sem);
        Console.WriteLine(optim);
        var visitor = new OperatorVisitor(optim.Item2);
        var gates = optim.Item1.Accept(visitor);
        //var circuit = CircuitFactory.CreateCircuit(gates);
    }
 }