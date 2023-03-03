namespace QuboxSimulator.Models;

public class Circuit
{
    public Register Allocation { get; private set; }
    

    public Circuit(int qubitNumber, int cbitNumber)
    {
        Allocation = new Register(qubitNumber, cbitNumber);
    }

}