# Elevator Control System

University of Bedfordshire — CIS016-2 — Assignment 1: Control an Elevator.

C# Windows Forms, .NET 8 (`net8.0-windows`), two elevators, three floors, SQL Server LocalDB log.

## What you need

- Windows
- .NET 8 SDK (or a newer SDK that can build `net8.0-windows`)
- SQL Server LocalDB instance `(localdb)\MSSQLLocalDB` (comes with Visual Studio)

If your PC only has a newer runtime (for example .NET 10), the project sets `RollForward` to LatestMajor so it still starts. The **target** stays `net8.0-windows` as required.

## How to run

In PowerShell, open this folder (quote the path because it contains spaces and parentheses):

```powershell
cd "E:\(L-5) (Assignments Of Sem-1(Sep-Dec-Intake_2026)\Desktop Application\ProjectElevator"
dotnet build
sqllocaldb start MSSQLLocalDB
dotnet run
```

Or open `ElevatorSystem.sln` in Visual Studio and press F5.

On first run the app creates:

`[exe folder]\Data\ElevatorLog.mdf`

Debug output path:

`bin\Debug\net8.0-windows\Data\ElevatorLog.mdf`

If LocalDB is off, you get a warning box. The elevators still move; logging is skipped until the database works.

Optional: run `Database\CreateDatabase.sql` in SSMS after the `.mdf` exists, if you want to create the table by hand.

## How to use the window

- **Outside requests** (left): hall calls. The controller picks A or B (shortest estimated wait).
- **Shafts**: car A and car B (pictures if `Images\ElevatorA.png` and `ElevatorB.png` are present).
- **Control panel A / B**: inside buttons. They only move that car. **Emergency** stops that car at the next floor. **Reset** puts it back to idle.
- **Log**: loads the history table into the grid (newest first).

## How to zip for submission

1. Run the program once so `ElevatorLog.mdf` is created (optional but recommended).
2. Close the program so LocalDB releases the file.
3. Copy `bin\Debug\net8.0-windows\Data\ElevatorLog.mdf` and `ElevatorLog_log.ldf` into the project `Data` folder if they are not already there.
4. **Do not** zip `bin` and `obj` if the marker asked for source only — but **do** include the `Data` folder with the `.mdf` **and** the `Images` folder (`ElevatorA.png`, `ElevatorB.png`).
5. Zip the project folder and name it:

`studentID_AssignmentOne.zip`

Example: `1234567_AssignmentOne.zip`

Suggested contents:

- `ElevatorSystem.sln`
- `ElevatorSystem.csproj`
- all `.cs` files
- `Database\CreateDatabase.sql`
- `Data\` (including `.mdf` if you have it)
- `Images\` (`ElevatorA.png` and `ElevatorB.png`)
- `README.md`, `DEMO_NOTES.md`, `TEST_CASES.md`

Exclude if you want a smaller zip: `bin\`, `obj\`.

## Project files (quick map)

| File | Role |
| --- | --- |
| `Form1.cs` | UI and button handlers |
| `Elevator.cs` | One car: queue, timer, events |
| `ElevatorController.cs` | Chooses A or B for hall calls |
| `DatabaseHelper.cs` | All SQL |
| `IElevatorState.cs` + `IdleState` / `MovingUpState` / `MovingDownState` / `DoorOpenState` / `EmergencyState` | State pattern |
| `IElevatorObserver.cs` | Observer pattern |
| `DEMO_NOTES.md` | Spoken walkthrough |
| `TEST_CASES.md` | Marking-style tests |
