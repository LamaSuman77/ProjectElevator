namespace ElevatorSystem;

/// <summary>
/// The elevator is standing still with the doors closed (or just after they closed).
/// A new request can start a trip immediately.
/// </summary>
public class IdleState : IElevatorState
{
    public string Name
    {
        get { return "Idle"; }
    }

    /// <summary>
    /// Starts moving if the target is a different floor, otherwise reports already here.
    /// </summary>
    public void HandleRequest(Elevator elevator, int targetFloor, string source)
    {
        if (targetFloor == elevator.CurrentFloor)
        {
            elevator.ReportAlreadyHere();
            return;
        }

        elevator.StartTrip(targetFloor, source);
    }
}
