namespace MiniGuys;

using System;
using Godot;

public enum GatherJobState
{
    Idle,
    SeekWork,
    TravelToRock,
    Gather,
    ReturnToBase,
    Deliver,
    TravelToBuildSite,
    Build
}

public class WorkerGatherJobLogic
{
    private const float GatherDuration = 1f;
    private const float BuildDuration = 1f;
    private const float IdleCooldown = 0.5f;

    public GatherJobState State { get; private set; } = GatherJobState.Idle;

    public bool NearestRockFound { get; set; }
    public Vector2 NearestRockPos { get; set; }
    public bool NearestBuildSiteFound { get; set; }
    public Vector2 NearestBuildSitePos { get; set; }
    public bool HasArrivedAtTarget { get; set; }
    public bool CurrentTargetValid { get; set; } = true;
    public Vector2 BasePos { get; set; }

    public Vector2 MoveTarget { get; private set; }
    public bool WantsToMove { get; private set; }
    public float GatherTimer { get; private set; }
    public float BuildTimer { get; private set; }
    public float IdleTimer { get; private set; }
    public bool IsCarryingStone { get; private set; }

    public event Action? GatherCompleted;
    public event Action? DeliveryCompleted;
    public event Action? BuildLabourApplied;

    public void Tick(float delta)
    {
        switch (State)
        {
            case GatherJobState.Idle:
                TickIdle(delta);
                break;
            case GatherJobState.SeekWork:
                TickSeekWork();
                break;
            case GatherJobState.TravelToRock:
                TickTravelToRock();
                break;
            case GatherJobState.Gather:
                TickGather(delta);
                break;
            case GatherJobState.ReturnToBase:
                TickReturnToBase();
                break;
            case GatherJobState.Deliver:
                TickDeliver();
                break;
            case GatherJobState.TravelToBuildSite:
                TickTravelToBuildSite();
                break;
            case GatherJobState.Build:
                TickBuild(delta);
                break;
        }
    }

    private void TickIdle(float delta)
    {
        IdleTimer += delta;
        if (IdleTimer >= IdleCooldown)
        {
            TransitionTo(GatherJobState.SeekWork);
        }
    }

    private void TickSeekWork()
    {
        if (NearestRockFound)
        {
            MoveTarget = NearestRockPos;
            WantsToMove = true;
            TransitionTo(GatherJobState.TravelToRock);
        }
        else if (NearestBuildSiteFound)
        {
            MoveTarget = NearestBuildSitePos;
            WantsToMove = true;
            TransitionTo(GatherJobState.TravelToBuildSite);
        }
        else
        {
            TransitionTo(GatherJobState.Idle);
        }
    }

    private void TickTravelToRock()
    {
        if (!CurrentTargetValid)
        {
            WantsToMove = false;
            TransitionTo(GatherJobState.SeekWork);
            return;
        }

        if (HasArrivedAtTarget)
        {
            WantsToMove = false;
            TransitionTo(GatherJobState.Gather);
        }
    }

    private void TickGather(float delta)
    {
        if (!CurrentTargetValid)
        {
            TransitionTo(GatherJobState.SeekWork);
            return;
        }

        GatherTimer += delta;
        if (GatherTimer >= GatherDuration)
        {
            IsCarryingStone = true;
            GatherCompleted?.Invoke();
            MoveTarget = BasePos;
            WantsToMove = true;
            TransitionTo(GatherJobState.ReturnToBase);
        }
    }

    private void TickReturnToBase()
    {
        if (HasArrivedAtTarget)
        {
            WantsToMove = false;
            TransitionTo(GatherJobState.Deliver);
        }
    }

    private void TickDeliver()
    {
        DeliveryCompleted?.Invoke();
        IsCarryingStone = false;
        TransitionTo(GatherJobState.Idle);
    }

    private void TickTravelToBuildSite()
    {
        if (!CurrentTargetValid)
        {
            WantsToMove = false;
            TransitionTo(GatherJobState.SeekWork);
            return;
        }

        if (HasArrivedAtTarget)
        {
            WantsToMove = false;
            TransitionTo(GatherJobState.Build);
        }
    }

    private void TickBuild(float delta)
    {
        if (!CurrentTargetValid)
        {
            TransitionTo(GatherJobState.SeekWork);
            return;
        }

        BuildTimer += delta;
        if (BuildTimer >= BuildDuration)
        {
            BuildTimer -= BuildDuration;
            BuildLabourApplied?.Invoke();
        }
    }

    private void TransitionTo(GatherJobState newState)
    {
        if (State == newState)
            return;

        if (newState == GatherJobState.Idle)
            IdleTimer = 0f;

        if (newState == GatherJobState.SeekWork)
            CurrentTargetValid = true;

        if (newState == GatherJobState.Gather)
            GatherTimer = 0f;

        if (newState == GatherJobState.Build)
            BuildTimer = 0f;

        State = newState;
    }
}
