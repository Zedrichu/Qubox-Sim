namespace QuboxSimulator.Models;

public class Tower
{
    public int QubitNo { get; private set; }
    public int CbitNo { get; private set; }
    public Gate[] Gates { get; private set; }

    public Tower(int qubitNo, int cbitNo)
    {
        QubitNo = qubitNo;
        CbitNo = cbitNo;
        int totalChls = qubitNo + cbitNo;
        Gates = new Gate[totalChls];
    }
}