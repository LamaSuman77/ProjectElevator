namespace ElevatorSystem;

/// <summary>
/// The two door panels are sliding together. A same-floor call opens them again.
/// </summary>
public class DoorsClosingState : IElevatorState
{
    public string Name
    {
        get { return "Doors Closing"; }
    }

    /// <summary>
    /// Same floor: open the doors again. Other floor: queue it.
    /// </summary>
    public void HandleRequest(Elevator elevator, int targetFloor, string source)
    {
        if (targetFloor == elevator.CurrentFloor)
        {
            elevator.BeginDoorOpening(source);
            return;
        }

        elevator.QueueRequest(targetFloor, source);
    }
}
