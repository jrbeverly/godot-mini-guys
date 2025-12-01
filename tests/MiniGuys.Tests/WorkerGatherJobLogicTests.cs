namespace MiniGuys.Tests;

using FluentAssertions;
using Godot;
using Xunit;

public class WorkerGatherJobLogicTests
{
    private static readonly Vector2 RockPos = new(100f, 0f);
    private static readonly Vector2 BuildSitePos = new(-80f, 0f);
    private static readonly Vector2 BasePos = new(0f, 0f);

    [Fact]
    public void Initial_state_is_idle()
    {
        var logic = new WorkerGatherJobLogic();

        logic.State.Should().Be(GatherJobState.Idle);
        logic.WantsToMove.Should().BeFalse();
        logic.IsCarryingStone.Should().BeFalse();
    }

    [Fact]
    public void Stays_idle_before_cooldown_expires()
    {
        var logic = new WorkerGatherJobLogic();

        logic.Tick(0.3f);

        logic.State.Should().Be(GatherJobState.Idle);
    }

    [Fact]
    public void Idle_transitions_to_seek_work_after_cooldown()
    {
        var logic = new WorkerGatherJobLogic();

        logic.Tick(0.5f);

        logic.State.Should().Be(GatherJobState.SeekWork);
    }

    [Fact]
    public void SeekWork_with_no_rocks_returns_to_idle()
    {
        var logic = new WorkerGatherJobLogic();
        logic.Tick(0.5f);
        logic.State.Should().Be(GatherJobState.SeekWork);

        logic.NearestRockFound = false;
        logic.Tick(0f);

        logic.State.Should().Be(GatherJobState.Idle);
        logic.IdleTimer.Should().Be(0f);
    }

    [Fact]
    public void SeekWork_with_marked_rock_transitions_to_travel()
    {
        var logic = new WorkerGatherJobLogic();
        logic.Tick(0.5f);
        logic.State.Should().Be(GatherJobState.SeekWork);

        logic.NearestRockFound = true;
        logic.NearestRockPos = RockPos;
        logic.Tick(0f);

        logic.State.Should().Be(GatherJobState.TravelToRock);
        logic.WantsToMove.Should().BeTrue();
        logic.MoveTarget.Should().Be(RockPos);
    }

    [Fact]
    public void TravelToRock_stays_while_moving()
    {
        var logic = EnterTravelToRock();
        logic.HasArrivedAtTarget = false;

        logic.Tick(0f);

        logic.State.Should().Be(GatherJobState.TravelToRock);
        logic.WantsToMove.Should().BeTrue();
    }

    [Fact]
    public void TravelToRock_arrival_transitions_to_gather()
    {
        var logic = EnterTravelToRock();
        logic.HasArrivedAtTarget = true;

        logic.Tick(0f);

        logic.State.Should().Be(GatherJobState.Gather);
        logic.WantsToMove.Should().BeFalse();
        logic.GatherTimer.Should().Be(0f);
    }

    [Fact]
    public void TravelToRock_invalid_target_returns_to_seek()
    {
        var logic = EnterTravelToRock();
        logic.CurrentTargetValid = false;

        logic.Tick(0f);

        logic.State.Should().Be(GatherJobState.SeekWork);
        logic.WantsToMove.Should().BeFalse();
        logic.CurrentTargetValid.Should().BeTrue();
    }

    [Fact]
    public void Gather_timer_counts_up()
    {
        var logic = EnterGather();
        logic.CurrentTargetValid = true;

        logic.Tick(0.3f);
        logic.GatherTimer.Should().BeApproximately(0.3f, 0.001f);
        logic.State.Should().Be(GatherJobState.Gather);

        logic.Tick(0.3f);
        logic.GatherTimer.Should().BeApproximately(0.6f, 0.001f);
        logic.State.Should().Be(GatherJobState.Gather);
    }

    [Fact]
    public void Gather_completion_transitions_to_return_and_fires_event()
    {
        var logic = EnterGather();
        var gatherFired = false;
        logic.GatherCompleted += () => gatherFired = true;
        logic.BasePos = BasePos;

        logic.Tick(1.0f);

        logic.State.Should().Be(GatherJobState.ReturnToBase);
        logic.IsCarryingStone.Should().BeTrue();
        logic.WantsToMove.Should().BeTrue();
        logic.MoveTarget.Should().Be(BasePos);
        gatherFired.Should().BeTrue();
    }

