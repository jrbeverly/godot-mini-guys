namespace MiniGuys.Tests;

using FluentAssertions;
using Xunit;

public class RockLogicTests
{
    [Fact]
    public void Initial_state_has_full_stone_and_not_marked()
    {
        var logic = new RockLogic(stoneAmount: 20);

        logic.StoneRemaining.Should().Be(20);
        logic.IsMarked.Should().BeFalse();
    }

    [Fact]
    public void Mark_sets_is_marked_and_fires_event()
    {
        var logic = new RockLogic(20);
        var fireCount = 0;
        logic.MarkedChanged += () => fireCount++;

        logic.Mark();

        logic.IsMarked.Should().BeTrue();
        fireCount.Should().Be(1);
    }

    [Fact]
    public void Mark_is_idempotent()
    {
        var logic = new RockLogic(20);
        logic.Mark();

        var fireCount = 0;
        logic.MarkedChanged += () => fireCount++;

        logic.Mark();

        logic.IsMarked.Should().BeTrue();
        fireCount.Should().Be(0);
    }

    [Fact]
    public void Unmark_clears_is_marked_and_fires_event()
    {
        var logic = new RockLogic(20);
        logic.Mark();

        var fireCount = 0;
        logic.MarkedChanged += () => fireCount++;

        logic.Unmark();

        logic.IsMarked.Should().BeFalse();
        fireCount.Should().Be(1);
    }

    [Fact]
    public void Unmark_is_idempotent()
    {
        var logic = new RockLogic(20);

        var fireCount = 0;
        logic.MarkedChanged += () => fireCount++;

        logic.Unmark();

        logic.IsMarked.Should().BeFalse();
        fireCount.Should().Be(0);
    }

    [Fact]
    public void Mine_returns_amount_actually_mined()
    {
        var logic = new RockLogic(stoneAmount: 20);

        var mined = logic.Mine(5);

        mined.Should().Be(5);
        logic.StoneRemaining.Should().Be(15);
    }

    [Fact]
    public void Mine_clamps_at_zero()
    {
        var logic = new RockLogic(stoneAmount: 10);

        var mined = logic.Mine(20);

        mined.Should().Be(10);
        logic.StoneRemaining.Should().Be(0);
    }

    [Fact]
    public void Mine_returns_zero_when_depleted()
    {
        var logic = new RockLogic(stoneAmount: 10);
        logic.Mine(10);

        var mined = logic.Mine(5);

        mined.Should().Be(0);
        logic.StoneRemaining.Should().Be(0);
    }

    [Fact]
    public void Depleted_fires_exactly_once()
    {
        var logic = new RockLogic(stoneAmount: 15);
        var fireCount = 0;
        logic.Depleted += () => fireCount++;

        logic.Mine(5);
        fireCount.Should().Be(0);

        logic.Mine(5);
        fireCount.Should().Be(0);

        logic.Mine(5);
        fireCount.Should().Be(1);

        logic.Mine(5);
        fireCount.Should().Be(1);
    }

    [Fact]
    public void Depleted_fires_on_exact_amount_mine()
    {
        var logic = new RockLogic(stoneAmount: 10);
        var fireCount = 0;
        logic.Depleted += () => fireCount++;

        logic.Mine(10);

        fireCount.Should().Be(1);
        logic.StoneRemaining.Should().Be(0);
    }

    [Fact]
    public void Multiple_mines_accumulate_correctly()
    {
        var logic = new RockLogic(stoneAmount: 20);

        logic.Mine(7).Should().Be(7);
        logic.Mine(8).Should().Be(8);
        logic.Mine(3).Should().Be(3);
        logic.Mine(5).Should().Be(2);

        logic.StoneRemaining.Should().Be(0);
    }
}
