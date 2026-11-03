namespace ElevatorSystem;

/// <summary>
/// Double-buffered cross-section of the building: 3 stacked floor rows,
/// two glass shafts (Elevator A and B), landing and car sliding doors,
/// hall lanterns, outside call buttons, and animated passenger walkways.
/// All graphics are drawn with GDI+ (System.Drawing) without external images.
/// </summary>
public class BuildingView : Panel
{
    private Elevator _elevatorA;
    private Elevator _elevatorB;

    private Button _btnHall1Up;
    private Button _btnHall2Up;
    private Button _btnHall2Down;
    private Button _btnHall3Down;

    private readonly Color _hallOff = Color.FromArgb(230, 230, 230);
    private readonly Color _hallOn = Color.FromArgb(255, 191, 0);

    private const int HeaderHeight = 36;
    private const int ShaftALeft = 72;
    private const int ShaftBLeft = 228;
    private const int ShaftWidth = 122;

    // Part 2: Passenger layer (one active passenger per elevator at a time)
    private Passenger _passengerA;
    private Passenger _passengerB;

    /// <summary>
    /// Fired when an outside hall button is pressed (passes requested floor and direction).
    /// </summary>
    public event Action<int, string> HallCall;

    /// <summary>
    /// Fired when a passenger has stepped inside the elevator car.
    /// </summary>
    public event Action<int> PassengerBoarded;

    /// <summary>
    /// Fired when a passenger has stepped out of the elevator and cleared the building.
    /// </summary>
    public event Action<int> PassengerExited;

