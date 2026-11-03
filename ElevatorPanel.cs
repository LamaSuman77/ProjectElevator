namespace ElevatorSystem;

/// <summary>
/// One inside control panel, drawn like a real car panel: round floor buttons,
/// door open / door close, alarm, plus Emergency and Reset.
/// </summary>
public class ElevatorPanel : UserControl
{
    private Label _lblTitle;
    private Label _lblFloor;
    private Label _lblArrow;
    private Button _btnFloor1;
    private Button _btnFloor2;
    private Button _btnFloor3;
    private Button _btnDoorOpen;
    private Button _btnDoorClose;
    private Button _btnAlarm;
    private Button _btnEmergency;
    private Button _btnReset;

    private readonly Color _litAmber = Color.FromArgb(255, 191, 0);
    private readonly Color _idleGrey = Color.FromArgb(220, 220, 220);

    public event Action<int> FloorRequested;
    public event EventHandler DoorOpenClicked;
    public event EventHandler DoorCloseClicked;
    public event EventHandler AlarmClicked;
    public event EventHandler EmergencyClicked;
    public event EventHandler ResetClicked;

    /// <summary>
    /// Builds every control on this panel.
    /// </summary>
    public ElevatorPanel()
    {
        Size = new Size(230, 430);
        BackColor = Color.FromArgb(45, 48, 55);
        BorderStyle = BorderStyle.FixedSingle;
        BuildControls();
    }

    /// <summary>
    /// Title on the metal plate, for example "Car A".
    /// </summary>
    public void SetCarName(string name)
    {
        _lblTitle.Text = "Car " + name;
    }

    /// <summary>
    /// Updates the floor window, the arrow, and which round buttons are glowing.
    /// </summary>
    public void ApplyElevator(Elevator elevator)
    {
        _lblFloor.Text = elevator.CurrentFloor.ToString();
        if (elevator.IsInEmergency)
        {
            _lblArrow.Text = AppConstants.DISPLAY_EMERGENCY;
            _lblArrow.ForeColor = Color.Red;
        }
        else if (elevator.Direction == AppConstants.DirectionUp)
        {
            _lblArrow.Text = "▲";
            _lblArrow.ForeColor = Color.Lime;
        }
        else if (elevator.Direction == AppConstants.DirectionDown)
        {
            _lblArrow.Text = "▼";
            _lblArrow.ForeColor = Color.Lime;
        }
        else
        {
            _lblArrow.Text = "-";
            _lblArrow.ForeColor = Color.Lime;
        }

        LightButton(_btnFloor1, elevator.IsFloorButtonLit(1));
        LightButton(_btnFloor2, elevator.IsFloorButtonLit(2));
        LightButton(_btnFloor3, elevator.IsFloorButtonLit(3));
    }

    /// <summary>
    /// Places the display, round 1/2/3 buttons, door buttons, alarm, emergency and reset.
    /// </summary>
    private void BuildControls()
    {
        _lblTitle = new Label();
        _lblTitle.ForeColor = Color.White;
        _lblTitle.Font = new Font("Segoe UI Semibold", 11f);
        _lblTitle.Location = new Point(12, 8);
        _lblTitle.AutoSize = true;
        _lblTitle.Text = "Car";
        Controls.Add(_lblTitle);

        _lblFloor = new Label();
        _lblFloor.BackColor = Color.Black;
        _lblFloor.ForeColor = Color.Lime;
        _lblFloor.Font = new Font("Consolas", 22f, FontStyle.Bold);
        _lblFloor.TextAlign = ContentAlignment.MiddleCenter;
        _lblFloor.Location = new Point(18, 36);
        _lblFloor.Size = new Size(110, 48);
        _lblFloor.Text = "1";
        Controls.Add(_lblFloor);

        _lblArrow = new Label();
        _lblArrow.BackColor = Color.Black;
        _lblArrow.ForeColor = Color.Lime;
        _lblArrow.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
        _lblArrow.TextAlign = ContentAlignment.MiddleCenter;
        _lblArrow.Location = new Point(132, 36);
        _lblArrow.Size = new Size(72, 48);
        _lblArrow.Text = "-";
        Controls.Add(_lblArrow);

        _btnFloor3 = MakeRoundFloorButton(3, new Point(88, 100));
        _btnFloor2 = MakeRoundFloorButton(2, new Point(88, 162));
        _btnFloor1 = MakeRoundFloorButton(1, new Point(88, 224));

        _btnDoorOpen = MakeWideButton("<|>", new Point(18, 290), Color.FromArgb(70, 90, 110));
        _btnDoorClose = MakeWideButton("|><|", new Point(118, 290), Color.FromArgb(70, 90, 110));
        _btnDoorOpen.Size = new Size(92, 36);
        _btnDoorClose.Size = new Size(92, 36);
        _btnDoorOpen.Name = "btnDoorOpen";
        _btnDoorClose.Name = "btnDoorClose";

        _btnAlarm = MakeWideButton("🔔 Alarm", new Point(18, 334), Color.Gold);
        _btnAlarm.Name = "btnAlarm";
        _btnAlarm.ForeColor = Color.Black;
        _btnAlarm.Font = new Font("Segoe UI Semibold", 10f);
        _btnAlarm.Size = new Size(192, 32);

        _btnEmergency = MakeWideButton("Emergency", new Point(18, 374), Color.FromArgb(192, 57, 43));
        _btnEmergency.Size = new Size(92, 36);
        _btnReset = MakeWideButton("Reset", new Point(118, 374), Color.FromArgb(100, 110, 120));
        _btnReset.Size = new Size(92, 36);

        _btnDoorOpen.Click += (s, e) => Raise(DoorOpenClicked, e);
        _btnDoorClose.Click += (s, e) => Raise(DoorCloseClicked, e);
        _btnAlarm.Click += (s, e) => Raise(AlarmClicked, e);
        _btnEmergency.Click += (s, e) => Raise(EmergencyClicked, e);
        _btnReset.Click += (s, e) => Raise(ResetClicked, e);
    }

    /// <summary>
    /// Round floor button. Amber when that floor has been requested.
    /// </summary>
    private Button MakeRoundFloorButton(int floor, Point location)
    {
        Button button = new Button();
        button.Text = floor.ToString();
        button.Size = new Size(52, 52);
        button.Location = location;
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = _idleGrey;
        button.Font = new Font("Segoe UI Semibold", 12f);
        button.UseVisualStyleBackColor = false;
        using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
        {
            path.AddEllipse(0, 0, 51, 51);
            button.Region = new Region(path);
        }
        button.Click += (s, e) =>
        {
            if (FloorRequested != null)
            {
                FloorRequested(floor);
            }
        };
        Controls.Add(button);
        return button;
    }

    /// <summary>
    /// Rectangular button used for doors, alarm, emergency and reset.
    /// </summary>
    private Button MakeWideButton(string text, Point location, Color back)
    {
        Button button = new Button();
        button.Text = text;
        button.Location = location;
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = back;
        button.ForeColor = Color.White;
        button.UseVisualStyleBackColor = false;
        Controls.Add(button);
        return button;
    }

    /// <summary>
    /// Amber = this floor has been pressed and we have not arrived yet.
    /// </summary>
    private void LightButton(Button button, bool on)
    {
        button.BackColor = on ? _litAmber : _idleGrey;
    }

    /// <summary>
    /// Forwards a simple button click to the form.
    /// </summary>
    private void Raise(EventHandler handler, EventArgs e)
    {
        if (handler != null)
        {
            handler(this, e);
        }
    }
}
