namespace ElevatorSystem;

/// <summary>
/// Observer Pattern: anything that wants elevator news implements this interface.
/// The elevator (subject) calls these methods. It does not know about buttons or labels.
/// </summary>
public interface IElevatorObserver
{
    /// <summary>
    /// Called when the car passes or arrives at a floor.
    /// </summary>
    void OnFloorChanged(Elevator elevator, int floor, string direction);

    /// <summary>
    /// Called when the status text should change (idle, moving, already here).
    /// </summary>
    void OnStatusChanged(Elevator elevator, string statusText);

    /// <summary>
    /// Called when a row should be written to the database.
    /// </summary>
    void OnLogNeeded(Elevator elevator, int fromFloor, int toFloor, string status, string source);

    /// <summary>
    /// Called when the doors start opening, stay open, start closing, or finish closed.
    /// </summary>
    void OnDoorStateChanged(Elevator elevator, string doorState);

    /// <summary>
    /// Called on every animation tick so the building picture can redraw.
    /// </summary>
    void OnAnimationTick(Elevator elevator);
}
