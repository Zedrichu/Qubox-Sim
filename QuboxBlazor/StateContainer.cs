using QuBoxEngine.Circuits;

namespace QuboxBlazor;

public class StateContainer
{
    private Circuit? savedCircuit;

    public string PrintState()
    {
        if (savedCircuit == null)
        {
            return "Circuit is missing!";
        }
        return savedCircuit.ToString();
    }

    public Circuit? Property
    {
        get => savedCircuit;
        set
        {
            savedCircuit = value;
            NotifyStateChanged();
        }
    }
    
    public event Action? OnChange;
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}