namespace ElevatorSystem;

/// <summary>
/// The doors are fully open and waiting (dwell time). The car still does not move.
/// </summary>
public class DoorsOpenState : IElevatorState
{
    public string Name
    {
        get { return "Doors Open"; }
    }

    /// <summary>
    /// Same floor: restart the dwell timer. Other floor: queue it until the doors close.
    /// </summary>
    public void HandleRequest(Elevator elevator, int targetFloor, string source)
    {
        if (targetFloor == elevator.CurrentFloor)
        {
            elevator.HoldDoorsOpen();
            return;
        }

        elevator.QueueRequest(targetFloor, source);
    }
}
