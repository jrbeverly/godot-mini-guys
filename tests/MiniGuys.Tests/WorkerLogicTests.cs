namespace MiniGuys.Tests;

using FluentAssertions;
using Xunit;

public class WorkerLogicTests
{
    [Fact]
    public void Initial_state_has_start_position_and_no_target()
    {
        var logic = new WorkerLogic(10f, 20f);

        logic.PosX.Should().Be(10f);
        logic.PosY.Should().Be(20f);
        logic.HasTarget.Should().BeFalse();
        logic.HasArrived.Should().BeFalse();
    }

    [Fact]
    public void MoveTo_sets_target_and_clears_arrival()
    {
        var logic = new WorkerLogic(0f, 0f);

        logic.MoveTo(100f, 200f);

        logic.HasTarget.Should().BeTrue();
        logic.HasArrived.Should().BeFalse();
        logic.TargetX.Should().Be(100f);
        logic.TargetY.Should().Be(200f);
    }

    [Fact]
    public void Step_does_nothing_when_no_target()
    {
        var logic = new WorkerLogic(0f, 0f);

        logic.Step(0.5f);

        logic.PosX.Should().Be(0f);
        logic.PosY.Should().Be(0f);
        logic.HasArrived.Should().BeFalse();
    }

    [Fact]
    public void Step_moves_toward_target()
    {
        var logic = new WorkerLogic(0f, 0f, speed: 100f);
        logic.MoveTo(100f, 0f);

        logic.Step(0.5f);

        logic.PosX.Should().BeApproximately(50f, 0.01f);
        logic.PosY.Should().Be(0f);
        logic.HasArrived.Should().BeFalse();
    }

    [Fact]
    public void Step_moves_diagonally()
    {
        var logic = new WorkerLogic(0f, 0f, speed: 100f);
        logic.MoveTo(100f, 100f);

        logic.Step(1f);

        var dist = System.MathF.Sqrt(
            (logic.PosX - 100f) * (logic.PosX - 100f) +
            (logic.PosY - 100f) * (logic.PosY - 100f));
        dist.Should().BeApproximately(141.42f - 100f, 1f);
    }

    [Fact]
    public void Step_stops_within_arrival_radius()
    {
        var logic = new WorkerLogic(0f, 0f, speed: 100f, arrivalRadius: 5f);
        logic.MoveTo(3f, 0f);

        logic.Step(0.1f);

        logic.PosX.Should().Be(3f);
        logic.PosY.Should().Be(0f);
        logic.HasArrived.Should().BeTrue();
    }

    [Fact]
    public void Step_snaps_to_target_when_step_exceeds_distance()
    {
        var logic = new WorkerLogic(0f, 0f, speed: 100f);
        logic.MoveTo(5f, 0f);

        logic.Step(1f);

        logic.PosX.Should().Be(5f);
        logic.PosY.Should().Be(0f);
        logic.HasArrived.Should().BeTrue();
    }

    [Fact]
    public void Step_is_noop_after_arrival()
    {
        var logic = new WorkerLogic(0f, 0f, speed: 100f, arrivalRadius: 5f);
        logic.MoveTo(3f, 0f);
        logic.Step(0.1f);
        logic.HasArrived.Should().BeTrue();

        var posBefore = (logic.PosX, logic.PosY);
        logic.Step(0.1f);

        logic.PosX.Should().Be(posBefore.PosX);
        logic.PosY.Should().Be(posBefore.PosY);
        logic.HasArrived.Should().BeTrue();
    }

    [Fact]
    public void MoveTo_re_targets_after_arrival()
    {
        var logic = new WorkerLogic(0f, 0f, speed: 100f);
        logic.MoveTo(10f, 0f);
        logic.Step(1f);
        logic.HasArrived.Should().BeTrue();

        logic.MoveTo(50f, 0f);

        logic.HasArrived.Should().BeFalse();
        logic.HasTarget.Should().BeTrue();

        logic.Step(0.5f);
        logic.PosX.Should().BeGreaterThan(10f);
    }

    [Fact]
    public void Speed_is_configurable()
    {
        var logic = new WorkerLogic(0f, 0f, speed: 50f);
        logic.MoveTo(100f, 0f);

        logic.Step(0.5f);

        logic.PosX.Should().BeApproximately(25f, 0.01f);
    }

    [Fact]
    public void Speed_can_be_changed_after_construction()
    {
        var logic = new WorkerLogic(0f, 0f, speed: 100f);
        logic.Speed = 200f;
        logic.MoveTo(100f, 0f);

        logic.Step(0.5f);

        logic.PosX.Should().BeApproximately(100f, 0.01f);
    }
}