    [Fact]
    public void Gather_invalid_target_returns_to_seek()
    {
        var logic = EnterGather();
        logic.CurrentTargetValid = false;

        logic.Tick(0f);

        logic.State.Should().Be(GatherJobState.SeekWork);
    }

    [Fact]
    public void ReturnToBase_arrival_transitions_to_deliver()
    {
        var logic = EnterReturnToBase();
        logic.HasArrivedAtTarget = true;

        logic.Tick(0f);

        logic.State.Should().Be(GatherJobState.Deliver);
        logic.WantsToMove.Should().BeFalse();
    }

    [Fact]
    public void ReturnToBase_stays_while_moving()
    {
        var logic = EnterReturnToBase();
        logic.HasArrivedAtTarget = false;

        logic.Tick(0f);

        logic.State.Should().Be(GatherJobState.ReturnToBase);
        logic.WantsToMove.Should().BeTrue();
    }

    [Fact]
    public void Deliver_fires_event_and_returns_to_idle()
    {
        var logic = EnterReturnToBase();
        logic.HasArrivedAtTarget = true;
        logic.Tick(0f);
        logic.State.Should().Be(GatherJobState.Deliver);

        var deliveredFired = false;
        logic.DeliveryCompleted += () => deliveredFired = true;

        logic.Tick(0f);

        logic.State.Should().Be(GatherJobState.Idle);
        logic.IsCarryingStone.Should().BeFalse();
        logic.IdleTimer.Should().Be(0f);
        deliveredFired.Should().BeTrue();
    }

    [Fact]
    public void Full_happy_path_cycle()
    {
        var logic = new WorkerGatherJobLogic();
        logic.BasePos = BasePos;

        // Idle → SeekWork
        logic.Tick(0.5f);
        logic.State.Should().Be(GatherJobState.SeekWork);

        // SeekWork → TravelToRock
        logic.NearestRockFound = true;
        logic.NearestRockPos = RockPos;
        logic.Tick(0f);
        logic.State.Should().Be(GatherJobState.TravelToRock);

        // TravelToRock → Gather
        logic.HasArrivedAtTarget = true;
        logic.Tick(0f);
        logic.State.Should().Be(GatherJobState.Gather);

        // Gather → ReturnToBase
        logic.Tick(1.0f);
        logic.State.Should().Be(GatherJobState.ReturnToBase);
        logic.IsCarryingStone.Should().BeTrue();

        // ReturnToBase → Deliver
        logic.HasArrivedAtTarget = true;
        logic.Tick(0f);
        logic.State.Should().Be(GatherJobState.Deliver);

        // Deliver → Idle
        logic.Tick(0f);
        logic.State.Should().Be(GatherJobState.Idle);
        logic.IsCarryingStone.Should().BeFalse();
    }

    [Fact]
    public void Rock_depleted_during_travel_re_seeks()
    {
        var logic = EnterTravelToRock();
        logic.CurrentTargetValid = false;

        logic.Tick(0f);

        logic.State.Should().Be(GatherJobState.SeekWork);
    }

    [Fact]
    public void Rock_depleted_during_gather_re_seeks()
    {
        var logic = EnterGather();
        logic.Tick(0.3f);

        logic.CurrentTargetValid = false;
        logic.Tick(0f);

        logic.State.Should().Be(GatherJobState.SeekWork);
    }

    [Fact]
    public void After_deliver_worker_auto_seeks_again()
    {
        var logic = new WorkerGatherJobLogic();
        logic.BasePos = BasePos;
        RunFullCycle(logic);

        logic.State.Should().Be(GatherJobState.Idle);

        logic.Tick(0.5f);
        logic.State.Should().Be(GatherJobState.SeekWork);
    }

    [Fact]
    public void MoveTarget_updates_each_time_seek_finds_rock()
    {
        var logic = new WorkerGatherJobLogic();
        logic.Tick(0.5f);

        var firstRock = new Vector2(50f, 0f);
        logic.NearestRockFound = true;
        logic.NearestRockPos = firstRock;
        logic.Tick(0f);
        logic.MoveTarget.Should().Be(firstRock);

        // Simulate target invalid → re-seek → new rock
        logic.CurrentTargetValid = false;
        logic.Tick(0f);

        var secondRock = new Vector2(200f, 0f);
        logic.NearestRockFound = true;
        logic.NearestRockPos = secondRock;
        logic.Tick(0f);
        logic.MoveTarget.Should().Be(secondRock);
    }

