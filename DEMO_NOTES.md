# Demo notes — Elevator Control System

Use this as a speaking script. The code is written so you can open each file and talk through it line by line.

## What the program is

A 3-floor building with **two elevators** (A and B).  
Outside buttons call *either* car. Inside buttons belong to *one* car only.  
Every request, move and arrival is saved in SQL Server LocalDB.

---

## Classes (what to say for each file)

### `Program.cs`
Normal WinForms start-up. `Main` turns on visual styles and opens `Form1`.

### `AppConstants.cs`
Holds `TOTAL_FLOORS = 3` and other shared values (elevator ids, timer speed, status words).  
**Talking point:** we never hard-code “floor 1 / 2 / 3” logic three times. Loops use `TOTAL_FLOORS`.

### `Form1` (UI only)
Draws:

- three **outside** request buttons (`btnRequestFloor1` … `3`)
- two **shafts** with moving cars (`pnlCarA`, `pnlCarB`)
- three **floor status** labels per elevator (yellow = car is there)
- two **control panels** (buttons 1, 2, 3 + Emergency + Reset + floor number + arrow)
- two **car pictures** (`picCarA`, `picCarB`) inside the moving panels
- a movement sentence per elevator
- one **Log** button and a `DataGridView`

Every button has **its own click method**. Each method only calls shared logic:

- hall buttons → `_controller.HandleHallRequest(floor)`
- panel A buttons → `_elevatorA.MoveElevatorTo(floor, "Panel Button F…")`
- panel B buttons → `_elevatorB.MoveElevatorTo(...)`
- Emergency / Reset → `_elevatorA.TriggerEmergency` / `ResetEmergency` (same for B)

The form implements `IElevatorObserver`, so it updates labels when an elevator talks to it. It does **not** contain SQL.

### `Elevator.cs`
One car. Important members:

- `CurrentFloor`, `TargetFloor`, `Direction`
- `Queue<int>` for extra stops while moving
- `IElevatorState _state` (State pattern)
- list of `IElevatorObserver` (Observer pattern)
- `public event FloorChangedHandler FloorChanged` (delegate + event)
- a `Timer` that moves the panel a few pixels each tick

**Shared method:** `MoveElevatorTo(int targetFloor, string source)`  
All six (actually nine) buttons end up here.

If the target is the current floor and we are idle: status says “already here”, no move.  
If we are already moving: the floor goes on the queue. Repeated clicks are ignored if that floor is already the target or already queued, so the app never crashes.

### `ElevatorController.cs`
Used **only** for outside requests.

`ChooseElevator` asks each car for `EstimateResponseTime(floor)`:

- remaining floors of the trip in progress
- plus each queued stop
- plus the extra hop to the new floor

The smaller number wins. A trip is **never cancelled**. If the scores are equal, A is used.

### `IElevatorState.cs` + state classes
| State | What a new request does |
| --- | --- |
| `IdleState` | start a trip, or say already here |
| `MovingUpState` | queue the floor |
| `MovingDownState` | queue the floor |
| `DoorOpenState` | start the next trip, or say already here |
| `EmergencyState` | ignore every floor request until Reset |

The elevator **delegates** `HandleRequest` to whichever state object it currently holds. That is the State pattern in one sentence.

### `IElevatorObserver.cs`
The elevator is the **subject**. Observers are notified of:

1. floor changes (update lights and arrows)
2. status text (the “Moving: Floor 1 -> Floor 3 (Up)” label)
3. log rows (save to the database)

`Form1` is the observer. The elevator does not know about labels or SQL.

### `DatabaseHelper.cs`
Owns the connection string and **all** SQL.

Relative paths:

- folder = `Path.Combine(AppContext.BaseDirectory, "Data")`
- file = `Data\ElevatorLog.mdf`

On start:

1. create the Data folder if missing
2. if the `.mdf` is missing, `CREATE DATABASE` on `(localdb)\MSSQLLocalDB`
3. create table `ElevatorLog` if missing

**Disconnected insert:** `SqlDataAdapter` + empty `DataTable` + `SqlCommandBuilder` + `adapter.Update(table)`.  
`ExecuteNonQuery` is only used for CREATE DATABASE / CREATE TABLE.

`LoadLog()` fills a `DataTable` newest first for the grid.

Every call is in try/catch with a MessageBox. The lifts still work if the database fails.

### `LogEntry.cs`
Tiny packet of fields passed into the BackgroundWorker.

---

## Live flow (button → logic → animation → database)

Speak this while you click.

1. **You click** `Floor 3` (outside) or panel button `3`.
2. The click handler calls either the **controller** (hall) or **that elevator** (panel).
3. `MoveElevatorTo` writes a **Request** log (via the observer) then asks the **current state** what to do.
4. If idle, `StartTrip` sets direction Up or Down, switches to `MovingUpState` / `MovingDownState`, logs **Moving**, and starts the timer.
5. Each **timer tick** moves `pnlCarA` or `pnlCarB` toward `_floorYPositions[target]`.
6. When the car lines up with a floor, `FloorChanged` fires **and** observers are told. Displays change `1 → 2 → 3` and the arrow shows ▲ or ▼.
7. The status label is always of the form  
   `Elevator A: Moving: Floor 1 -> Floor 3 (Up)`  
   covering every pair: 1→2, 1→3, 2→1, 2→3, 3→1, 3→2.
