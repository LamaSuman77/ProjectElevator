namespace ElevatorSystem;

/// <summary>
/// The two door panels are sliding apart. The car must not move yet.
/// </summary>
public class DoorsOpeningState : IElevatorState
{
    public string Name
    {
        get { return "Doors Opening"; }
    }

    /// <summary>
    /// Same floor: keep opening. Other floor: queue it for after the doors close.
    /// </summary>
    public void HandleRequest(Elevator elevator, int targetFloor, string source)
    {
        if (targetFloor == elevator.CurrentFloor)
        {
            return;
        }

        elevator.QueueRequest(targetFloor, source);
    }
}
