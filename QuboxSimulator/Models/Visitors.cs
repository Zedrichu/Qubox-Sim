using static QuantumLanguage.VisitorPattern;
using static QuantumLanguage.AST;

namespace QuboxSimulator.Models;

public class OperatorVisitor: IVisitor<@operator, Circuit>
{
    private Circuit _circuit;
    private GateFactory _gateFactory;
    
    public OperatorVisitor(Circuit circuit, GateFactory gateFactory)
    {
        _circuit = circuit;
        _gateFactory = gateFactory;
    }
    
    public Circuit Visit(@operator op)
    {
        switch (op)
        {
            case var _ when op.Equals(@operator.NOP):
                Console.WriteLine(op.ToString());
                break;
            case @operator.Assign pair:
                Console.WriteLine(pair.Item.Item1);
                Console.WriteLine(pair.Item.Item2.ToString());
                break;
                
        }

        return null;
    }
}
