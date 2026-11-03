# Demo notes — Elevator Control System

Use this as a speaking script. The code is written so you can open each file and talk through it line by line during a live demo.

## What the program is

A 3-floor building with **two elevators** (A and B).  
Outside hall buttons call the closest/best elevator using shortest response time.  
Inside car panels belong to *one* car only.  
Every request, move, door cycle, alarm, arrival, and passenger boarding/exit is saved in SQL Server LocalDB.  
All graphics are drawn with **GDI+ (System.Drawing)** with double buffering — no external image files required.

---

## Part 1: Door Animation Root Cause & Fix

### Root Cause
1. In `BuildingView.cs`, the door drawing code evaluated `elevator.CurrentFloor` directly (which changes dynamically during transit when passing intermediate floors) rather than relying on a verified stationary `StoppedFloor`.
2. Intermediate queued stops were bypassed during transit without stopping to open doors, and the roof header labels were misaligned.

### Fix
- Introduced `elevator.StoppedFloor` (returns `0` while moving, and `1..3` when stationary).
- Landing doors and car doors open and close together strictly when `floor == elevator.StoppedFloor`, driven by the elevator's `DoorOpenness` (0.0 to 1.0).
- At every other floor (`floor != StoppedFloor`), that elevator's landing doors stay fully closed.
- While the car is moving between floors (`StoppedFloor == 0`), all landing doors and car doors stay fully closed.
- Intermediate queued stops (e.g., reaching floor 2 while travelling 1 $\rightarrow$ 3) are detected during transit, causing the car to stop, open doors, dwell, close, and then continue on to the final target.
- "Shaft A" and "Shaft B" header labels are dynamically measured and centered directly above their respective shafts with clear color differentiation (Cool Cyan-Blue for Shaft A, Warm Amber-Gold for Shaft B).

---

## Part 2: Passenger Animation Layer

Controlled by the constant `AppConstants.ENABLE_PASSENGER = true;`. Setting it to `false` disables all passenger logic without affecting the elevators.

### `Passenger.cs`
An animated boy drawn purely in GDI+ code (round head, hair/cap, peach skin tone, colored shirt, swinging denim legs with shoes, swinging arms):
- Scaled to ~65% of door height (44 pixels tall) to fit the floor row walkway.
- States (`PassengerState`):
  1. `WalkingToDoors`: Spawns at the floor row outer edge (Left edge for Elevator A, Right edge for Elevator B) when a hall call is assigned, walking to the doors with animated legs.
  2. `WaitingForElevator`: Stands in front of the assigned doors while the elevator travels to him.
  3. `Boarding`: When doors fully open, walks into the car cabin. The doors **never close while he is in the doorway** (`elevator.IsBoardingOrExiting = true`).
  4. `Riding`: Positioned inside the car, moving vertically in sync with `elevator.CarY`. Status displays `"Passenger inside - choose a floor"`.
  5. `Exiting`: When destination floor is reached and doors are open, walks out of the car across the walkway. Doors stay open until he clears the doorway.
  6. `Gone`: Clears the building edge and is removed.
- Events: `PassengerBoarded(int elevatorId)` and `PassengerExited(int elevatorId)`, logged to the database as `"PassengerBoarded"` and `"PassengerExited"` via `DatabaseHelper` and `BackgroundWorker`.
- Single Timer: All walking, leg swing, and door coordination runs on the existing 30 ms timer tick — no extra timers or background threads.

---

## Classes Overview

### `Program.cs`
Standard Windows Forms entry point. `Main` turns on visual styles and runs `Form1`.

### `AppConstants.cs`
Holds `TOTAL_FLOORS = 3`, `ENABLE_PASSENGER = true`, `SPAWN_PASSENGER_ON_PANEL_PRESS = false`, and all application-wide constants:
- Car IDs (`ElevatorIdA = 1`, `ElevatorIdB = 2`)
- Animation settings (`TimerIntervalMs = 30`, `PixelStep = 5`)
- Door timing ticks: opening ~1 s (`DoorOpenTicks = 33`), dwell ~3 s (`DoorDwellTicks = 100`), closing ~1 s (`DoorCloseTicks = 33`)
- Status strings (`Request`, `Moving`, `Arrived`, `DoorsOpened`, `DoorsClosed`, `Alarm`, `Emergency`, `Reset`, `PassengerBoarded`, `PassengerExited`)
- Source names and door states (`Opening`, `Open`, `Closing`, `Closed`).

### `Form1` (UI Wiring Only)
Acts as the central coordinator and implements `IElevatorObserver`:
- Hosts `BuildingView` and two `ElevatorPanel` user controls (Car A and Car B).
- Hosts movement/status labels (`lblStatusA`, `lblStatusB`).
- Provides **Log**, **Refresh**, and **Clear Log** buttons with a `DataGridView` (`dgvLog`).
- Uses three `BackgroundWorker` instances (`_saveWorker`, `_loadWorker`, `_clearWorker`) so database operations never freeze UI animation.
- Wires passenger events to logging and hint labels.

