namespace ElevatorSystem;

/// <summary>
/// The elevator is standing still with the doors closed.
/// A new request can start a trip, or open the doors if we are already on that floor.
/// </summary>
public class IdleState : IElevatorState
{
    public string Name
    {
        get { return "Idle"; }
    }

    /// <summary>
    /// Starts moving if the target is a different floor. Same floor opens the doors.
    /// </summary>
    public void HandleRequest(Elevator elevator, int targetFloor, string source)
    {
        if (targetFloor == elevator.CurrentFloor)
        {
            elevator.BeginDoorOpening(source);
            return;
        }

        elevator.StartTrip(targetFloor, source);
    }
}
