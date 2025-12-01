namespace MiniGuys.Tests;

using FluentAssertions;
using Xunit;

public class BuildSiteLogicTests
{
    [Fact]
    public void Initial_state_has_correct_type_and_no_progress()
    {
        var logic = new BuildSiteLogic(BuildingType.Wall, labourRequired: 10);

        logic.Type.Should().Be(BuildingType.Wall);
        logic.LabourRequired.Should().Be(10);
        logic.LabourApplied.Should().Be(0);
        logic.LabourRemaining.Should().Be(10);
        logic.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void ContributeLabour_increments_progress()
    {
        var logic = new BuildSiteLogic(BuildingType.Tower, labourRequired: 10);

        logic.ContributeLabour(3);

        logic.LabourApplied.Should().Be(3);
        logic.LabourRemaining.Should().Be(7);
        logic.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void ContributeLabour_returns_amount_actually_applied()
    {
        var logic = new BuildSiteLogic(BuildingType.Wall, labourRequired: 10);

        var applied = logic.ContributeLabour(5);

        applied.Should().Be(5);
    }

    [Fact]
    public void ContributeLabour_clamps_at_required()
    {
        var logic = new BuildSiteLogic(BuildingType.Wall, labourRequired: 10);

        var applied = logic.ContributeLabour(15);

        applied.Should().Be(10);
        logic.LabourApplied.Should().Be(10);
        logic.LabourRemaining.Should().Be(0);
        logic.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void Completed_fires_exactly_once_when_progress_reaches_required()
    {
        var logic = new BuildSiteLogic(BuildingType.Tower, labourRequired: 8);
        var fireCount = 0;
        logic.Completed += () => fireCount++;

        logic.ContributeLabour(4);
        fireCount.Should().Be(0);

        logic.ContributeLabour(4);
        fireCount.Should().Be(1);

        logic.ContributeLabour(1);
        fireCount.Should().Be(1);
    }

    [Fact]
    public void ContributeLabour_after_completion_is_noop()
    {
        var logic = new BuildSiteLogic(BuildingType.Wall, labourRequired: 5);

        logic.ContributeLabour(5);
        logic.IsComplete.Should().BeTrue();

        var applied = logic.ContributeLabour(5);

        applied.Should().Be(0);
        logic.LabourApplied.Should().Be(5);
    }

    [Fact]
    public void Multiple_contributions_accumulate_correctly()
    {
        var logic = new BuildSiteLogic(BuildingType.Tower, labourRequired: 15);

        logic.ContributeLabour(3).Should().Be(3);
        logic.ContributeLabour(7).Should().Be(7);
        logic.ContributeLabour(3).Should().Be(3);

        logic.LabourApplied.Should().Be(13);
        logic.LabourRemaining.Should().Be(2);
        logic.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void Completed_fires_on_exact_contribution()
    {
        var logic = new BuildSiteLogic(BuildingType.Wall, labourRequired: 5);
        var fireCount = 0;
        logic.Completed += () => fireCount++;

        logic.ContributeLabour(5);

        fireCount.Should().Be(1);
        logic.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void Completed_fires_on_overflow_contribution()
    {
        var logic = new BuildSiteLogic(BuildingType.Tower, labourRequired: 3);
        var fireCount = 0;
        logic.Completed += () => fireCount++;

        logic.ContributeLabour(10);

        fireCount.Should().Be(1);
        logic.LabourApplied.Should().Be(3);
    }

    [Fact]
    public void Tower_type_is_preserved()
    {
        var logic = new BuildSiteLogic(BuildingType.Tower, labourRequired: 20);

        logic.Type.Should().Be(BuildingType.Tower);
        logic.LabourRequired.Should().Be(20);
    }

    [Fact]
    public void LabourRemaining_is_zero_after_completion()
    {
        var logic = new BuildSiteLogic(BuildingType.Wall, labourRequired: 7);

        logic.ContributeLabour(7);

        logic.LabourRemaining.Should().Be(0);
    }

    [Fact]
    public void LabourRemaining_reflects_partial_progress()
    {
        var logic = new BuildSiteLogic(BuildingType.Tower, labourRequired: 10);

        logic.ContributeLabour(4);

        logic.LabourRemaining.Should().Be(6);
    }
}
