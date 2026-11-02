using System.ComponentModel;
using System.Data;

namespace ElevatorSystem;

/// <summary>
/// Main window. It draws the building, wires the buttons, and updates labels.
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
    private readonly Queue<LogEntry> _pendingSaves = new Queue<LogEntry>();
    private Label[] _floorLabelsA;
    private Label[] _floorLabelsB;
    private Image _carImageA;
    private Image _carImageB;

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

        int[] floorY = BuildFloorYPositions();

        _elevatorA = new Elevator(AppConstants.ElevatorIdA, AppConstants.ElevatorNameA, pnlCarA, floorY);
        _elevatorB = new Elevator(AppConstants.ElevatorIdB, AppConstants.ElevatorNameB, pnlCarB, floorY);
        _controller = new ElevatorController(_elevatorA, _elevatorB);

        _elevatorA.AttachObserver(this);
        _elevatorB.AttachObserver(this);

        // Task 4 also asks for a delegate/event. The form listens as well as using Observer.
        _elevatorA.FloorChanged += ElevatorA_FloorChanged;
        _elevatorB.FloorChanged += ElevatorB_FloorChanged;

        _floorLabelsA = new Label[] { null, lblFloorStatusA1, lblFloorStatusA2, lblFloorStatusA3 };
        _floorLabelsB = new Label[] { null, lblFloorStatusB1, lblFloorStatusB2, lblFloorStatusB3 };

        SetupBackgroundWorkers();
        LoadCarImages();
        RefreshFloorDisplays(_elevatorA);
        RefreshFloorDisplays(_elevatorB);
        pnlCarA.BringToFront();
        pnlCarB.BringToFront();
    }

    /// <summary>
    /// Loads ElevatorA.png and ElevatorB.png from the Images folder next to the exe.
    /// Uses a relative path only. If a file is missing, one message is shown and the coloured letter is kept.
    /// </summary>
    private void LoadCarImages()
    {
        try
        {
            string folder = Path.Combine(AppContext.BaseDirectory, AppConstants.IMAGES_FOLDER);
            string pathA = Path.Combine(folder, AppConstants.CAR_A_IMAGE);
            string pathB = Path.Combine(folder, AppConstants.CAR_B_IMAGE);
            bool loadedOk = true;

            if (File.Exists(pathA))
            {
                _carImageA = new Bitmap(pathA);
                picCarA.Image = _carImageA;
                lblCarA.Visible = false;
                picCarA.BringToFront();
            }
            else
            {
                loadedOk = false;
                lblCarA.BringToFront();
            }

            if (File.Exists(pathB))
            {
                _carImageB = new Bitmap(pathB);
                picCarB.Image = _carImageB;
                lblCarB.Visible = false;
                picCarB.BringToFront();
            }
            else
            {
                loadedOk = false;
                lblCarB.BringToFront();
            }

            if (!loadedOk)
            {
                MessageBox.Show(
                    "Could not load the elevator pictures. The cars will use the coloured squares with letters instead.",
                    "Elevator pictures",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        catch (Exception)
        {
            MessageBox.Show(
                "Could not load the elevator pictures. The cars will use the coloured squares with letters instead.",
                "Elevator pictures",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    /// <summary>
    /// Frees the bitmaps we created so they do not stay in memory after the window closes.
    /// </summary>
    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (picCarA.Image != null)
        {
            picCarA.Image = null;
        }

        if (picCarB.Image != null)
        {
            picCarB.Image = null;
        }

        if (_carImageA != null)
        {
            _carImageA.Dispose();
            _carImageA = null;
        }

        if (_carImageB != null)
        {
            _carImageB.Dispose();
            _carImageB = null;
        }
    }

    /// <summary>
    /// Calculates the Y pixel for each floor inside the shaft (floor 1 at the bottom).
    /// </summary>
    private int[] BuildFloorYPositions()
    {
        int[] positions = new int[AppConstants.TOTAL_FLOORS + 1];
        int carHeight = pnlCarA.Height;
        positions[3] = 20;
        positions[2] = (pnlShaftA.Height - carHeight) / 2;
        positions[1] = pnlShaftA.Height - carHeight - 20;
        return positions;
    }

    /// <summary>
    /// Creates one worker for saving rows and one worker for loading the grid.
    /// </summary>
    private void SetupBackgroundWorkers()
    {
        _saveWorker = new BackgroundWorker();
        _saveWorker.DoWork += SaveWorker_DoWork;
        _saveWorker.RunWorkerCompleted += SaveWorker_RunWorkerCompleted;

        _loadWorker = new BackgroundWorker();
        _loadWorker.DoWork += LoadWorker_DoWork;
        _loadWorker.RunWorkerCompleted += LoadWorker_RunWorkerCompleted;
    }

    /// <summary>
    /// Hall button for floor 1. Asks the controller to pick an elevator.
    /// </summary>
    private void btnRequestFloor1_Click(object sender, EventArgs e)
    {
        _controller.HandleHallRequest(1);
    }

    /// <summary>
    /// Hall button for floor 2.
    /// </summary>
    private void btnRequestFloor2_Click(object sender, EventArgs e)
    {
        _controller.HandleHallRequest(2);
    }

    /// <summary>
    /// Hall button for floor 3.
    /// </summary>
    private void btnRequestFloor3_Click(object sender, EventArgs e)
    {
        _controller.HandleHallRequest(3);
    }

    /// <summary>
    /// Panel A button 1. Only elevator A handles this.
    /// </summary>
    private void btnPanelAFloor1_Click(object sender, EventArgs e)
    {
        _elevatorA.MoveElevatorTo(1, "Panel Button F1");
    }

    /// <summary>
    /// Panel A button 2.
    /// </summary>
    private void btnPanelAFloor2_Click(object sender, EventArgs e)
    {
        _elevatorA.MoveElevatorTo(2, "Panel Button F2");
    }

    /// <summary>
    /// Panel A button 3.
    /// </summary>
    private void btnPanelAFloor3_Click(object sender, EventArgs e)
    {
        _elevatorA.MoveElevatorTo(3, "Panel Button F3");
    }

    /// <summary>
    /// Panel B button 1. Only elevator B handles this.
    /// </summary>
    private void btnPanelBFloor1_Click(object sender, EventArgs e)
    {
        _elevatorB.MoveElevatorTo(1, "Panel Button F1");
    }

    /// <summary>
    /// Panel B button 2.
    /// </summary>
    private void btnPanelBFloor2_Click(object sender, EventArgs e)
    {
        _elevatorB.MoveElevatorTo(2, "Panel Button F2");
    }

    /// <summary>
    /// Panel B button 3.
    /// </summary>
    private void btnPanelBFloor3_Click(object sender, EventArgs e)
    {
        _elevatorB.MoveElevatorTo(3, "Panel Button F3");
    }

    /// <summary>
    /// Emergency button for car A. The elevator decides whether to stop now or at the next floor.
    /// </summary>
    private void btnEmergencyA_Click(object sender, EventArgs e)
    {
        _elevatorA.TriggerEmergency(AppConstants.SOURCE_EMERGENCY_A);
    }

    /// <summary>
    /// Reset button for car A. Puts A back to idle if it is in emergency.
    /// </summary>
    private void btnResetA_Click(object sender, EventArgs e)
    {
        _elevatorA.ResetEmergency(AppConstants.SOURCE_RESET_A);
    }

    /// <summary>
    /// Emergency button for car B.
    /// </summary>
    private void btnEmergencyB_Click(object sender, EventArgs e)
    {
        _elevatorB.TriggerEmergency(AppConstants.SOURCE_EMERGENCY_B);
    }

    /// <summary>
    /// Reset button for car B.
    /// </summary>
    private void btnResetB_Click(object sender, EventArgs e)
    {
        _elevatorB.ResetEmergency(AppConstants.SOURCE_RESET_B);
    }

    /// <summary>
    /// Loads historical records on a background thread so the cars keep moving.
    /// </summary>
    private void btnLog_Click(object sender, EventArgs e)
    {
        if (_loadWorker.IsBusy)
        {
            return;
        }

        btnLog.Enabled = false;
        btnLog.Text = "Loading...";
        _loadWorker.RunWorkerAsync();
    }

    /// <summary>
    /// Event subscription for elevator A (Task 4 delegate/event).
    /// </summary>
    private void ElevatorA_FloorChanged(int floor, string direction)
    {
        UpdatePanelDisplay(lblCurrentFloorA, lblArrowA, floor, direction, _elevatorA.IsInEmergency);
    }

    /// <summary>
    /// Event subscription for elevator B (Task 4 delegate/event).
    /// </summary>
    private void ElevatorB_FloorChanged(int floor, string direction)
    {
        UpdatePanelDisplay(lblCurrentFloorB, lblArrowB, floor, direction, _elevatorB.IsInEmergency);
    }

    /// <summary>
    /// Observer: refresh the three floor lights and the control-panel number.
    /// </summary>
    public void OnFloorChanged(Elevator elevator, int floor, string direction)
    {
        if (elevator.ElevatorId == AppConstants.ElevatorIdA)
        {
            UpdatePanelDisplay(lblCurrentFloorA, lblArrowA, floor, direction, elevator.IsInEmergency);
            RefreshFloorDisplays(elevator);
        }
        else
        {
            UpdatePanelDisplay(lblCurrentFloorB, lblArrowB, floor, direction, elevator.IsInEmergency);
            RefreshFloorDisplays(elevator);
        }
    }

    /// <summary>
    /// Observer: write the movement sentence under the shafts.
    /// </summary>
    public void OnStatusChanged(Elevator elevator, string statusText)
    {
        if (elevator.ElevatorId == AppConstants.ElevatorIdA)
        {
            lblStatusA.Text = statusText;
            lblStatusA.ForeColor = elevator.IsInEmergency ? Color.Red : SystemColors.ControlText;
        }
        else
        {
            lblStatusB.Text = statusText;
            lblStatusB.ForeColor = elevator.IsInEmergency ? Color.Red : SystemColors.ControlText;
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
    /// Highlights the floor where the car currently is.
    /// </summary>
    private void RefreshFloorDisplays(Elevator elevator)
    {
        Label[] labels = elevator.ElevatorId == AppConstants.ElevatorIdA ? _floorLabelsA : _floorLabelsB;
        string arrow = elevator.IsInEmergency ? AppConstants.DISPLAY_EMERGENCY : ArrowFor(elevator.Direction);

        for (int floor = 1; floor <= AppConstants.TOTAL_FLOORS; floor++)
        {
            bool here = floor == elevator.CurrentFloor;
            labels[floor].Text = here
                ? "Elevator " + elevator.Name + "\nFloor " + floor + " " + arrow
                : "Floor " + floor;
            labels[floor].BackColor = here
                ? (elevator.IsInEmergency ? Color.FromArgb(255, 205, 210) : Color.FromArgb(255, 236, 150))
                : Color.White;
            labels[floor].ForeColor = here && elevator.IsInEmergency ? Color.Red : Color.Black;
            labels[floor].Font = new Font("Segoe UI", here ? 9F : 9F, here ? FontStyle.Bold : FontStyle.Regular);
        }
    }

    /// <summary>
    /// Updates the black display window and the up/down arrow.
    /// </summary>
    private void UpdatePanelDisplay(Label floorLabel, Label arrowLabel, int floor, string direction, bool emergency)
    {
        floorLabel.Text = floor.ToString();
        arrowLabel.Text = emergency ? AppConstants.DISPLAY_EMERGENCY : ArrowFor(direction);
        arrowLabel.ForeColor = emergency ? Color.Red : Color.Lime;
    }

    /// <summary>
    /// Turns direction text into a simple arrow for the displays.
    /// </summary>
    private static string ArrowFor(string direction)
    {
        if (direction == AppConstants.DirectionUp)
        {
            return "▲";
        }

        if (direction == AppConstants.DirectionDown)
        {
            return "▼";
        }

        return "-";
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
        btnLog.Enabled = true;
        btnLog.Text = "Log";

        if (e.Error != null)
        {
            MessageBox.Show("Could not load the log.\n\n" + e.Error.Message, "Log",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DataTable table = e.Result as DataTable;
        dgvLog.DataSource = table;
    }
}