    [Fact]
    public void GatherCompletion_only_fires_once_per_cycle()
    {
        var logic = EnterGather();
        logic.BasePos = BasePos;
        var fireCount = 0;
        logic.GatherCompleted += () => fireCount++;

        logic.Tick(1.0f);
        fireCount.Should().Be(1);

        // Extra ticks in ReturnToBase should not fire again
        logic.Tick(0.5f);
        fireCount.Should().Be(1);
    }

    // --- Construction FSM tests ---

    [Fact]
    public void SeekWork_with_build_site_transitions_to_travel()
    {
        var logic = new WorkerGatherJobLogic();
        logic.Tick(0.5f);
        logic.State.Should().Be(GatherJobState.SeekWork);

        logic.NearestBuildSiteFound = true;
        logic.NearestBuildSitePos = BuildSitePos;
        logic.Tick(0f);

        logic.State.Should().Be(GatherJobState.TravelToBuildSite);
        logic.WantsToMove.Should().BeTrue();
        logic.MoveTarget.Should().Be(BuildSitePos);
    }

    [Fact]
    public void TravelToBuildSite_stays_while_moving()
    {
        var logic = EnterTravelToBuildSite();
        logic.HasArrivedAtTarget = false;

        logic.Tick(0f);

        logic.State.Should().Be(GatherJobState.TravelToBuildSite);
        logic.WantsToMove.Should().BeTrue();
    }

    [Fact]
    public void TravelToBuildSite_arrival_transitions_to_build()
    {
        var logic = EnterTravelToBuildSite();
        logic.HasArrivedAtTarget = true;

        logic.Tick(0f);

        logic.State.Should().Be(GatherJobState.Build);
        logic.WantsToMove.Should().BeFalse();
        logic.BuildTimer.Should().Be(0f);
    }

    [Fact]
    public void TravelToBuildSite_invalid_target_returns_to_seek()
    {
        var logic = EnterTravelToBuildSite();
        logic.CurrentTargetValid = false;

        logic.Tick(0f);

        logic.State.Should().Be(GatherJobState.SeekWork);
        logic.WantsToMove.Should().BeFalse();
        logic.CurrentTargetValid.Should().BeTrue();
    }

    [Fact]
    public void Build_fires_labour_event_at_interval()
    {
        var logic = EnterBuild();
        var fireCount = 0;
        logic.BuildLabourApplied += () => fireCount++;

        logic.Tick(1.0f);

        fireCount.Should().Be(1);
        logic.State.Should().Be(GatherJobState.Build);
    }

    [Fact]
    public void Build_fires_labour_event_repeatedly()
    {
        var logic = EnterBuild();
        var fireCount = 0;
        logic.BuildLabourApplied += () => fireCount++;

        logic.Tick(1.0f);
        fireCount.Should().Be(1);

        logic.Tick(1.0f);
        fireCount.Should().Be(2);

        logic.Tick(1.0f);
        fireCount.Should().Be(3);

        logic.State.Should().Be(GatherJobState.Build);
    }

    [Fact]
    public void Build_invalid_target_returns_to_seek()
    {
        var logic = EnterBuild();
        logic.CurrentTargetValid = false;

        logic.Tick(0f);

        logic.State.Should().Be(GatherJobState.SeekWork);
    }

    [Fact]
    public void Build_carries_surplus_time()
    {
        var logic = EnterBuild();
        var fireCount = 0;
        logic.BuildLabourApplied += () => fireCount++;

        // Tick 1.5s → one fire at 1.0s, 0.5s carry-over
        logic.Tick(1.5f);
        fireCount.Should().Be(1);

        // Tick another 0.6s → total 1.1s → second fire
        logic.Tick(0.6f);
        fireCount.Should().Be(2);
    }

    [Fact]
    public void Full_build_cycle()
    {
        var logic = new WorkerGatherJobLogic();
        logic.BasePos = BasePos;

        // Idle → SeekWork
        logic.Tick(0.5f);
        logic.State.Should().Be(GatherJobState.SeekWork);

        // SeekWork → TravelToBuildSite
        logic.NearestBuildSiteFound = true;
        logic.NearestBuildSitePos = BuildSitePos;
        logic.Tick(0f);
        logic.State.Should().Be(GatherJobState.TravelToBuildSite);

        // TravelToBuildSite → Build
        logic.HasArrivedAtTarget = true;
        logic.Tick(0f);
        logic.State.Should().Be(GatherJobState.Build);

        // Build → contribute labour
        var fired = false;
        logic.BuildLabourApplied += () => fired = true;
        logic.Tick(1.0f);
        fired.Should().BeTrue();

        // Build site completes → SeekWork
        logic.CurrentTargetValid = false;
        logic.Tick(0f);
        logic.State.Should().Be(GatherJobState.SeekWork);

        // No work → Idle
        logic.NearestBuildSiteFound = false;
        logic.Tick(0f);
        logic.State.Should().Be(GatherJobState.Idle);
    }

