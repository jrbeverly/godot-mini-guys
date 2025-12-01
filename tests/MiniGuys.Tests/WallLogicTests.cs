namespace MiniGuys.Tests;

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

public class WallLogicTests
{
    [Fact]
    public void Initial_state_has_full_health()
    {
        var logic = new WallLogic(maxHealth: 50);
        logic.Health.Should().Be(50);
        logic.MaxHealth.Should().Be(50);
        logic.IsDestroyed.Should().BeFalse();
    }

    [Fact]
    public void TakeDamage_reduces_health()
    {
        var logic = new WallLogic(maxHealth: 50);
        logic.TakeDamage(10);
        logic.Health.Should().Be(40);
        logic.IsDestroyed.Should().BeFalse();
    }

    [Fact]
    public void TakeDamage_below_zero_floors_health_at_zero()
    {
        var logic = new WallLogic(maxHealth: 30);
        logic.TakeDamage(35);
        logic.Health.Should().Be(0);
        logic.IsDestroyed.Should().BeTrue();
    }

    [Fact]
    public void Destroyed_event_fires_exactly_once()
    {
        var logic = new WallLogic(maxHealth: 25);
        var fireCount = 0;
        logic.Destroyed += () => fireCount++;

        logic.TakeDamage(25);
        fireCount.Should().Be(1);

        logic.TakeDamage(5);
        fireCount.Should().Be(1);
    }

    [Fact]
    public void TakeDamage_after_destruction_is_noop()
    {
        var logic = new WallLogic(maxHealth: 10);
        logic.TakeDamage(10);
        logic.Health.Should().Be(0);

        logic.TakeDamage(5);
        logic.Health.Should().Be(0);
        logic.IsDestroyed.Should().BeTrue();
    }

    [Fact]
    public void Multiple_damage_events_before_destruction_accumulate()
    {
        var logic = new WallLogic(maxHealth: 60);
        logic.TakeDamage(15);
        logic.TakeDamage(25);
        logic.Health.Should().Be(20);
        logic.IsDestroyed.Should().BeFalse();
    }

    [Fact]
    public void Damaged_event_fires_after_each_damage()
    {
        var logic = new WallLogic(maxHealth: 50);
        var calls = new List<(int current, int max)>();

        logic.Damaged += (h, m) => calls.Add((h, m));

        logic.TakeDamage(10);
        logic.TakeDamage(5);

        calls.Should().HaveCount(2);
        calls[0].Should().Be((40, 50));
        calls[1].Should().Be((35, 50));
    }

    [Fact]
    public void Damaged_event_fires_on_destroying_blow()
    {
        var logic = new WallLogic(maxHealth: 40);
        var calls = new List<(int current, int max)>();
        var destroyedCount = 0;

        logic.Damaged += (h, m) => calls.Add((h, m));
        logic.Destroyed += () => destroyedCount++;

        logic.TakeDamage(50);

        calls.Should().HaveCount(1);
        calls[0].Should().Be((0, 40));
        destroyedCount.Should().Be(1);
    }

    [Fact]
    public void Damaged_event_does_not_fire_after_destruction()
    {
        var logic = new WallLogic(maxHealth: 15);
        var damagedCount = 0;

        logic.Damaged += (_, _) => damagedCount++;
        logic.TakeDamage(15);
        damagedCount.Should().Be(1);

        logic.TakeDamage(5);
        damagedCount.Should().Be(1);
    }

    [Fact]
    public void Default_max_health_is_50()
    {
        var logic = new WallLogic();
        logic.MaxHealth.Should().Be(50);
        logic.Health.Should().Be(50);
    }

    [Fact]
    public void Custom_max_health_is_configurable()
    {
        var logic = new WallLogic(maxHealth: 80);
        logic.MaxHealth.Should().Be(80);
        logic.Health.Should().Be(80);
    }
}
