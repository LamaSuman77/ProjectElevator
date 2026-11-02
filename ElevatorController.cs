namespace ElevatorSystem;

/// <summary>
/// Chooses which elevator should answer an outside (hall) call.
/// Panel buttons never come here; they go straight to that elevator.
/// </summary>
public class ElevatorController
{
    private readonly Elevator _elevatorA;
    private readonly Elevator _elevatorB;

    /// <summary>
    /// Stores the two cars so hall buttons can ask "who should come?"
    /// </summary>
    public ElevatorController(Elevator elevatorA, Elevator elevatorB)
    {
        _elevatorA = elevatorA;
        _elevatorB = elevatorB;
    }

    /// <summary>
    /// Sends an outside request to the elevator with the shortest estimated wait.
    /// If both scores are equal, elevator A is used.
    /// </summary>
    public void HandleHallRequest(int floor)
    {
        bool aBusy = _elevatorA.IsInEmergency;
        bool bBusy = _elevatorB.IsInEmergency;

        if (aBusy && bBusy)
        {
            MessageBox.Show("No elevator available", "Outside request",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Elevator chosen;
        if (aBusy)
        {
            chosen = _elevatorB;
        }
        else if (bBusy)
        {
            chosen = _elevatorA;
        }
        else
        {
            chosen = ChooseElevator(floor);
        }

        string source = "Request Button F" + floor;
        chosen.MoveElevatorTo(floor, source);
    }

    /// <summary>
    /// Compares estimated response times. Does not stop a trip that is already running.
    /// </summary>
    public Elevator ChooseElevator(int requestFloor)
    {
        int timeA = _elevatorA.EstimateResponseTime(requestFloor);
        int timeB = _elevatorB.EstimateResponseTime(requestFloor);

        if (timeB < timeA)
        {
            return _elevatorB;
        }

        return _elevatorA;
    }
}
