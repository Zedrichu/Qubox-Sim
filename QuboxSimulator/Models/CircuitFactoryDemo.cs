namespace QuboxSimulator.Models;
using QuantumLanguage;

public class CircuitDemo
{
    static void Main(string[] args)
    {
        String qulangCode = Console.ReadLine();
        Console.WriteLine(Handler.ParseQuLang(qulangCode));
    }
}