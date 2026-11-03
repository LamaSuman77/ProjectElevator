namespace ElevatorSystem;

/// <summary>
/// Delegate used when the car passes or reaches a floor.
/// The form can also subscribe to this event as well as using IElevatorObserver.
/// </summary>
public delegate void FloorChangedHandler(int floor, string direction);

/// <summary>
/// Delegate used when the doors change (opening, open, closing, closed).
/// </summary>
public delegate void DoorStateChangedHandler(int elevatorId, string doorState);

/// <summary>
/// One elevator car: state, queue, animation timer, and observer list.
/// The car Y and door openness are numbers. BuildingView only draws them.
/// </summary>
public class Elevator
{
    private readonly int[] _floorYPositions;
    private readonly List<IElevatorObserver> _observers;
    private readonly Queue<int> _stopQueue;
    private readonly HashSet<int> _litFloors;
    private readonly System.Windows.Forms.Timer _timer;

    private IElevatorState _state;
    private int _doorTickCount;
    private int _lastAnnouncedFloor;
    private string _tripSource;
    private bool _emergencyPending;
    private string _emergencySource;
    private int _carY;
    private float _doorOpenness;

    /// <summary>
    /// Raised when the car passes or arrives at a floor (Task 4).
    /// </summary>
    public event FloorChangedHandler FloorChanged;

    /// <summary>
    /// Raised when the doors start opening, stay open, start closing, or finish closed.
    /// </summary>
    public event DoorStateChangedHandler DoorStateChanged;

    public int ElevatorId { get; }
    public string Name { get; }
    public int CurrentFloor { get; private set; }
    public int TargetFloor { get; private set; }
    public string Direction { get; private set; }
    public int TripFromFloor { get; private set; }

    /// <summary>
    /// True while a passenger is actively in the doorway boarding or exiting.
    /// While true, doors will not begin closing.
    /// </summary>
    public bool IsBoardingOrExiting { get; set; }

    /// <summary>
    /// Floor where the elevator is currently stopped (1, 2, or 3).
    /// Returns 0 while the elevator is moving between floors.
    /// </summary>
    public int StoppedFloor
    {
        get
        {
            if (IsMoving)
            {
                return 0;
            }

            return CurrentFloor;
        }
    }

    /// <summary>
    /// Pixel Y of the top of the car. BuildingView paints the glass car here.
    /// </summary>
    public int CarY
    {
        get { return _carY; }
    }

    /// <summary>
    /// 0 = doors fully closed, 1 = doors fully open. Only the current floor draws this.
    /// </summary>
    public float DoorOpenness
    {
        get { return _doorOpenness; }
    }

    /// <summary>
    /// Short door word for the hall lanterns and the status line.
    /// </summary>
    public string CurrentDoorState
    {
        get
        {
            if (_state is DoorsOpeningState)
            {
                return AppConstants.DoorStateOpening;
            }

            if (_state is DoorsOpenState)
            {
                return AppConstants.DoorStateOpen;
            }

            if (_state is DoorsClosingState)
            {
                return AppConstants.DoorStateClosing;
            }

            return AppConstants.DoorStateClosed;
        }
    }

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
    /// Creates one elevator, places its car on floor 1, and starts in Idle with closed doors.
    /// floorYPositions is 1-based: index 1 is floor 1, index 3 is floor 3.
    /// </summary>
    public Elevator(int elevatorId, string name, int[] floorYPositions)
    {
        ElevatorId = elevatorId;
        Name = name;
        _floorYPositions = floorYPositions;
        _observers = new List<IElevatorObserver>();
        _stopQueue = new Queue<int>();
        _litFloors = new HashSet<int>();
        _state = new IdleState();
        CurrentFloor = 1;
        TargetFloor = 1;
        Direction = AppConstants.DirectionIdle;
        _tripSource = "";
        _emergencySource = "";
        _emergencyPending = false;
        _lastAnnouncedFloor = 1;
        _doorOpenness = 0f;
        _carY = _floorYPositions[1];
        IsBoardingOrExiting = false;

        _timer = new System.Windows.Forms.Timer();
        _timer.Interval = AppConstants.TimerIntervalMs;
        _timer.Tick += Timer_Tick;
    }