### `BuildingView.cs`
Custom double-buffered control (`DoubleBuffered = true`):
- Cross-section view of 3 stacked floor rows (Floor 3 on top, Floor 1 at the bottom).
- Centered header labels "Shaft A" and "Shaft B" with theme colors.
- Two distinct glass elevator shafts:
  - Shaft A: Cool steel blue theme (`Color.SteelBlue`).
  - Shaft B: Warm bronze/amber theme (`Color.DarkGoldenrod`).
- Fixed landing door frames on each floor.
- Animated glass cars sliding vertically at `elevator.CarY`.
- Synchronized sliding doors: landing doors at `StoppedFloor` open and close with `DoorOpenness`. All other floors remain strictly shut.
- Hall lanterns above each door pair showing floor number and direction arrow (`▲`, `▼`, `-`, `E`).
- Four outside hall call buttons beside doors.
- Passenger animation layer drawn on top of the floor walkway baseline in front of shafts.

### `ElevatorPanel.cs`
Custom `UserControl` mimicking a real car operating panel:
- Floor buttons 1, 2, 3 styled as round buttons using GDI+ circular clipping (`Region`). Glow amber when pressed, turn off on arrival.
- Door Open button (`<|>`) and Door Close button (`|><|`).
- Yellow alarm bell button (`🔔 Alarm`).
- Emergency and Reset buttons.
- Digital display window with black background and lime text showing floor number and direction arrow.

### `Elevator.cs`
Core elevator domain model:
- Properties: `CurrentFloor`, `TargetFloor`, `Direction`, `CarY`, `DoorOpenness`, `StoppedFloor`, `IsBoardingOrExiting`.
- Internal queue: `Queue<int> _stopQueue` for additional stops.
- State: `IElevatorState _state` (State pattern).
- Events: `FloorChanged` and `DoorStateChanged` delegate events.
- **Single Timer**: 30 ms timer driving both car movement and door animation.
- Safety rule: elevator NEVER starts moving while `_doorOpenness > 0.01f`, and doors never close while `IsBoardingOrExiting` is true.

### `ElevatorController.cs`
Outside hall call dispatcher:
- Compares `EstimateResponseTime(floor)` between Elevator A and Elevator B.
- Dispatches car with shortest estimated wait time. Running trips are never cancelled.

### `IElevatorState.cs` + Door & Motion State Classes (State Pattern)
- `IdleState`: Start trip or open doors if already on that floor.
- `MovingUpState` / `MovingDownState`: Queue requests; car continues trip with doors closed.
- `DoorsOpeningState`: Opening doors at stopped floor.
- `DoorsOpenState`: Holding doors open for 3 s dwell (held indefinitely while passenger boards/exits).
- `DoorsClosingState`: Closing doors; same-floor call reverses to reopen.
- `EmergencyState`: Ignores floor requests until Reset.

### `DatabaseHelper.cs`
Centralizes connection management and SQL operations:
- Uses relative path `AppContext.BaseDirectory\Data\ElevatorLog.mdf`.
- Self-initializing: creates database file and `ElevatorLog` table on LocalDB if missing.
- Disconnected insert: `SqlDataAdapter.Update(DataTable)`.
- `LoadLog()` reads all records newest first into a `DataTable`.
- `ClearLog()` executes `DELETE FROM ElevatorLog` inside try-catch.

---

## Live Flow to Demonstrate During Demo

1. **Door Sequences on Every Floor (Part 1 Verification)**:
   - Run sequence **1 $\rightarrow$ 2 $\rightarrow$ 3 $\rightarrow$ 1** on Car A.
   - At each arrival floor, doors visibly slide open (~1 s), dwell (~3 s), and close (~1 s).
   - On all other floors, landing doors stay completely closed.
   - Run sequence **1 $\rightarrow$ 3 $\rightarrow$ 2 $\rightarrow$ 1** on Car B. Same verified behavior.
   - Point out the centered header labels ("Shaft A" and "Shaft B") and distinctive shaft/car colors.

2. **Passenger Flow (Part 2 Verification)**:
   - Click **▲** on Floor 1. Controller assigns Car A.
   - A boy appears at the left edge of Floor 1 and walks toward Shaft A with swinging legs.
   - Car A doors open; boy walks into the car cabin.
   - Status updates: `"Elevator A: Passenger inside - choose a floor"`.
   - Point out that the doors wait and do not close while he is boarding!
   - Press **3** on Panel A.
   - Car A travels from Floor 1 to Floor 3 with the boy visibly riding inside the car.
   - At Floor 3, doors open; boy walks out across the walkway off the left edge.
   - Once he clears the doorway, doors finish their dwell and close.

3. **Two Passengers on Different Elevators**:
   - Press Floor 1 Up (assigns A) and Floor 3 Down (assigns B).
   - Two boys spawn simultaneously (one on left of F1, one on right of F3) and walk to their respective cars independently.

4. **Database History**:
   - Click **Refresh**: show recorded rows for `"PassengerBoarded"` and `"PassengerExited"` alongside `"DoorsOpened"`, `"DoorsClosed"`, `"Moving"`, `"Arrived"`.
   - Click **Clear Log**, confirm: table is wiped cleanly and grid resets to 0 rows.
