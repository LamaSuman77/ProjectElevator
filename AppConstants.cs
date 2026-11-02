namespace ElevatorSystem;

/// <summary>
/// Shared numbers and text used by the whole program.
/// Using constants means we do not scatter magic numbers in the code.
/// </summary>
public static class AppConstants
{
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

    // How long the door stays open (number of timer ticks).
    public const int DoorOpenTicks = 20;

    // Direction text used on labels and in events.
    public const string DirectionUp = "Up";
    public const string DirectionDown = "Down";
    public const string DirectionIdle = "Idle";

    // Log status values required by the assignment.
    public const string StatusRequest = "Request";
    public const string StatusMoving = "Moving";
    public const string StatusArrived = "Arrived";
    public const string STATUS_EMERGENCY = "Emergency";
    public const string STATUS_RESET = "Reset";

    // Button names stored in the Source column of the log.
    public const string SOURCE_EMERGENCY_A = "Emergency Button A";
    public const string SOURCE_EMERGENCY_B = "Emergency Button B";
    public const string SOURCE_RESET_A = "Reset Button A";
    public const string SOURCE_RESET_B = "Reset Button B";

    // Letter shown on the displays while a car is in emergency.
    public const string DISPLAY_EMERGENCY = "E";

    // Car pictures live in a folder next to the exe (copied from the project Images folder).
    public const string IMAGES_FOLDER = "Images";
    public const string CAR_A_IMAGE = "ElevatorA.png";
    public const string CAR_B_IMAGE = "ElevatorB.png";
}
