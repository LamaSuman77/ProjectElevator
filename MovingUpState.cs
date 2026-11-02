namespace ElevatorSystem;

/// <summary>
/// The car is travelling upward. New requests are queued, never interrupting the trip.
/// </summary>
public class MovingUpState : IElevatorState
{
    public string Name
    {
        get { return "Moving Up"; }
    }

    /// <summary>
    /// Adds the floor to the queue so it is served after the current trip.
    /// </summary>
    public void HandleRequest(Elevator elevator, int targetFloor, string source)
    {
        elevator.QueueRequest(targetFloor, source);
    }
}
