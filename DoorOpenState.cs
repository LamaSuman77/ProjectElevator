namespace ElevatorSystem;

/// <summary>
/// The car has arrived and the doors are open for a short time.
/// A new request can start the next trip without waiting for the door timer.
/// </summary>
public class DoorOpenState : IElevatorState
{
    public string Name
    {
        get { return "Doors Open"; }
    }

    /// <summary>
    /// Starts a new trip if needed, or keeps the doors open on the same floor.
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
