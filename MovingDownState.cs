namespace ElevatorSystem;

/// <summary>
/// The car is travelling downward. New requests are queued, never interrupting the trip.
/// </summary>
public class MovingDownState : IElevatorState
{
    public string Name
    {
        get { return "Moving Down"; }
    }

    /// <summary>
    /// Adds the floor to the queue so it is served after the current trip.
    /// </summary>
    public void HandleRequest(Elevator elevator, int targetFloor, string source)
    {
        elevator.QueueRequest(targetFloor, source);
    }
}
