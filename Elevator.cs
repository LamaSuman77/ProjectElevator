namespace ElevatorSystem;

/// <summary>
/// Delegate used when the car passes or reaches a floor.
/// The form can also subscribe to this event as well as using IElevatorObserver.
/// </summary>
public delegate void FloorChangedHandler(int floor, string direction);

/// <summary>
/// One elevator car: state, queue, animation timer, and observer list.
/// </summary>
public class Elevator
{
    private readonly Panel _car;
    private readonly int[] _floorYPositions;
    private readonly List<IElevatorObserver> _observers;
    private readonly Queue<int> _stopQueue;
    private readonly System.Windows.Forms.Timer _timer;

    private IElevatorState _state;
    private int _doorTickCount;
    private int _lastAnnouncedFloor;
    private string _tripSource;
    private bool _emergencyPending;
    private string _emergencySource;

    /// <summary>
    /// Raised when the car passes or arrives at a floor (Task 4).
    /// </summary>
    public event FloorChangedHandler FloorChanged;

    public int ElevatorId { get; }
    public string Name { get; }
    public int CurrentFloor { get; private set; }
    public int TargetFloor { get; private set; }
    public string Direction { get; private set; }
    public int TripFromFloor { get; private set; }

    /// <summary>
    /// True while the car is travelling between floors.
    /// </summary>
    public bool IsMoving
    {
        get { return _state is MovingUpState || _state is MovingDownState; }
    }

    /// <summary>
    /// True while this car is stopping for emergency or already stopped in EmergencyState.
    /// </summary>
    public bool IsInEmergency
    {
        get { return _emergencyPending || _state is EmergencyState; }
    }

    /// <summary>
    /// Creates one elevator, places its car on floor 1, and starts in Idle.
    /// floorYPositions is 1-based: index 1 is floor 1, index 3 is floor 3.
    /// </summary>
    public Elevator(int elevatorId, string name, Panel car, int[] floorYPositions)
    {
        ElevatorId = elevatorId;
        Name = name;
        _car = car;
        _floorYPositions = floorYPositions;
        _observers = new List<IElevatorObserver>();
        _stopQueue = new Queue<int>();
        _state = new IdleState();
        CurrentFloor = 1;
        TargetFloor = 1;
        Direction = AppConstants.DirectionIdle;
        _tripSource = "";
        _emergencySource = "";
        _emergencyPending = false;
        _lastAnnouncedFloor = 1;

        _timer = new System.Windows.Forms.Timer();
        _timer.Interval = AppConstants.TimerIntervalMs;
        _timer.Tick += Timer_Tick;

        _car.Top = _floorYPositions[1];
    }

