namespace QuboxSimulator.Models;

public class Register
{
    public int QubitNumber { get; private set; } = 1;
    public int CbitNumber { get; private set; } = 1;

    public Register(int qubitNumber, int cbitNumber)
    {
        QubitNumber = qubitNumber;
        CbitNumber = cbitNumber;
    }
}