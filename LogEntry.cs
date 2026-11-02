namespace ElevatorSystem;

/// <summary>
/// A simple packet of data we pass to the BackgroundWorker when saving a log row.
/// </summary>
public class LogEntry
{
    public int ElevatorId { get; set; }
    public int FromFloor { get; set; }
    public int ToFloor { get; set; }
    public string Status { get; set; }
    public string Source { get; set; }
}