    [Fact]
    public void After_build_completion_worker_auto_seeks_again()
    {
        var logic = new WorkerGatherJobLogic();
        logic.BasePos = BasePos;
        RunFullBuildCycle(logic);

        logic.State.Should().Be(GatherJobState.Idle);

        logic.Tick(0.5f);
        logic.State.Should().Be(GatherJobState.SeekWork);
    }

    [Fact]
    public void Build_enters_with_fresh_timer_each_time()
    {
        var logic = EnterBuild();
        var fireCount = 0;
        logic.BuildLabourApplied += () => fireCount++;

        logic.Tick(0.7f);
        logic.BuildTimer.Should().BeApproximately(0.7f, 0.001f);

        // Site invalid → SeekWork → re-find same site → re-enter Build
        logic.CurrentTargetValid = false;
        logic.Tick(0f);
        logic.State.Should().Be(GatherJobState.SeekWork);

        logic.NearestBuildSiteFound = true;
        logic.NearestBuildSitePos = BuildSitePos;
        logic.Tick(0f);
        logic.State.Should().Be(GatherJobState.TravelToBuildSite);

        logic.HasArrivedAtTarget = true;
        logic.Tick(0f);
        logic.State.Should().Be(GatherJobState.Build);
        logic.BuildTimer.Should().Be(0f);
    }

    // --- Helpers ---

    private static WorkerGatherJobLogic EnterTravelToRock()
    {
        var logic = new WorkerGatherJobLogic();
        logic.Tick(0.5f);
        logic.NearestRockFound = true;
        logic.NearestRockPos = RockPos;
        logic.Tick(0f);
        logic.State.Should().Be(GatherJobState.TravelToRock);
        return logic;
    }

    private static WorkerGatherJobLogic EnterGather()
    {
        var logic = EnterTravelToRock();
        logic.HasArrivedAtTarget = true;
        logic.CurrentTargetValid = true;
        logic.Tick(0f);
        logic.State.Should().Be(GatherJobState.Gather);
        return logic;
    }

    private static WorkerGatherJobLogic EnterReturnToBase()
    {
        var logic = EnterGather();
        logic.BasePos = BasePos;
        logic.CurrentTargetValid = true;
        logic.Tick(1.0f);
        logic.State.Should().Be(GatherJobState.ReturnToBase);
        return logic;
    }

    private static void RunFullCycle(WorkerGatherJobLogic logic)
    {
        logic.Tick(0.5f);

        logic.NearestRockFound = true;
        logic.NearestRockPos = RockPos;
        logic.Tick(0f);

        logic.HasArrivedAtTarget = true;
        logic.Tick(0f);

        logic.Tick(1.0f);

        logic.HasArrivedAtTarget = true;
        logic.Tick(0f);

        logic.Tick(0f);
    }

    private static WorkerGatherJobLogic EnterTravelToBuildSite()
    {
        var logic = new WorkerGatherJobLogic();
        logic.Tick(0.5f);
        logic.NearestBuildSiteFound = true;
        logic.NearestBuildSitePos = BuildSitePos;
        logic.Tick(0f);
        logic.State.Should().Be(GatherJobState.TravelToBuildSite);
        return logic;
    }

    private static WorkerGatherJobLogic EnterBuild()
    {
        var logic = EnterTravelToBuildSite();
        logic.HasArrivedAtTarget = true;
        logic.CurrentTargetValid = true;
        logic.Tick(0f);
        logic.State.Should().Be(GatherJobState.Build);
        return logic;
    }

    private static void RunFullBuildCycle(WorkerGatherJobLogic logic)
    {
        logic.Tick(0.5f);

        logic.NearestBuildSiteFound = true;
        logic.NearestBuildSitePos = BuildSitePos;
        logic.Tick(0f);

        logic.HasArrivedAtTarget = true;
        logic.Tick(0f);

        logic.Tick(1.0f);

        logic.CurrentTargetValid = false;
        logic.Tick(0f);

        logic.NearestBuildSiteFound = false;
        logic.Tick(0f);
    }
}
