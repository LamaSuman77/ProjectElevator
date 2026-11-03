using System.ComponentModel;
using System.Data;

namespace ElevatorSystem;

/// <summary>
/// Main window. It wires the building view, the two car panels, status labels and the log grid.
/// All elevator rules live in Elevator / ElevatorController. All SQL lives in DatabaseHelper.
/// </summary>
public partial class Form1 : Form, IElevatorObserver
{
    private DatabaseHelper _database;
    private Elevator _elevatorA;
    private Elevator _elevatorB;
    private ElevatorController _controller;
    private BackgroundWorker _saveWorker;
    private BackgroundWorker _loadWorker;
    private BackgroundWorker _clearWorker;
    private readonly Queue<LogEntry> _pendingSaves = new Queue<LogEntry>();

    /// <summary>
    /// Builds the window controls.
    /// </summary>
    public Form1()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Sets up the database, the two elevators, observers, and background workers.
    /// </summary>
    private void Form1_Load(object sender, EventArgs e)
    {
        _database = new DatabaseHelper();
        _database.InitializeDatabase();
        lblDataPath.Text = "Database file: Data\\ElevatorLog.mdf  (folder: " + _database.GetDataFolder() + ")";

        int[] floorY = buildingView.GetFloorYPositions();

        _elevatorA = new Elevator(AppConstants.ElevatorIdA, AppConstants.ElevatorNameA, floorY);
        _elevatorB = new Elevator(AppConstants.ElevatorIdB, AppConstants.ElevatorNameB, floorY);
        _controller = new ElevatorController(_elevatorA, _elevatorB);

        buildingView.Bind(_elevatorA, _elevatorB);
        buildingView.HallCall += BuildingView_HallCall;
        buildingView.PassengerBoarded += BuildingView_PassengerBoarded;
        buildingView.PassengerExited += BuildingView_PassengerExited;

        _elevatorA.AttachObserver(this);
        _elevatorB.AttachObserver(this);

        _elevatorA.FloorChanged += ElevatorA_FloorChanged;
        _elevatorB.FloorChanged += ElevatorB_FloorChanged;
        _elevatorA.DoorStateChanged += Elevator_DoorStateChanged;
        _elevatorB.DoorStateChanged += Elevator_DoorStateChanged;

        WirePanel(panelA, _elevatorA, AppConstants.SOURCE_ALARM_A, AppConstants.SOURCE_EMERGENCY_A, AppConstants.SOURCE_RESET_A);
        WirePanel(panelB, _elevatorB, AppConstants.SOURCE_ALARM_B, AppConstants.SOURCE_EMERGENCY_B, AppConstants.SOURCE_RESET_B);

        SetupBackgroundWorkers();
        panelA.ApplyElevator(_elevatorA);
        panelB.ApplyElevator(_elevatorB);
        buildingView.Invalidate();
    }

    /// <summary>
    /// Connects one car panel so every button only calls a method on that elevator.
    /// </summary>
    private void WirePanel(ElevatorPanel panel, Elevator elevator, string alarmSource, string emergencySource, string resetSource)
    {
        panel.FloorRequested += floor =>
        {
            // If a passenger is inside riding, set their destination floor
            if (AppConstants.ENABLE_PASSENGER)
            {
                buildingView.SetPassengerDestination(elevator.ElevatorId, floor);
            }

            elevator.MoveElevatorTo(floor, "Panel Button F" + floor);
        };
        panel.DoorOpenClicked += (s, e) =>
        {
            elevator.RequestDoorOpen(AppConstants.SOURCE_DOOR_OPEN);
            if (AppConstants.ENABLE_PASSENGER && !elevator.IsMoving)
            {
                buildingView.SpawnPassenger(elevator.CurrentFloor, elevator.ElevatorId);
            }
        };
        panel.DoorCloseClicked += (s, e) => elevator.RequestDoorClose(AppConstants.SOURCE_DOOR_CLOSE);
        panel.AlarmClicked += (s, e) =>
        {
            elevator.PressAlarm(alarmSource);
            MessageBox.Show("Alarm pressed in Elevator " + elevator.Name, "Alarm",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        };
        panel.EmergencyClicked += (s, e) => elevator.TriggerEmergency(emergencySource);
        panel.ResetClicked += (s, e) => elevator.ResetEmergency(resetSource);
    }

    /// <summary>
    /// Outside hall button pressed: controller assigns best car, and spawns passenger.
    /// </summary>
    private void BuildingView_HallCall(int floor, string direction)
    {
        string source = "Hall " + direction + " F" + floor;
        Elevator chosen = _controller.ChooseElevator(floor);
        bool taken = _controller.HandleHallRequest(floor, source);

        if (taken)
        {
            if (AppConstants.ENABLE_PASSENGER && chosen != null)
            {
                buildingView.SpawnPassenger(floor, chosen.ElevatorId);
            }
        }
        else
        {
            buildingView.ClearHallLights(floor);
        }
    }

    /// <summary>
    /// Passenger has stepped inside an elevator: shows hint label and logs event.
    /// </summary>
    private void BuildingView_PassengerBoarded(int elevatorId)
    {
        Elevator elevator = elevatorId == AppConstants.ElevatorIdA ? _elevatorA : _elevatorB;
        if (elevator != null)
        {
            if (elevatorId == AppConstants.ElevatorIdA)
            {
                lblStatusA.Text = "Elevator A: " + AppConstants.HintPassengerInside;
            }
            else
            {
                lblStatusB.Text = "Elevator B: " + AppConstants.HintPassengerInside;
            }

            OnLogNeeded(elevator, elevator.CurrentFloor, elevator.CurrentFloor,
                AppConstants.StatusPassengerBoarded, "Passenger " + elevator.Name);
        }
    }

    /// <summary>
    /// Passenger has exited the elevator and left the walkway: logs event.
    /// </summary>
    private void BuildingView_PassengerExited(int elevatorId)
    {
        Elevator elevator = elevatorId == AppConstants.ElevatorIdA ? _elevatorA : _elevatorB;
        if (elevator != null)
        {
            OnLogNeeded(elevator, elevator.CurrentFloor, elevator.CurrentFloor,
                AppConstants.StatusPassengerExited, "Passenger " + elevator.Name);
        }
    }

    /// <summary>
    /// Form closing handler.
    /// </summary>
    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
    }

