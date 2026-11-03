namespace ElevatorSystem;

/// <summary>
/// Shared numbers and text used by the whole program.
/// Using constants means we do not scatter magic numbers in the code.
/// </summary>
public static class AppConstants
{
    // Part 2: Passenger animation toggle. Set to false to disable all passenger behavior.
    public const bool ENABLE_PASSENGER = true;

    // Optional Part 2: Spawn passenger on idle panel button press.
    public const bool SPAWN_PASSENGER_ON_PANEL_PRESS = false;

    // Official building size for this assignment.
    public const int TOTAL_FLOORS = 3;

    // Elevator identity values stored in the log table.
    public const int ElevatorIdA = 1;
    public const int ElevatorIdB = 2;

    public const string ElevatorNameA = "A";
    public const string ElevatorNameB = "B";

    // Animation: how often the timer ticks, and how many pixels the car moves.
    public const int TimerIntervalMs = 30;
    public const int PixelStep = 5;

    // Drawn car size used by BuildingView and the elevator Y positions.
    public const int CarHeight = 72;
    public const int CarWidth = 70;

    // Door timings at 30 ms per tick: open ~1 s, stay open ~3 s, close ~1 s.
    public const int DoorOpenTicks = 33;
    public const int DoorDwellTicks = 100;
    public const int DoorCloseTicks = 33;

    // Direction text used on labels and in events.
    public const string DirectionUp = "Up";
    public const string DirectionDown = "Down";
    public const string DirectionIdle = "Idle";

    // Log status values required by the assignment.
    public const string StatusRequest = "Request";
    public const string StatusMoving = "Moving";
    public const string StatusArrived = "Arrived";
    public const string StatusDoorsOpened = "DoorsOpened";
    public const string StatusDoorsClosed = "DoorsClosed";
    public const string StatusAlarm = "Alarm";
    public const string STATUS_EMERGENCY = "Emergency";
    public const string STATUS_RESET = "Reset";
    public const string StatusPassengerBoarded = "PassengerBoarded";
    public const string StatusPassengerExited = "PassengerExited";

    // Button names stored in the Source column of the log.
    public const string SOURCE_EMERGENCY_A = "Emergency Button A";
    public const string SOURCE_EMERGENCY_B = "Emergency Button B";
    public const string SOURCE_RESET_A = "Reset Button A";
    public const string SOURCE_RESET_B = "Reset Button B";
    public const string SOURCE_DOOR_OPEN = "Door Open Button";
    public const string SOURCE_DOOR_CLOSE = "Door Close Button";
    public const string SOURCE_ALARM_A = "Alarm Button A";
    public const string SOURCE_ALARM_B = "Alarm Button B";
    public const string SOURCE_DOORS = "Doors";
    public const string SOURCE_PASSENGER = "Passenger";

    // Door state names sent by DoorStateChanged.
    public const string DoorStateOpening = "Opening";
    public const string DoorStateOpen = "Open";
    public const string DoorStateClosing = "Closing";
    public const string DoorStateClosed = "Closed";

    public const string StatusDoorsLocked = "Doors locked while moving";
    public const string HintPassengerInside = "Passenger inside - choose a floor";

    // Letter shown on the displays while a car is in emergency.
    public const string DISPLAY_EMERGENCY = "E";
}
