namespace QuboxSimulator.Models;

public interface IGate
{
    public abstract int QubitNumber { get; set; }
    
    public abstract double[] Matrix { get; set; }

}