    /// <summary>
    /// Creates background workers for saving, loading, and clearing log rows.
    /// </summary>
    private void SetupBackgroundWorkers()
    {
        _saveWorker = new BackgroundWorker();
        _saveWorker.DoWork += SaveWorker_DoWork;
        _saveWorker.RunWorkerCompleted += SaveWorker_RunWorkerCompleted;

        _loadWorker = new BackgroundWorker();
        _loadWorker.DoWork += LoadWorker_DoWork;
        _loadWorker.RunWorkerCompleted += LoadWorker_RunWorkerCompleted;

        _clearWorker = new BackgroundWorker();
        _clearWorker.DoWork += ClearWorker_DoWork;
        _clearWorker.RunWorkerCompleted += ClearWorker_RunWorkerCompleted;
    }

    /// <summary>
    /// Enables or disables the database log buttons while background workers run.
    /// </summary>
    private void SetLogButtonsEnabled(bool enabled)
    {
        btnLog.Enabled = enabled;
        btnRefreshLog.Enabled = enabled;
        btnClearLog.Enabled = enabled;
    }

    /// <summary>
    /// Loads historical records on a background thread so the cars keep moving.
    /// </summary>
    private void btnLog_Click(object sender, EventArgs e)
    {
        if (_loadWorker.IsBusy || _clearWorker.IsBusy)
        {
            return;
        }

        SetLogButtonsEnabled(false);
        btnLog.Text = "Loading...";
        _loadWorker.RunWorkerAsync();
    }

    /// <summary>
    /// Refreshes the log DataGridView with the latest database records on a background thread.
    /// </summary>
    private void btnRefreshLog_Click(object sender, EventArgs e)
    {
        if (_loadWorker.IsBusy || _clearWorker.IsBusy)
        {
            return;
        }

        SetLogButtonsEnabled(false);
        btnRefreshLog.Text = "Refreshing...";
        _loadWorker.RunWorkerAsync();
    }

