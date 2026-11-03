using System.Drawing;

namespace ElevatorSystem;

/// <summary>
/// States of the animated passenger boy.
/// </summary>
public enum PassengerState
{
    WalkingToDoors,
    WaitingForElevator,
    Boarding,
    Riding,
    Exiting,
    Gone
}

/// <summary>
/// An animated boy who appears when a hall button is pressed, walks to the assigned
/// elevator, waits for it, boards when doors open, rides with the car, and exits.
/// Drawn purely in GDI+ (System.Drawing) code without external image files.
/// </summary>
public class Passenger
{
    public float X { get; set; }
    public float Y { get; set; }
    public int CurrentFloor { get; set; }
    public int AssignedElevatorId { get; set; }
    public int DestinationFloor { get; set; }
    public PassengerState State { get; set; }
    public bool IsFacingRight { get; set; }
    public int WalkStep { get; set; }

    public const int PassengerWidth = 18;
    public const int PassengerHeight = 44;

    /// <summary>
    /// Creates a passenger boy at a specific floor row and horizontal start position.
    /// </summary>
    public Passenger(int currentFloor, int assignedElevatorId, float startX, float baselineY, bool facingRight)
    {
        CurrentFloor = currentFloor;
        AssignedElevatorId = assignedElevatorId;
        X = startX;
        Y = baselineY - PassengerHeight;
        IsFacingRight = facingRight;
        State = PassengerState.WalkingToDoors;
        DestinationFloor = 0;
        WalkStep = 0;
    }

    /// <summary>
    /// True when the boy has walked into the middle of the car.
    /// </summary>
    public bool HasSteppedInside(float doorCenterX)
    {
        return Math.Abs(X - doorCenterX) <= 3f;
    }