    /// <summary>
    /// Initializes the BuildingView with optimized double buffering to prevent any flicker.
    /// </summary>
    public BuildingView()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint |
                 ControlStyles.ResizeRedraw, true);
        UpdateStyles();
        BackColor = Color.FromArgb(215, 225, 232);
        BorderStyle = BorderStyle.FixedSingle;
        CreateHallButtons();
    }

    /// <summary>
    /// Binds the two elevator domain objects to this view.
    /// </summary>
    public void Bind(Elevator elevatorA, Elevator elevatorB)
    {
        _elevatorA = elevatorA;
        _elevatorB = elevatorB;
        SyncCarPositions();
        int floorH;
        GetFloorLayout(out _, out floorH);
        PlaceHallButtons(floorH);
        Invalidate();
    }

    /// <summary>
    /// Gives both cars the same floor Y values that OnPaint uses, so doors line up on floors 1, 2 and 3.
    /// </summary>
    private void SyncCarPositions()
    {
        int[] floorY = GetFloorYPositions();
        if (_elevatorA != null)
        {
            _elevatorA.SetFloorYPositions(floorY);
        }

        if (_elevatorB != null)
        {
            _elevatorB.SetFloorYPositions(floorY);
        }
    }

    /// <summary>
    /// Spawns an animated passenger boy at the assigned floor walkway when a hall call is made.
    /// If assigned to Elevator A: spawns at the left edge and walks right.
    /// If assigned to Elevator B: spawns at the right edge and walks left.
    /// </summary>
    public void SpawnPassenger(int floor, int elevatorId)
    {
        int header;
        int floorH;
        GetFloorLayout(out header, out floorH);
        int rowTop = FloorRowTop(header, floorH, floor);
        float baselineY = rowTop + floorH - 6;

        if (elevatorId == AppConstants.ElevatorIdA)
        {
            if (_passengerA == null || _passengerA.State == PassengerState.Gone)
            {
                _passengerA = new Passenger(floor, elevatorId, 8f, baselineY, true);
            }
        }
        else if (elevatorId == AppConstants.ElevatorIdB)
        {
            if (_passengerB == null || _passengerB.State == PassengerState.Gone)
            {
                _passengerB = new Passenger(floor, elevatorId, Width - 28f, baselineY, false);
            }
        }

        Invalidate();
    }

    /// <summary>
    /// Sets the destination floor for a passenger riding inside the specified elevator.
    /// </summary>
    public void SetPassengerDestination(int elevatorId, int destinationFloor)
    {
        if (AppConstants.ENABLE_PASSENGER)
        {
            if (elevatorId == AppConstants.ElevatorIdA && _passengerA != null && _passengerA.State == PassengerState.Riding)
            {
                _passengerA.DestinationFloor = destinationFloor;
            }
            else if (elevatorId == AppConstants.ElevatorIdB && _passengerB != null && _passengerB.State == PassengerState.Riding)
            {
                _passengerB.DestinationFloor = destinationFloor;
            }
        }
    }

    /// <summary>
    /// Advances passenger walking cycles and manages boarding/riding/exiting transitions.
    /// Driven synchronously by the single timer tick.
    /// </summary>
    public void TickPassengers()
    {
        if (AppConstants.ENABLE_PASSENGER)
        {
            int header, floorH;
            GetFloorLayout(out header, out floorH);

            TickOnePassenger(ref _passengerA, _elevatorA, ShaftALeft, header, floorH, true);
            TickOnePassenger(ref _passengerB, _elevatorB, ShaftBLeft, header, floorH, false);
        }
    }

    /// <summary>
    /// Moves one boy: walk to the doors, wait, step inside, ride, then walk out.
    /// Doors stay open until he is inside, then they close on their own.
    /// </summary>
    private void TickOnePassenger(ref Passenger passenger, Elevator elevator, int shaftLeft, int header, int floorH, bool exitsLeft)
    {
        if (passenger == null || passenger.State == PassengerState.Gone || elevator == null)
        {
            return;
        }

        float doorCenterX = shaftLeft + (ShaftWidth - Passenger.PassengerWidth) / 2f;
        float waitX = exitsLeft ? doorCenterX - 24f : doorCenterX + 24f;
        float baselineY = FloorRowTop(header, floorH, passenger.CurrentFloor) + floorH - 6;
        bool doorsReady = elevator.StoppedFloor == passenger.CurrentFloor && elevator.DoorOpenness >= 0.85f;

        if (passenger.State == PassengerState.WalkingToDoors || passenger.State == PassengerState.WaitingForElevator)
        {
            if (elevator.StoppedFloor == passenger.CurrentFloor)
            {
                elevator.IsBoardingOrExiting = true;
            }
        }

        if (passenger.State == PassengerState.WaitingForElevator && doorsReady)
        {
            passenger.State = PassengerState.Boarding;
            elevator.IsBoardingOrExiting = true;
        }
        else if (passenger.State == PassengerState.Boarding && passenger.HasSteppedInside(doorCenterX))
        {
            passenger.State = PassengerState.Riding;
            elevator.PassengerFinishedBoarding();
            if (PassengerBoarded != null)
            {
                PassengerBoarded(elevator.ElevatorId);
            }
        }
        else if (passenger.State == PassengerState.Riding &&
                 passenger.DestinationFloor > 0 &&
                 elevator.StoppedFloor == passenger.DestinationFloor &&
                 elevator.DoorOpenness >= 0.85f)
        {
            passenger.CurrentFloor = passenger.DestinationFloor;
            passenger.State = PassengerState.Exiting;
            elevator.IsBoardingOrExiting = true;
            passenger.IsFacingRight = !exitsLeft;
        }

        passenger.Update(elevator, doorCenterX, waitX, baselineY, Width);

        if (passenger.State == PassengerState.Gone)
        {
            if (PassengerExited != null)
            {
                PassengerExited(elevator.ElevatorId);
            }

            passenger = null;
        }
    }

    /// <summary>
    /// Pixel Y of the top of the car when parked at each floor.
    /// </summary>
    public int[] GetFloorYPositions()
    {
        int header, floorH;
        GetFloorLayout(out header, out floorH);

        int[] positions = new int[AppConstants.TOTAL_FLOORS + 1];
        int carH = AppConstants.CarHeight;

        for (int floor = AppConstants.TOTAL_FLOORS; floor >= 1; floor--)
        {
            int rowTop = FloorRowTop(header, floorH, floor);
            positions[floor] = rowTop + (floorH - carH) / 2;
        }

        return positions;
    }

    /// <summary>
    /// Turns a hall button amber, or resets to grey when a car arrives.
    /// </summary>
    public void SetHallLit(int floor, string direction, bool on)
    {
        Color colour = on ? _hallOn : _hallOff;
        if (floor == 1 && direction == AppConstants.DirectionUp)
        {
            _btnHall1Up.BackColor = colour;
        }
        else if (floor == 2 && direction == AppConstants.DirectionUp)
        {
            _btnHall2Up.BackColor = colour;
        }
        else if (floor == 2 && direction == AppConstants.DirectionDown)
        {
            _btnHall2Down.BackColor = colour;
        }
        else if (floor == 3 && direction == AppConstants.DirectionDown)
        {
            _btnHall3Down.BackColor = colour;
        }
    }

    /// <summary>
    /// Clears both hall lights on a floor after an elevator arrives there.
    /// </summary>
    public void ClearHallLights(int floor)
    {
        if (floor == 1)
        {
            _btnHall1Up.BackColor = _hallOff;
        }
        else if (floor == 2)
        {
            _btnHall2Up.BackColor = _hallOff;
            _btnHall2Down.BackColor = _hallOff;
        }
        else if (floor == 3)
        {
            _btnHall3Down.BackColor = _hallOff;
        }
    }

    /// <summary>
    /// Shared row sizes so painting and the elevator Timer use the same floor Y values.
    /// </summary>
    private void GetFloorLayout(out int header, out int floorHeight)
    {
        header = HeaderHeight;
        int usable = Math.Max(180, Height - header - 8);
        floorHeight = usable / AppConstants.TOTAL_FLOORS;
    }

    /// <summary>
    /// Top pixel of a floor row. Floor 3 is at the top, floor 1 at the bottom.
    /// </summary>
    private static int FloorRowTop(int header, int floorH, int floor)
    {
        int rowFromTop = AppConstants.TOTAL_FLOORS - floor;
        return header + rowFromTop * floorH;
    }

    /// <summary>
    /// Main painting routine: roof, floor rows, shafts, door frames, glass cars,
    /// synchronized landing and car doors, lanterns, and passenger walkway.
    /// </summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(BackColor);

        int header, floorH;
        GetFloorLayout(out header, out floorH);
        int shaftTop = header;
        int shaftHeight = floorH * AppConstants.TOTAL_FLOORS;

        // 1. Draw building roof header with centered "Shaft A" and "Shaft B" labels
        DrawRoof(g, header, Width);

        // 2. Draw floor rows and baseline slabs
        for (int floor = AppConstants.TOTAL_FLOORS; floor >= 1; floor--)
        {
            int rowTop = FloorRowTop(header, floorH, floor);
            DrawFloorRow(g, rowTop, floorH, floor);
        }

        // 3. Draw glass elevator shafts (Shaft A: cool steel blue, Shaft B: warm bronze/gold)
        DrawShaft(g, ShaftALeft, shaftTop, shaftHeight, AppConstants.ElevatorIdA);
        DrawShaft(g, ShaftBLeft, shaftTop, shaftHeight, AppConstants.ElevatorIdB);

        // 4. Draw fixed door frames in the shafts for each floor
        for (int floor = AppConstants.TOTAL_FLOORS; floor >= 1; floor--)
        {
            int rowTop = FloorRowTop(header, floorH, floor);
            DrawDoorFrame(g, ShaftALeft, rowTop, floorH);
            DrawDoorFrame(g, ShaftBLeft, rowTop, floorH);
        }

        // 5. Draw the moving glass elevator cars (cabin first, then car doors on the car)
        if (_elevatorA != null)
        {
            DrawCar(g, _elevatorA, ShaftALeft);
        }

        if (_elevatorB != null)
        {
            DrawCar(g, _elevatorB, ShaftBLeft);
        }

        DrawPassengers(g, true);

        if (_elevatorA != null)
        {
            DrawCarDoors(g, _elevatorA, ShaftALeft);
        }

        if (_elevatorB != null)
        {
            DrawCarDoors(g, _elevatorB, ShaftBLeft);
        }

        // 6. Draw landing doors and hall lanterns for each floor
        for (int floor = AppConstants.TOTAL_FLOORS; floor >= 1; floor--)
        {
            int rowTop = FloorRowTop(header, floorH, floor);

            if (_elevatorA != null)
            {
                DrawLandingDoors(g, _elevatorA, ShaftALeft, rowTop, floorH, floor);
                DrawLantern(g, _elevatorA, ShaftALeft, rowTop, floor);
            }

            if (_elevatorB != null)
            {
                DrawLandingDoors(g, _elevatorB, ShaftBLeft, rowTop, floorH, floor);
                DrawLantern(g, _elevatorB, ShaftBLeft, rowTop, floor);
            }
        }

        DrawPassengers(g, false);
    }

    /// <summary>
    /// Keeps hall buttons lined up with the floor rows on control resize.
    /// </summary>
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        SyncCarPositions();
        int floorH;
        GetFloorLayout(out _, out floorH);
        PlaceHallButtons(floorH);
        Invalidate();
    }

    /// <summary>
    /// Draws passengers. ridingOnly = true draws boys standing inside a car.
    /// </summary>
    private void DrawPassengers(Graphics g, bool ridingOnly)
    {
        DrawOnePassenger(g, _passengerA, ridingOnly);
        DrawOnePassenger(g, _passengerB, ridingOnly);
    }

    /// <summary>
    /// Draws one boy if his state matches the riding / hall pass.
    /// </summary>
    private static void DrawOnePassenger(Graphics g, Passenger passenger, bool ridingOnly)
    {
        if (passenger == null || passenger.State == PassengerState.Gone)
        {
            return;
        }

        bool isInside = passenger.State == PassengerState.Riding || passenger.State == PassengerState.Boarding;
        if (isInside == ridingOnly)
        {
            passenger.Draw(g);
        }
    }

    /// <summary>
    /// Title strip at the top of the building with cleanly centered "Shaft A" and "Shaft B" labels.
    /// </summary>
    private static void DrawRoof(Graphics g, int header, int width)
    {
        using (SolidBrush roofBrush = new SolidBrush(Color.FromArgb(42, 52, 65)))
        using (SolidBrush titleBrush = new SolidBrush(Color.White))
        using (Font font = new Font("Segoe UI Semibold", 9.5f))
        using (Font shaftFont = new Font("Segoe UI", 10f, FontStyle.Bold))
        {
            g.FillRectangle(roofBrush, 0, 0, width, header);
            g.DrawString("Building View", font, titleBrush, 10, 8);

            // Centered above Shaft A (Cool Sky Blue theme)
            string labelA = "Shaft A";
            SizeF sizeA = g.MeasureString(labelA, shaftFont);
            float xA = ShaftALeft + (ShaftWidth - sizeA.Width) / 2f;
            using (SolidBrush brushA = new SolidBrush(Color.FromArgb(115, 215, 255)))
            {
                g.DrawString(labelA, shaftFont, brushA, xA, 8);
            }

            // Centered above Shaft B (Warm Amber / Gold theme)
            string labelB = "Shaft B";
            SizeF sizeB = g.MeasureString(labelB, shaftFont);
            float xB = ShaftBLeft + (ShaftWidth - sizeB.Width) / 2f;
            using (SolidBrush brushB = new SolidBrush(Color.FromArgb(255, 215, 95)))
            {
                g.DrawString(labelB, shaftFont, brushB, xB, 8);
            }
        }
    }

    /// <summary>
    /// One floor row plus slab and large floor indicator label.
    /// </summary>
    private void DrawFloorRow(Graphics g, int rowTop, int floorH, int floor)
    {
        Rectangle row = new Rectangle(0, rowTop, Width - 1, floorH);
        Color fill = floor % 2 == 1
            ? Color.FromArgb(235, 227, 213)
            : Color.FromArgb(223, 214, 198);

        using (SolidBrush floorBrush = new SolidBrush(fill))
        using (SolidBrush slab = new SolidBrush(Color.FromArgb(160, 150, 135)))
        using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(50, 50, 50)))
        using (Font floorFont = new Font("Segoe UI Semibold", 12f))
        {
            g.FillRectangle(floorBrush, row);
            g.FillRectangle(slab, 0, rowTop + floorH - 6, Width, 6);
            g.DrawString("F" + floor, floorFont, textBrush, 10, rowTop + floorH / 2 - 10);
        }
    }

    /// <summary>
    /// The glass elevator shaft with guide rails. Distinguishes Shaft A (cool) from Shaft B (warm).
    /// </summary>
    private static void DrawShaft(Graphics g, int shaftLeft, int shaftTop, int shaftHeight, int elevatorId)
    {
        Rectangle shaft = new Rectangle(shaftLeft, shaftTop, ShaftWidth, shaftHeight);

        Color glassColor = elevatorId == AppConstants.ElevatorIdA
            ? Color.FromArgb(95, 185, 215, 240)   // Cool Sky Blue
            : Color.FromArgb(95, 245, 225, 180);  // Warm Amber-Gold

        Color frameColor = elevatorId == AppConstants.ElevatorIdA
            ? Color.FromArgb(70, 120, 170)
            : Color.FromArgb(165, 125, 55);

        Color railColor = elevatorId == AppConstants.ElevatorIdA
            ? Color.FromArgb(90, 125, 155)
            : Color.FromArgb(150, 125, 80);

        using (SolidBrush glass = new SolidBrush(glassColor))
        using (Pen frame = new Pen(frameColor, 3))
        using (Pen rail = new Pen(railColor, 2))
        {
            g.FillRectangle(glass, shaft);
            g.DrawRectangle(frame, shaft);
            g.DrawLine(rail, shaftLeft + 8, shaftTop + 4, shaftLeft + 8, shaftTop + shaftHeight - 4);
            g.DrawLine(rail, shaftLeft + ShaftWidth - 8, shaftTop + 4, shaftLeft + ShaftWidth - 8, shaftTop + shaftHeight - 4);
        }
    }

    /// <summary>
    /// Fixed door frame in the shaft at this floor row.
    /// </summary>
    private static void DrawDoorFrame(Graphics g, int shaftLeft, int rowTop, int floorH)
    {
        Rectangle frame = DoorRectangle(shaftLeft, rowTop, floorH);
        using (Pen doorFrame = new Pen(Color.FromArgb(60, 70, 82), 3))
        using (SolidBrush recess = new SolidBrush(Color.FromArgb(40, 20, 30, 40)))
        {
            g.FillRectangle(recess, frame);
            g.DrawRectangle(doorFrame, frame);
        }
    }

    /// <summary>
    /// Digital hall lantern above each floor's door pair showing floor number and direction arrow.
    /// </summary>
    private static void DrawLantern(Graphics g, Elevator elevator, int shaftLeft, int rowTop, int floor)
    {
        Rectangle box = new Rectangle(shaftLeft + ShaftWidth / 2 - 26, rowTop + 6, 52, 22);
        using (SolidBrush black = new SolidBrush(Color.Black))
        using (SolidBrush lime = new SolidBrush(Color.Lime))
        using (Font font = new Font("Consolas", 9f, FontStyle.Bold))
        {
            g.FillRectangle(black, box);

            string arrow = "-";
            if (elevator.Direction == AppConstants.DirectionUp)
            {
                arrow = "▲";
            }
            else if (elevator.Direction == AppConstants.DirectionDown)
            {
                arrow = "▼";
            }

            if (elevator.IsInEmergency)
            {
                arrow = AppConstants.DISPLAY_EMERGENCY;
            }

            if (elevator.CurrentFloor == floor)
            {
                using (Pen here = new Pen(Color.Lime, 1))
                {
                    g.DrawRectangle(here, box);
                }
            }

            string text = elevator.CurrentFloor + " " + arrow;
            g.DrawString(text, font, lime, box.X + 6, box.Y + 3);
        }
    }

    /// <summary>
    /// Landing doors on this floor row. They only slide open on the floor where the car has stopped.
    /// </summary>
    private static void DrawLandingDoors(Graphics g, Elevator elevator, int shaftLeft, int rowTop, int floorH, int floor)
    {
        Rectangle frame = DoorRectangle(shaftLeft, rowTop, floorH);

        // Line the landing doors up with the car so the opening matches on every floor.
        if (!elevator.IsMoving && elevator.CurrentFloor == floor)
        {
            frame.Y = elevator.CarY + 4;
        }

        float openness = 0f;
        if (!elevator.IsMoving && elevator.CurrentFloor == floor)
        {
            openness = elevator.DoorOpenness;
        }

        DrawDoorLeaves(g, frame, openness, Color.FromArgb(220, 168, 176, 188));
    }

    /// <summary>
    /// Sliding doors drawn on the car itself, so open/close is visible at floor 1, 2 and 3.
    /// </summary>
    private static void DrawCarDoors(Graphics g, Elevator elevator, int shaftLeft)
    {
        int doorW = 60;
        int doorH = AppConstants.CarHeight - 8;
        int doorX = shaftLeft + (ShaftWidth - doorW) / 2;
        int doorY = elevator.CarY + 4;
        Rectangle frame = new Rectangle(doorX, doorY, doorW, doorH);
        float openness = elevator.IsMoving ? 0f : elevator.DoorOpenness;
        DrawDoorLeaves(g, frame, openness, Color.FromArgb(230, 190, 200, 210));
    }

    /// <summary>
    /// Two door panels that slide apart. openness 0 = shut, 1 = fully open.
    /// </summary>
    private static void DrawDoorLeaves(Graphics g, Rectangle frame, float openness, Color fill)
    {
        int leafW = frame.Width / 2;
        int slide = (int)(leafW * openness);
        Rectangle leftLeaf = new Rectangle(frame.X - slide, frame.Y, leafW, frame.Height);
        Rectangle rightLeaf = new Rectangle(frame.X + leafW + slide, frame.Y, leafW, frame.Height);

        using (SolidBrush leaf = new SolidBrush(fill))
        using (Pen edge = new Pen(Color.FromArgb(70, 80, 92), 2))
        using (Pen slit = new Pen(Color.FromArgb(120, 130, 142), 1))
        {
            g.FillRectangle(leaf, leftLeaf);
            g.FillRectangle(leaf, rightLeaf);
            g.DrawRectangle(edge, leftLeaf);
            g.DrawRectangle(edge, rightLeaf);
            g.DrawLine(edge, leftLeaf.Right - 1, leftLeaf.Top, leftLeaf.Right - 1, leftLeaf.Bottom);
            g.DrawLine(edge, rightLeaf.Left, rightLeaf.Top, rightLeaf.Left, rightLeaf.Bottom);
            g.DrawLine(slit, leftLeaf.X + leafW / 2, leftLeaf.Top + 6, leftLeaf.X + leafW / 2, leftLeaf.Bottom - 6);
            g.DrawLine(slit, rightLeaf.X + leafW / 2, rightLeaf.Top + 6, rightLeaf.X + leafW / 2, rightLeaf.Bottom - 6);
        }
    }

    /// <summary>
    /// Glass elevator car sliding along the shaft at elevator.CarY.
    /// Shows car name (A or B) and distinctive interior color.
    /// </summary>
    private static void DrawCar(Graphics g, Elevator elevator, int shaftLeft)
    {
        int x = shaftLeft + (ShaftWidth - AppConstants.CarWidth) / 2;
        int y = elevator.CarY;
        Rectangle car = new Rectangle(x, y, AppConstants.CarWidth, AppConstants.CarHeight);

        bool isA = elevator.ElevatorId == AppConstants.ElevatorIdA;
        Color glassColor = isA
            ? Color.FromArgb(150, 140, 200, 240)   // Cool Blue
            : Color.FromArgb(150, 245, 220, 165);  // Warm Gold

        Color rimColor = isA
            ? Color.FromArgb(30, 80, 140)
            : Color.FromArgb(150, 105, 30);

        Color textColor = isA
            ? Color.FromArgb(15, 45, 80)
            : Color.FromArgb(85, 55, 15);

        using (SolidBrush fill = new SolidBrush(glassColor))
        using (Pen rim = new Pen(rimColor, 2))
        using (Font font = new Font("Segoe UI Semibold", 12f))
        using (SolidBrush text = new SolidBrush(textColor))
        using (SolidBrush inner = new SolidBrush(Color.FromArgb(70, 255, 255, 255)))
        {
            g.FillRectangle(fill, car);
            Rectangle window = new Rectangle(car.X + 8, car.Y + 10, car.Width - 16, car.Height - 22);
            g.FillRectangle(inner, window);
            g.DrawRectangle(rim, car);
            g.DrawString(elevator.Name, font, text, x + 26, y + 24);
        }
    }

    /// <summary>
    /// The door opening rectangle inside a shaft, centred on this floor row.
    /// </summary>
    private static Rectangle DoorRectangle(int shaftLeft, int rowTop, int floorH)
    {
        int doorW = 60;
        int doorH = AppConstants.CarHeight - 8;
        int doorX = shaftLeft + (ShaftWidth - doorW) / 2;
        int doorY = rowTop + (floorH - doorH) / 2;
        return new Rectangle(doorX, doorY, doorW, doorH);
    }

    /// <summary>
    /// Builds the four hall buttons: Floor 1 Up, Floor 2 Up, Floor 2 Down, Floor 3 Down.
    /// </summary>
    private void CreateHallButtons()
    {
        _btnHall3Down = MakeHallButton("▼", "btnHall3Down");
        _btnHall2Up = MakeHallButton("▲", "btnHall2Up");
        _btnHall2Down = MakeHallButton("▼", "btnHall2Down");
        _btnHall1Up = MakeHallButton("▲", "btnHall1Up");

        _btnHall1Up.Click += (s, e) => RaiseHall(1, AppConstants.DirectionUp);
        _btnHall2Up.Click += (s, e) => RaiseHall(2, AppConstants.DirectionUp);
        _btnHall2Down.Click += (s, e) => RaiseHall(2, AppConstants.DirectionDown);
        _btnHall3Down.Click += (s, e) => RaiseHall(3, AppConstants.DirectionDown);

        Controls.Add(_btnHall3Down);
        Controls.Add(_btnHall2Up);
        Controls.Add(_btnHall2Down);
        Controls.Add(_btnHall1Up);
    }

    /// <summary>
    /// Places hall buttons beside the doors on each floor row.
    /// </summary>
    private void PlaceHallButtons(int floorH)
    {
        int x = ShaftBLeft + ShaftWidth + 16;
        int f3 = HeaderHeight;
        int f2 = HeaderHeight + floorH;
        int f1 = HeaderHeight + floorH * 2;

        _btnHall3Down.Location = new Point(x, f3 + floorH / 2 - 14);
        _btnHall2Up.Location = new Point(x, f2 + floorH / 2 - 34);
        _btnHall2Down.Location = new Point(x, f2 + floorH / 2 + 6);
        _btnHall1Up.Location = new Point(x, f1 + floorH / 2 - 14);
    }

    /// <summary>
    /// Builds one small hall call button.
    /// </summary>
    private Button MakeHallButton(string caption, string name)
    {
        Button button = new Button();
        button.Name = name;
        button.Text = caption;
        button.Size = new Size(36, 28);
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = _hallOff;
        button.UseVisualStyleBackColor = false;
        return button;
    }

    /// <summary>
    /// Lights the pressed hall button amber and fires the HallCall event.
    /// </summary>
    private void RaiseHall(int floor, string direction)
    {
        SetHallLit(floor, direction, true);
        if (HallCall != null)
        {
            HallCall(floor, direction);
        }
    }
}