    /// <summary>
    /// Copies fresh floor Y values from the building picture so the car lines up on every floor.
    /// </summary>
    public void SetFloorYPositions(int[] floorYPositions)
    {
        if (floorYPositions == null || floorYPositions.Length < AppConstants.TOTAL_FLOORS + 1)
        {
            return;
        }

        for (int floor = 1; floor <= AppConstants.TOTAL_FLOORS; floor++)
        {
            _floorYPositions[floor] = floorYPositions[floor];
        }

        if (!IsMoving)
        {
            _carY = _floorYPositions[CurrentFloor];
        }
    }

    /// <summary>
    /// Called when the boy has finished stepping into the car. Doors can close by themselves now.
    /// </summary>
    public void PassengerFinishedBoarding()
    {
        IsBoardingOrExiting = false;
        if (_state is DoorsOpenState)
        {
            BeginDoorClosing(AppConstants.SOURCE_DOORS);
        }
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
    /// True when that floor button on this car's panel should glow amber.
    /// </summary>
    public bool IsFloorButtonLit(int floor)
    {
        return _litFloors.Contains(floor);
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

        _litFloors.Add(targetFloor);
        NotifyFloorChanged();
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
        _litFloors.Clear();
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
        _doorOpenness = 0f;
        _state = new IdleState();
        Direction = AppConstants.DirectionIdle;
        NotifyDoorState(AppConstants.DoorStateClosed);
        NotifyLog(CurrentFloor, CurrentFloor, AppConstants.STATUS_RESET, source);
        NotifyStatus("Idle at Floor " + CurrentFloor);
        NotifyFloorChanged();
    }

    /// <summary>
    /// Door Open button. Does nothing while the car is moving.
    /// </summary>
    public void RequestDoorOpen(string source)
    {
        if (IsInEmergency)
        {
            return;
        }

        if (IsMoving)
        {
            NotifyStatus(AppConstants.StatusDoorsLocked);
            return;
        }

        if (_state is DoorsOpenState)
        {
            HoldDoorsOpen();
            return;
        }

        if (_state is DoorsClosingState || _state is IdleState)
        {
            BeginDoorOpening(source);
            return;
        }

        if (_state is DoorsOpeningState)
        {
            return;
        }

        BeginDoorOpening(source);
    }

    /// <summary>
    /// Door Close button. Skips the rest of the dwell if the doors are already open.
    /// Never closes on a passenger who is boarding or exiting.
    /// </summary>
    public void RequestDoorClose(string source)
    {
        if (IsInEmergency)
        {
            return;
        }

        if (IsMoving)
        {
            NotifyStatus(AppConstants.StatusDoorsLocked);
            return;
        }

        if (IsBoardingOrExiting)
        {
            return;
        }

        if (_state is DoorsOpenState)
        {
            BeginDoorClosing(source);
        }
    }

    /// <summary>
    /// Alarm button inside this car. Writes an Alarm log row. The form shows the message box.
    /// </summary>
    public void PressAlarm(string source)
    {
        NotifyLog(CurrentFloor, CurrentFloor, AppConstants.StatusAlarm, source);
        NotifyStatus("Alarm pressed in Elevator " + Name);
    }

    /// <summary>
    /// Starts a trip. Called only when the doors are fully closed. Not used while already moving.
    /// </summary>
    public void StartTrip(int targetFloor, string source)
    {
        if (_doorOpenness > 0.01f)
        {
            QueueRequest(targetFloor, source);
            return;
        }

        TripFromFloor = CurrentFloor;
        TargetFloor = targetFloor;
        _tripSource = source;
        _lastAnnouncedFloor = CurrentFloor;
        _doorTickCount = 0;
        _doorOpenness = 0f;

        if (targetFloor > CurrentFloor)
        {
            Direction = AppConstants.DirectionUp;
            _state = new MovingUpState();
        }
        else if (targetFloor < CurrentFloor)
        {
            Direction = AppConstants.DirectionDown;
            _state = new MovingDownState();
        }
        else
        {
            BeginDoorOpening(source);
            return;
        }

        NotifyLog(TripFromFloor, TargetFloor, AppConstants.StatusMoving, source);
        NotifyStatus("Floor " + TripFromFloor + " -> Floor " + TargetFloor + " (" + Direction + ")");
        NotifyFloorChanged();
        _timer.Start();
    }

    /// <summary>
    /// Stores a floor to visit after the current trip. Ignores duplicates so spam is safe.
    /// </summary>
    public void QueueRequest(int targetFloor, string source)
    {
        if (targetFloor == TargetFloor && IsMoving)
        {
            return;
        }

        if (_stopQueue.Contains(targetFloor))
        {
            return;
        }

        _stopQueue.Enqueue(targetFloor);

        if (IsMoving)
        {
            NotifyStatus("Floor " + TripFromFloor + " -> Floor " + TargetFloor + " (" + Direction + ")" +
                         " (queued floor " + targetFloor + " from " + source + ")");
        }
        else
        {
            NotifyStatus("Queued floor " + targetFloor);
        }
    }

    /// <summary>
    /// Starts the doors sliding open. Safe to call again while they are already opening.
    /// </summary>
    public void BeginDoorOpening(string source)
    {
        if (IsMoving)
        {
            NotifyStatus(AppConstants.StatusDoorsLocked);
            return;
        }

        if (_state is DoorsOpeningState)
        {
            return;
        }

        _tripSource = source;
        _doorTickCount = 0;
        _litFloors.Remove(CurrentFloor);
        _state = new DoorsOpeningState();
        NotifyDoorState(AppConstants.DoorStateOpening);
        NotifyStatus("Doors opening");
        _timer.Start();
    }

    /// <summary>
    /// Restarts the 3 second dwell so the doors stay open a bit longer.
    /// </summary>
    public void HoldDoorsOpen()
    {
        _doorOpenness = 1f;
        _doorTickCount = 0;
        _litFloors.Remove(CurrentFloor);
        _state = new DoorsOpenState();
        NotifyDoorState(AppConstants.DoorStateOpen);
        NotifyStatus("Doors open");
        _timer.Start();
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
    /// One timer for the car AND the doors. Never starts a second timer.
    /// The car only moves while the doors are fully closed.
    /// </summary>
    private void Timer_Tick(object sender, EventArgs e)
    {
        if (_state is DoorsOpeningState)
        {
            TickDoorsOpening();
            NotifyAnimationTick();
            return;
        }

        if (_state is DoorsOpenState)
        {
            TickDoorsOpen();
            NotifyAnimationTick();
            return;
        }

        if (_state is DoorsClosingState)
        {
            TickDoorsClosing();
            NotifyAnimationTick();
            return;
        }

        if (!IsMoving)
        {
            return;
        }

        if (_doorOpenness > 0.01f)
        {
            return;
        }

        int targetY = _floorYPositions[TargetFloor];
        int step = AppConstants.PixelStep;

        if (_carY > targetY)
        {
            _carY = Math.Max(targetY, _carY - step);
        }
        else if (_carY < targetY)
        {
            _carY = Math.Min(targetY, _carY + step);
        }

        AnnounceIfPassingFloor();
        NotifyAnimationTick();

        // Check if we arrived at an intermediate queued floor along the way
        if (IsMoving && _stopQueue.Count > 0)
        {
            for (int floor = 1; floor <= AppConstants.TOTAL_FLOORS; floor++)
            {
                if (floor != TargetFloor && _carY == _floorYPositions[floor] && _stopQueue.Contains(floor))
                {
                    StopAtIntermediateFloor(floor);
                    return;
                }
            }
        }

        if (_carY == targetY)
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
    /// Stops at an intermediate floor that was queued, opening doors there first.
    /// </summary>
    private void StopAtIntermediateFloor(int floor)
    {
        int remainingTarget = TargetFloor;
        RemoveFromQueue(floor);
        if (!_stopQueue.Contains(remainingTarget))
        {
            Queue<int> temp = new Queue<int>();
            temp.Enqueue(remainingTarget);
            while (_stopQueue.Count > 0)
            {
                temp.Enqueue(_stopQueue.Dequeue());
            }
            while (temp.Count > 0)
            {
                _stopQueue.Enqueue(temp.Dequeue());
            }
        }

        CurrentFloor = floor;
        _lastAnnouncedFloor = floor;
        TargetFloor = floor;
        ArriveAtTarget();
    }

    /// <summary>
    /// Removes all instances of a floor from the stop queue.
    /// </summary>
    private void RemoveFromQueue(int floor)
    {
        Queue<int> temp = new Queue<int>();
        while (_stopQueue.Count > 0)
        {
            int item = _stopQueue.Dequeue();
            if (item != floor)
            {
                temp.Enqueue(item);
            }
        }
        while (temp.Count > 0)
        {
            _stopQueue.Enqueue(temp.Dequeue());
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
    /// Stops the timer and stays in EmergencyState. Doors are treated as open.
    /// </summary>
    private void EnterEmergencyStop()
    {
        _emergencyPending = false;
        _timer.Stop();
        _state = new EmergencyState();
        Direction = AppConstants.DirectionIdle;
        _doorOpenness = 1f;
        NotifyDoorState(AppConstants.DoorStateOpen);
        NotifyLog(CurrentFloor, CurrentFloor, AppConstants.STATUS_EMERGENCY, _emergencySource);
        NotifyStatus("EMERGENCY - stopped at Floor " + CurrentFloor);
        NotifyFloorChanged();
        NotifyAnimationTick();
    }

    /// <summary>
    /// Updates CurrentFloor when the car lines up with a floor on the way.
    /// </summary>
    private void AnnounceIfPassingFloor()
    {
        for (int floor = 1; floor <= AppConstants.TOTAL_FLOORS; floor++)
        {
            int gap = Math.Abs(_carY - _floorYPositions[floor]);
            if (gap <= AppConstants.PixelStep && floor != _lastAnnouncedFloor)
            {
                CurrentFloor = floor;
                _lastAnnouncedFloor = floor;
                NotifyFloorChanged();
            }
        }
    }

    /// <summary>
    /// Stops the car, logs Arrived, then starts opening the doors. The car cannot move again yet.
    /// </summary>
    private void ArriveAtTarget()
    {
        CurrentFloor = TargetFloor;
        _lastAnnouncedFloor = CurrentFloor;
        _litFloors.Remove(CurrentFloor);
        Direction = AppConstants.DirectionIdle;
        NotifyLog(TripFromFloor, CurrentFloor, AppConstants.StatusArrived, _tripSource);
        NotifyFloorChanged();
        NotifyStatus("Arrived at Floor " + CurrentFloor);
        BeginDoorOpening(AppConstants.SOURCE_DOORS);
    }

    /// <summary>
    /// Moves door openness from 0 toward 1 (about one second).
    /// </summary>
    private void TickDoorsOpening()
    {
        _doorOpenness += 1f / AppConstants.DoorOpenTicks;
        if (_doorOpenness >= 1f)
        {
            _doorOpenness = 1f;
            _doorTickCount = 0;
            _state = new DoorsOpenState();
            NotifyDoorState(AppConstants.DoorStateOpen);
            NotifyLog(CurrentFloor, CurrentFloor, AppConstants.StatusDoorsOpened, AppConstants.SOURCE_DOORS);
            NotifyStatus("Doors open");
        }
    }

    /// <summary>
    /// Waits about three seconds with the doors fully open.
    /// Holds dwell at 0 while a passenger is actively boarding or exiting.
    /// </summary>
    private void TickDoorsOpen()
    {
        if (IsBoardingOrExiting)
        {
            _doorTickCount = 0;
            return;
        }

        _doorTickCount++;
        if (_doorTickCount >= AppConstants.DoorDwellTicks)
        {
            BeginDoorClosing(AppConstants.SOURCE_DOORS);
        }
    }

    /// <summary>
    /// Starts the doors sliding shut.
    /// </summary>
    private void BeginDoorClosing(string source)
    {
        _tripSource = source;
        _state = new DoorsClosingState();
        NotifyDoorState(AppConstants.DoorStateClosing);
        NotifyStatus("Doors closing");
        _timer.Start();
    }

    /// <summary>
    /// Moves door openness from 1 toward 0 (about one second), then idle or the next queued floor.
    /// </summary>
    private void TickDoorsClosing()
    {
        _doorOpenness -= 1f / AppConstants.DoorCloseTicks;
        if (_doorOpenness > 0f)
        {
            return;
        }

        _doorOpenness = 0f;
        NotifyDoorState(AppConstants.DoorStateClosed);
        NotifyLog(CurrentFloor, CurrentFloor, AppConstants.StatusDoorsClosed, AppConstants.SOURCE_DOORS);

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

    /// <summary>
    /// Raises DoorStateChanged and tells observers, so the building view can redraw the doors.
    /// </summary>
    private void NotifyDoorState(string doorState)
    {
        if (DoorStateChanged != null)
        {
            DoorStateChanged(ElevatorId, doorState);
        }

        for (int i = 0; i < _observers.Count; i++)
        {
            _observers[i].OnDoorStateChanged(this, doorState);
        }
    }

    /// <summary>
    /// Tells observers one animation frame happened (car or doors).
    /// </summary>
    private void NotifyAnimationTick()
    {
        for (int i = 0; i < _observers.Count; i++)
        {
            _observers[i].OnAnimationTick(this);
        }
    }
}