    /// <summary>
    /// Updates position and animation state driven by the existing 30 ms timer.
    /// waitX is just outside the doors. doorCenterX is inside the car.
    /// </summary>
    public void Update(Elevator elevator, float doorCenterX, float waitX, float baselineY, int buildingWidth)
    {
        if (State == PassengerState.Gone)
        {
            return;
        }

        WalkStep++;

        switch (State)
        {
            case PassengerState.WalkingToDoors:
                Y = baselineY - PassengerHeight;
                WalkToward(waitX, 2.6f);
                if (Math.Abs(X - waitX) <= 2.6f)
                {
                    X = waitX;
                    State = PassengerState.WaitingForElevator;
                }
                break;

            case PassengerState.WaitingForElevator:
                Y = baselineY - PassengerHeight;
                X = waitX;
                break;

            case PassengerState.Boarding:
                elevator.IsBoardingOrExiting = true;
                float carFloorY = elevator.CarY + AppConstants.CarHeight - 8;
                Y = carFloorY - PassengerHeight;
                WalkToward(doorCenterX, 1.8f);
                break;

            case PassengerState.Riding:
                float rideFloorY = elevator.CarY + AppConstants.CarHeight - 8;
                Y = rideFloorY - PassengerHeight;
                X = doorCenterX;
                break;

            case PassengerState.Exiting:
                // Walking off the walkway toward the edge of the building
                Y = baselineY - PassengerHeight;
                float exitSpeed = 2.8f;

                if (AssignedElevatorId == AppConstants.ElevatorIdA)
                {
                    // Elevator A passenger exits off the left edge
                    X -= exitSpeed;
                    IsFacingRight = false;

                    // Release door hold once cleared the shaft doorway
                    if (X < doorCenterX - 35)
                    {
                        elevator.IsBoardingOrExiting = false;
                    }

                    if (X < -PassengerWidth - 10)
                    {
                        State = PassengerState.Gone;
                    }
                }
                else
                {
                    // Elevator B passenger exits off the right edge
                    X += exitSpeed;
                    IsFacingRight = true;

                    // Release door hold once cleared the shaft doorway
                    if (X > doorCenterX + 35)
                    {
                        elevator.IsBoardingOrExiting = false;
                    }

                    if (X > buildingWidth + 10)
                    {
                        State = PassengerState.Gone;
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Walks left or right toward a target X position.
    /// </summary>
    private void WalkToward(float targetX, float speed)
    {
        if (X < targetX)
        {
            X += speed;
            IsFacingRight = true;
        }
        else if (X > targetX)
        {
            X -= speed;
            IsFacingRight = false;
        }
    }

    /// <summary>
    /// Draws the boy with head, shirt, pants, swinging legs, and arms in GDI+.
    /// Scaled to ~65% of door height so he looks realistic on the floor walkway.
    /// </summary>
    public void Draw(Graphics g)
    {
        if (State == PassengerState.Gone)
        {
            return;
        }

        int px = (int)X;
        int py = (int)Y;

        // Walking cycle leg swinging
        bool isWalking = (State == PassengerState.WalkingToDoors || State == PassengerState.Exiting);
        float swing = isWalking ? (float)(Math.Sin(WalkStep * 0.45) * 5.0) : 0f;

        // Visual colors
        Color skin = Color.FromArgb(255, 220, 185);
        Color hair = Color.FromArgb(60, 40, 20);
        Color shirt = AssignedElevatorId == AppConstants.ElevatorIdA
            ? Color.FromArgb(220, 50, 40)    // Red shirt for Elevator A passenger
            : Color.FromArgb(30, 120, 210);  // Blue shirt for Elevator B passenger
        Color pants = Color.FromArgb(40, 50, 75);
        Color shoes = Color.FromArgb(25, 25, 25);

        using (SolidBrush skinBrush = new SolidBrush(skin))
        using (SolidBrush hairBrush = new SolidBrush(hair))
        using (SolidBrush shirtBrush = new SolidBrush(shirt))
        using (SolidBrush pantsBrush = new SolidBrush(pants))
        using (SolidBrush shoeBrush = new SolidBrush(shoes))
        using (Pen outlinePen = new Pen(Color.FromArgb(35, 35, 35), 1))
        {
            // 1. Legs and shoes
            int legTopY = py + 26;
            int legH = 14;

            // Leg 1
            Point leg1Start = new Point(px + 4, legTopY);
            Point leg1End = new Point((int)(px + 4 + swing), legTopY + legH);
            g.DrawLine(new Pen(pantsBrush, 4), leg1Start, leg1End);
            g.FillRectangle(shoeBrush, (int)(px + 2 + swing), legTopY + legH - 2, 6, 4);

            // Leg 2
            Point leg2Start = new Point(px + 12, legTopY);
            Point leg2End = new Point((int)(px + 12 - swing), legTopY + legH);
            g.DrawLine(new Pen(pantsBrush, 4), leg2Start, leg2End);
            g.FillRectangle(shoeBrush, (int)(px + 10 - swing), legTopY + legH - 2, 6, 4);

            // 2. Torso (Shirt)
            Rectangle torso = new Rectangle(px + 3, py + 12, 12, 14);
            g.FillRectangle(shirtBrush, torso);
            g.DrawRectangle(outlinePen, torso);

            // 3. Arms
            float armSwing = -swing * 0.7f;
            g.DrawLine(new Pen(shirtBrush, 3), px + 3, py + 14, (int)(px + 1 + armSwing), py + 23);
            g.FillEllipse(skinBrush, (int)(px + armSwing), py + 22, 3, 3);

            g.DrawLine(new Pen(shirtBrush, 3), px + 14, py + 14, (int)(px + 16 - armSwing), py + 23);
            g.FillEllipse(skinBrush, (int)(px + 15 - armSwing), py + 22, 3, 3);

            // 4. Head and Hair
            Rectangle head = new Rectangle(px + 3, py, 12, 12);
            g.FillEllipse(skinBrush, head);
            g.DrawEllipse(outlinePen, head);

            // Hair
            g.FillPie(hairBrush, head.X - 1, head.Y - 2, head.Width + 2, head.Height + 2, 180, 180);

            // Eye facing movement direction
            int eyeX = IsFacingRight ? px + 10 : px + 5;
            g.FillEllipse(Brushes.Black, eyeX, py + 5, 2, 2);
        }
    }
}
