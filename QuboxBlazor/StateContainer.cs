namespace QuboxBlazor;
/* C#
 -*- coding: utf-8 -*-
StateContainer

Description: Implementation of a state container to maintain session state between tabs.

@__Author --> Created by Adrian Zvizdenco aka Zedrichu
@__Date & Time --> Created on 14/04/2023
@__Email --> adrzvizdencojr@gmail.com
@__Version --> 1.0
@__Status --> DEV
*/

using QuBoxEngine.Circuits;


/// <summary>
/// Class modelling the state container for the application.
/// </summary>
public class StateContainer
{
    /// <summary>
    /// Instance of the circuit object to be saved.
    /// </summary>
    private Circuit? savedCircuit;
    
    public string PrintState()
    {
        if (savedCircuit == null)
        {
            return "Circuit is missing!";
        }
        return savedCircuit.ToString();
    }

    /// <summary>
    /// Retrieval and update of the saved Circuit property.
    /// </summary>
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