8. On arrival: log **Arrived**, switch to `DoorOpenState`, then idle (or the next queued floor).
9. **Log** button starts a `BackgroundWorker` that calls `LoadLog()` and binds the grid. Saves also run on a BackgroundWorker so the car does not freeze.

---

## How to show State and Observer in the demo

**State:** put a breakpoint in `IdleState.HandleRequest` and in `MovingUpState.HandleRequest`. Click a floor while idle (starts moving). Click another floor while moving (goes to the queue). Same method name, different class.

**Observer:** put a breakpoint in `Form1.OnFloorChanged`. Move a car. The elevator calls the observer; the form only paints.

**Delegate/event:** `FloorChanged` in `Elevator.cs` is also subscribed in `Form1_Load`. Same floor number updates the black display window.

**Two cars:** send A to floor 3 with panel A. While it is travelling, press outside **Floor 1**. The controller should pick **B** (idle at 1, cost 0) or the closer idle car — then send B if needed. Send both cars away, then press a hall floor and explain the cost numbers.

---

## Database table (for the marker)

```
Id, ElevatorId, FromFloor, ToFloor, Status, Source, LogTime
Status = Request | Moving | Arrived | Emergency | Reset
Source = "Request Button F2" or "Panel Button F3" or "Emergency Button A" or "Reset Button A"
```

Manual script: `Database\CreateDatabase.sql`.

---

## Car images (relative path)

Pictures are **not** hard-coded as `C:\...`.

1. Project folder has an `Images` folder (same level as the `.csproj`).
2. Put `ElevatorA.png` and `ElevatorB.png` in that folder.
3. The `.csproj` copies `Images\**` to the output folder (`CopyToOutputDirectory` = PreserveNewest), same idea as the `Data` folder.
4. `Form1_Load` calls `LoadCarImages()`.
5. The path is built as `Path.Combine(AppContext.BaseDirectory, IMAGES_FOLDER, fileName)`.
6. The file is loaded with `new Bitmap(path)` so Windows does not lock the png.
7. `SizeMode` is Zoom. The old "A" / "B" letter is hidden when the picture loads.
8. If a file is missing or cannot be read, **one** MessageBox is shown and the coloured square with the letter is kept. The app does not crash.
9. The bitmaps are disposed in `FormClosing`.

To prove the path is relative: copy the whole output folder (`bin\Debug\net8.0-windows`) somewhere else, keep `Images` next to the exe, and run `ElevatorSystem.exe`.

---

## Emergency stop (next floor only)

Each car has its own **Emergency** and **Reset** button. The click handlers only call methods on `Elevator`. No SQL in the form.

### If the car is idle
`TriggerEmergency` switches straight to `EmergencyState`. Doors are treated as open. Status (in red):

`Elevator A: EMERGENCY - stopped at Floor X`

### If the car is moving
The car must **not** stop between floors.

1. Set `_emergencyPending = true`.
2. Clear `Queue<int>` so extra stops are forgotten.
3. Change `TargetFloor` to the **next floor in the current direction** (example: 1 → 3, pressed between 1 and 2 → stop at 2). If it is already going to the last floor of that trip, it stops there.
4. Status: `EMERGENCY - stopping at next floor`.
5. When the timer sees the car Y match that floor, it stops the timer and enters `EmergencyState`.

### While in `EmergencyState`
- Panel buttons and hall calls for that car are ignored (`HandleRequest` does nothing; `MoveElevatorTo` returns early).
- Displays show **E** instead of the arrow.
- The other car keeps working.

### Controller
`HandleHallRequest` skips any car where `IsInEmergency` is true (pending or stopped).  
If **both** cars are in emergency: MessageBox `"No elevator available"` and nothing is queued.

### Reset
`ResetEmergency` only works when the state really is `EmergencyState` (not while still moving to the next floor). It goes back to `IdleState` at the current floor. Normal colours and `Idle at Floor X` come back through the observers.

### Flow you can speak: button → elevator → state → observers → database

1. Click **Emergency** on panel A.
2. `btnEmergencyA_Click` calls `_elevatorA.TriggerEmergency("Emergency Button A")`.
3. The elevator logs **Emergency** (observer → BackgroundWorker → `DatabaseHelper.SaveLog` with DataTable, not `ExecuteNonQuery`).
4. State becomes pending or `EmergencyState`.
5. Observers update the red status line, the **E** on the displays, and the floor lights.
6. When the car stops, another **Emergency** row is saved.
7. Click **Reset** → `IdleState`, log **Reset**, Source `"Reset Button A"`.
