namespace MiniGuys;

public class WorkerLogic
{
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float TargetX { get; private set; }
    public float TargetY { get; private set; }
    public bool HasTarget { get; private set; }
    public bool HasArrived { get; private set; }
    public float Speed { get; set; }
    public float ArrivalRadius { get; }

    public WorkerLogic(float startX, float startY, float speed = 100f, float arrivalRadius = 5f)
    {
        PosX = startX;
        PosY = startY;
        Speed = speed;
        ArrivalRadius = arrivalRadius;
    }

    public void MoveTo(float x, float y)
    {
        TargetX = x;
        TargetY = y;
        HasTarget = true;
        HasArrived = false;
    }

    public void Step(float delta)
    {
        if (!HasTarget || HasArrived)
            return;

        var dx = TargetX - PosX;
        var dy = TargetY - PosY;
        var dist = System.MathF.Sqrt(dx * dx + dy * dy);

        if (dist <= ArrivalRadius)
        {
            PosX = TargetX;
            PosY = TargetY;
            HasArrived = true;
            return;
        }

        var step = Speed * delta;
        if (step >= dist)
        {
            PosX = TargetX;
            PosY = TargetY;
            HasArrived = true;
            return;
        }

        PosX += dx / dist * step;
        PosY += dy / dist * step;
    }
}