    /// <summary>
    /// Adds an observer so the UI and logger can hear about changes.
    /// </summary>
    public void AttachObserver(IElevatorObserver observer)
    {
        if (observer != null && !_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
    }

    /// <summary>
    /// Shared entry point for every hall and panel button that belongs to this car.
    /// </summary>
    public void MoveElevatorTo(int targetFloor, string source)
    {
        if (targetFloor < 1 || targetFloor > AppConstants.TOTAL_FLOORS)
        {
            return;
        }

        if (IsInEmergency)
        {
            return;
        }

        NotifyLog(CurrentFloor, targetFloor, AppConstants.StatusRequest, source);
        _state.HandleRequest(this, targetFloor, source);
    }

    /// <summary>
    /// Starts emergency mode. If the car is moving it finishes the next floor first.
    /// Pressing Emergency again does nothing, so the program cannot crash.
    /// </summary>
    public void TriggerEmergency(string source)
    {
        if (IsInEmergency)
        {
            return;
        }

        _emergencySource = source;
        _stopQueue.Clear();
        NotifyLog(CurrentFloor, CurrentFloor, AppConstants.STATUS_EMERGENCY, source);

        if (IsMoving)
        {
            _emergencyPending = true;
            TargetFloor = GetNextFloorInDirection();
            NotifyStatus("EMERGENCY - stopping at next floor");
            NotifyFloorChanged();
            return;
        }

        EnterEmergencyStop();
    }

    /// <summary>
    /// Leaves EmergencyState and goes back to Idle at the current floor.
    /// Does nothing if this car is not already in emergency.
    /// </summary>
    public void ResetEmergency(string source)
    {
        if (!(_state is EmergencyState))
        {
            return;
        }

        _emergencyPending = false;
        _emergencySource = "";
        _state = new IdleState();
        Direction = AppConstants.DirectionIdle;
        NotifyLog(CurrentFloor, CurrentFloor, AppConstants.STATUS_RESET, source);
        NotifyStatus("Idle at Floor " + CurrentFloor);
        NotifyFloorChanged();
    }

    /// <summary>
    /// Starts a trip. Called from Idle or DoorOpen. Not used while already moving.
    /// </summary>
    public void StartTrip(int targetFloor, string source)
    {
        TripFromFloor = CurrentFloor;
        TargetFloor = targetFloor;
        _tripSource = source;
        _lastAnnouncedFloor = CurrentFloor;
        _doorTickCount = 0;

        if (targetFloor > CurrentFloor)
        {
            Direction = AppConstants.DirectionUp;
            _state = new MovingUpState();
        }
        else
        {
            Direction = AppConstants.DirectionDown;
            _state = new MovingDownState();
        }

        NotifyLog(TripFromFloor, TargetFloor, AppConstants.StatusMoving, source);
        NotifyStatus(BuildMovingText());
        NotifyFloorChanged();
        _timer.Start();
    }

    /// <summary>
    /// Stores a floor to visit after the current trip. Ignores duplicates so spam is safe.
    /// </summary>
    public void QueueRequest(int targetFloor, string source)
    {
        if (targetFloor == TargetFloor)
        {
            return;
        }

        if (_stopQueue.Contains(targetFloor))
        {
            return;
        }

        _stopQueue.Enqueue(targetFloor);
        NotifyStatus(BuildMovingText() + " (queued floor " + targetFloor + " from " + source + ")");
    }

    /// <summary>
    /// Tells the UI the user asked for the floor we are already on.
    /// </summary>
    public void ReportAlreadyHere()
    {
        NotifyStatus("Idle at Floor " + CurrentFloor + " (already here)");
    }

    /// <summary>
    /// Cost used by the controller: remaining trip + queued hops + trip to the new floor.
    /// Lower is better. A trip in progress is never cancelled.
    /// </summary>
    public int EstimateResponseTime(int requestFloor)
    {
        int cost = 0;
        int simFloor = CurrentFloor;

        if (IsMoving)
        {
            if (TargetFloor == requestFloor)
            {
                return Math.Abs(simFloor - TargetFloor);
            }

            cost += Math.Abs(simFloor - TargetFloor);
            simFloor = TargetFloor;
        }

        foreach (int stop in _stopQueue)
        {
            if (stop == requestFloor)
            {
                return cost + Math.Abs(simFloor - stop);
            }

            cost += Math.Abs(simFloor - stop);
            simFloor = stop;
        }

        cost += Math.Abs(simFloor - requestFloor);
        return cost;
    }

    /// <summary>
    /// Moves the car a few pixels, or counts door-open time.
    /// </summary>
    private void Timer_Tick(object sender, EventArgs e)
    {
        if (_state is DoorOpenState)
        {
            TickDoors();
            return;
        }

        int targetY = _floorYPositions[TargetFloor];
        int step = AppConstants.PixelStep;

        if (_car.Top > targetY)
        {
            _car.Top = Math.Max(targetY, _car.Top - step);
        }
        else if (_car.Top < targetY)
        {
            _car.Top = Math.Min(targetY, _car.Top + step);
        }

        AnnounceIfPassingFloor();

        _car.BringToFront();

        if (_car.Top == targetY)
        {
            if (_emergencyPending)
            {
                CurrentFloor = TargetFloor;
                _lastAnnouncedFloor = CurrentFloor;
                EnterEmergencyStop();
                return;
            }

            ArriveAtTarget();
        }
    }

    /// <summary>
    /// Finds the next floor in the current travel direction, without going past the trip target.
    /// </summary>
    private int GetNextFloorInDirection()
    {
        int nextFloor = TargetFloor;

        if (Direction == AppConstants.DirectionUp)
        {
            nextFloor = CurrentFloor + 1;
            if (nextFloor > AppConstants.TOTAL_FLOORS)
            {
                nextFloor = AppConstants.TOTAL_FLOORS;
            }

            if (nextFloor > TargetFloor)
            {
                nextFloor = TargetFloor;
            }
        }
        else if (Direction == AppConstants.DirectionDown)
        {
            nextFloor = CurrentFloor - 1;
            if (nextFloor < 1)
            {
                nextFloor = 1;
            }

            if (nextFloor < TargetFloor)
            {
                nextFloor = TargetFloor;
            }
        }

        return nextFloor;
    }

    /// <summary>
    /// Stops the timer, treats the doors as open, and stays in EmergencyState.
    /// </summary>
    private void EnterEmergencyStop()
    {
        _emergencyPending = false;
        _timer.Stop();
        _state = new EmergencyState();
        Direction = AppConstants.DirectionIdle;
        NotifyLog(CurrentFloor, CurrentFloor, AppConstants.STATUS_EMERGENCY, _emergencySource);
        NotifyStatus("EMERGENCY - stopped at Floor " + CurrentFloor);
        NotifyFloorChanged();
    }

    /// <summary>
    /// Updates CurrentFloor when the car lines up with a floor on the way.
    /// </summary>
    private void AnnounceIfPassingFloor()
    {
        for (int floor = 1; floor <= AppConstants.TOTAL_FLOORS; floor++)
        {
            int gap = Math.Abs(_car.Top - _floorYPositions[floor]);
            if (gap <= AppConstants.PixelStep && floor != _lastAnnouncedFloor)
            {
                CurrentFloor = floor;
                _lastAnnouncedFloor = floor;
                NotifyFloorChanged();
            }
        }
    }

    /// <summary>
    /// Stops the car, logs Arrived, opens the doors, then serves the queue.
    /// </summary>
    private void ArriveAtTarget()
    {
        CurrentFloor = TargetFloor;
        _lastAnnouncedFloor = CurrentFloor;
        Direction = AppConstants.DirectionIdle;
        _state = new DoorOpenState();
        _doorTickCount = 0;

        NotifyLog(TripFromFloor, CurrentFloor, AppConstants.StatusArrived, _tripSource);
        NotifyFloorChanged();
        NotifyStatus("Arrived at Floor " + CurrentFloor + " (doors open)");
    }

    /// <summary>
    /// Counts ticks while doors are open, then goes idle or starts the next queued stop.
    /// </summary>
    private void TickDoors()
    {
        _doorTickCount++;
        if (_doorTickCount < AppConstants.DoorOpenTicks)
        {
            return;
        }

        if (_stopQueue.Count > 0)
        {
            int next = _stopQueue.Dequeue();
            StartTrip(next, "Queued request");
            return;
        }

        _timer.Stop();
        _state = new IdleState();
        Direction = AppConstants.DirectionIdle;
        NotifyStatus("Idle at Floor " + CurrentFloor);
        NotifyFloorChanged();
    }

    /// <summary>
    /// Builds the live journey text, for example "Moving: Floor 1 -> Floor 3 (Up)".
    /// </summary>
    private string BuildMovingText()
    {
        return "Moving: Floor " + TripFromFloor + " -> Floor " + TargetFloor + " (" + Direction + ")";
    }

    /// <summary>
    /// Tells every observer the floor and direction, and raises the FloorChanged event.
    /// </summary>
    private void NotifyFloorChanged()
    {
        if (FloorChanged != null)
        {
            FloorChanged(CurrentFloor, Direction);
        }

        for (int i = 0; i < _observers.Count; i++)
        {
            _observers[i].OnFloorChanged(this, CurrentFloor, Direction);
        }
    }

    /// <summary>
    /// Sends a status sentence to every observer.
    /// </summary>
    private void NotifyStatus(string text)
    {
        string labelled = "Elevator " + Name + ": " + text;
        for (int i = 0; i < _observers.Count; i++)
        {
            _observers[i].OnStatusChanged(this, labelled);
        }
    }

    /// <summary>
    /// Asks observers to save a database row (the form does this on a BackgroundWorker).
    /// </summary>
    private void NotifyLog(int fromFloor, int toFloor, string status, string source)
    {
        for (int i = 0; i < _observers.Count; i++)
        {
            _observers[i].OnLogNeeded(this, fromFloor, toFloor, status, source);
        }
    }
}