    /// <summary>
    /// Prompts the user and deletes all records from the database table on a background thread.
    /// </summary>
    private void btnClearLog_Click(object sender, EventArgs e)
    {
        if (_loadWorker.IsBusy || _clearWorker.IsBusy)
        {
            return;
        }

        DialogResult result = MessageBox.Show(
            "Are you sure you want to clear the entire elevator log history?",
            "Clear Log",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
        {
            return;
        }

        SetLogButtonsEnabled(false);
        btnClearLog.Text = "Clearing...";
        _clearWorker.RunWorkerAsync();
    }

    /// <summary>
    /// Event subscription for elevator A (Task 4 delegate/event).
    /// </summary>
    private void ElevatorA_FloorChanged(int floor, string direction)
    {
        panelA.ApplyElevator(_elevatorA);
        buildingView.Invalidate();
    }

    /// <summary>
    /// Event subscription for elevator B (Task 4 delegate/event).
    /// </summary>
    private void ElevatorB_FloorChanged(int floor, string direction)
    {
        panelB.ApplyElevator(_elevatorB);
        buildingView.Invalidate();
    }

    /// <summary>
    /// DoorStateChanged delegate: when doors open at a floor, the hall lights on that floor go out.
    /// </summary>
    private void Elevator_DoorStateChanged(int elevatorId, string doorState)
    {
        if (doorState == AppConstants.DoorStateOpening || doorState == AppConstants.DoorStateOpen)
        {
            Elevator elevator = elevatorId == AppConstants.ElevatorIdA ? _elevatorA : _elevatorB;
            buildingView.ClearHallLights(elevator.CurrentFloor);
        }

        buildingView.Invalidate();
    }

    /// <summary>
    /// Observer: refresh the car panel display and the hall lanterns.
    /// </summary>
    public void OnFloorChanged(Elevator elevator, int floor, string direction)
    {
        if (elevator.ElevatorId == AppConstants.ElevatorIdA)
        {
            panelA.ApplyElevator(elevator);
        }
        else
        {
            panelB.ApplyElevator(elevator);
        }

        buildingView.Invalidate();
    }

    /// <summary>
    /// Observer: write the movement sentence under the panels.
    /// </summary>
    public void OnStatusChanged(Elevator elevator, string statusText)
    {
        if (elevator.ElevatorId == AppConstants.ElevatorIdA)
        {
            lblStatusA.Text = statusText;
            lblStatusA.ForeColor = elevator.IsInEmergency ? Color.Red : SystemColors.ControlText;
            panelA.ApplyElevator(elevator);
        }
        else
        {
            lblStatusB.Text = statusText;
            lblStatusB.ForeColor = elevator.IsInEmergency ? Color.Red : SystemColors.ControlText;
            panelB.ApplyElevator(elevator);
        }
    }

    /// <summary>
    /// Observer: queue a database save on the BackgroundWorker.
    /// </summary>
    public void OnLogNeeded(Elevator elevator, int fromFloor, int toFloor, string status, string source)
    {
        LogEntry entry = new LogEntry();
        entry.ElevatorId = elevator.ElevatorId;
        entry.FromFloor = fromFloor;
        entry.ToFloor = toFloor;
        entry.Status = status;
        entry.Source = source;
        _pendingSaves.Enqueue(entry);
        StartNextSave();
    }

    /// <summary>
    /// Observer: doors changed. The building view redraws from the elevator values.
    /// </summary>
    public void OnDoorStateChanged(Elevator elevator, string doorState)
    {
        if (elevator.ElevatorId == AppConstants.ElevatorIdA)
        {
            panelA.ApplyElevator(elevator);
        }
        else
        {
            panelB.ApplyElevator(elevator);
        }

        buildingView.Invalidate();
    }

    /// <summary>
    /// Observer: car, doors, or passenger moved one timer step. Redraw the building.
    /// </summary>
    public void OnAnimationTick(Elevator elevator)
    {
        buildingView.TickPassengers();
        buildingView.Invalidate();
    }

    /// <summary>
    /// Starts the save worker if it is free and there are queued rows.
    /// </summary>
    private void StartNextSave()
    {
        if (_saveWorker.IsBusy || _pendingSaves.Count == 0)
        {
            return;
        }

        _saveWorker.RunWorkerAsync(_pendingSaves.Dequeue());
    }

    /// <summary>
    /// Background thread: insert one log row through DatabaseHelper.
    /// </summary>
    private void SaveWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        LogEntry entry = (LogEntry)e.Argument;
        _database.SaveLog(entry.ElevatorId, entry.FromFloor, entry.ToFloor, entry.Status, entry.Source);
    }

    /// <summary>
    /// Back on the UI thread: save the next queued row if there is one.
    /// </summary>
    private void SaveWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        StartNextSave();
    }

    /// <summary>
    /// Background thread: read the whole log table.
    /// </summary>
    private void LoadWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        e.Result = _database.LoadLog();
    }

    /// <summary>
    /// Back on the UI thread: show the table in the grid.
    /// </summary>
    private void LoadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        SetLogButtonsEnabled(true);
        btnLog.Text = "Log";
        btnRefreshLog.Text = "Refresh";

        if (e.Error != null)
        {
            MessageBox.Show("Could not load the log.\n\n" + e.Error.Message, "Log",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DataTable table = e.Result as DataTable;
        dgvLog.DataSource = table;
    }

    /// <summary>
    /// Background thread: clear all log records in the database, then load the empty table schema.
    /// </summary>
    private void ClearWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        bool cleared = _database.ClearLog();
        if (cleared)
        {
            e.Result = _database.LoadLog();
        }
        else
        {
            e.Result = null;
        }
    }

    /// <summary>
    /// Back on the UI thread: updates the grid after clearing the log.
    /// </summary>
    private void ClearWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        SetLogButtonsEnabled(true);
        btnClearLog.Text = "Clear Log";

        if (e.Error != null)
        {
            MessageBox.Show("Could not clear the log.\n\n" + e.Error.Message, "Clear Log",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (e.Result is DataTable emptyTable)
        {
            dgvLog.DataSource = emptyTable;
            MessageBox.Show("The full log has been cleared successfully.", "Clear Log",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
