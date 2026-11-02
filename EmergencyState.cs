namespace ElevatorSystem;

/// <summary>
/// The car is stopped because Emergency was pressed.
/// Floor requests are ignored until Reset is pressed.
/// </summary>
public class EmergencyState : IElevatorState
{
    public string Name
    {
        get { return "Emergency"; }
    }

    /// <summary>
    /// Does nothing. Panel buttons and hall calls cannot move a car in emergency.
    /// </summary>
    public void HandleRequest(Elevator elevator, int targetFloor, string source)
    {
        // Requests are ignored on purpose while the elevator is in emergency.
    }
}
