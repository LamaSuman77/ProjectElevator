namespace ElevatorSystem;

/// <summary>
/// State Pattern: each state class decides what the elevator does with a new floor request.
/// </summary>
public interface IElevatorState
{
    /// <summary>
    /// Returns a short name for display, such as Idle or Moving Up.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Handles a floor request while the elevator is in this state.
    /// </summary>
    void HandleRequest(Elevator elevator, int targetFloor, string source);
}
