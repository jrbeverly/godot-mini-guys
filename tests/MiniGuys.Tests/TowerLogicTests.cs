namespace MiniGuys.Tests;

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

public class TowerLogicTests
{
    [Fact]
    public void Initial_state_has_full_health()
    {
        var logic = new TowerLogic(maxHealth: 30);
        logic.Health.Should().Be(30);
        logic.MaxHealth.Should().Be(30);
        logic.IsDestroyed.Should().BeFalse();
    }

    [Fact]
    public void TakeDamage_reduces_health()
    {
        var logic = new TowerLogic(maxHealth: 30);
        logic.TakeDamage(10);
        logic.Health.Should().Be(20);
        logic.IsDestroyed.Should().BeFalse();
    }

    [Fact]
    public void TakeDamage_below_zero_floors_health_at_zero()
    {
        var logic = new TowerLogic(maxHealth: 20);
        logic.TakeDamage(25);
        logic.Health.Should().Be(0);
        logic.IsDestroyed.Should().BeTrue();
    }

    [Fact]
    public void Destroyed_event_fires_exactly_once()
    {
        var logic = new TowerLogic(maxHealth: 15);
        var fireCount = 0;
        logic.Destroyed += () => fireCount++;

        logic.TakeDamage(15);
        fireCount.Should().Be(1);

        logic.TakeDamage(5);
        fireCount.Should().Be(1);
    }

    [Fact]
    public void TakeDamage_after_destruction_is_noop()
    {
        var logic = new TowerLogic(maxHealth: 10);
        logic.TakeDamage(10);
        logic.Health.Should().Be(0);

        logic.TakeDamage(5);
        logic.Health.Should().Be(0);
        logic.IsDestroyed.Should().BeTrue();
    }

    [Fact]
    public void Multiple_damage_events_before_destruction_accumulate()
    {
        var logic = new TowerLogic(maxHealth: 50);
        logic.TakeDamage(15);
        logic.TakeDamage(20);
        logic.Health.Should().Be(15);
        logic.IsDestroyed.Should().BeFalse();
    }

    [Fact]
    public void Damaged_event_fires_after_each_damage()
    {
        var logic = new TowerLogic(maxHealth: 40);
        var calls = new List<(int current, int max)>();

        logic.Damaged += (h, m) => calls.Add((h, m));

        logic.TakeDamage(10);
        logic.TakeDamage(5);

        calls.Should().HaveCount(2);
        calls[0].Should().Be((30, 40));
        calls[1].Should().Be((25, 40));
    }

    [Fact]
    public void Damaged_event_fires_on_destroying_blow()
    {
        var logic = new TowerLogic(maxHealth: 30);
        var calls = new List<(int current, int max)>();
        var destroyedCount = 0;

        logic.Damaged += (h, m) => calls.Add((h, m));
        logic.Destroyed += () => destroyedCount++;

        logic.TakeDamage(40);

        calls.Should().HaveCount(1);
        calls[0].Should().Be((0, 30));
        destroyedCount.Should().Be(1);
    }

    [Fact]
    public void Damaged_event_does_not_fire_after_destruction()
    {
        var logic = new TowerLogic(maxHealth: 15);
        var damagedCount = 0;

        logic.Damaged += (_, _) => damagedCount++;
        logic.TakeDamage(15);
        damagedCount.Should().Be(1);

        logic.TakeDamage(5);
        damagedCount.Should().Be(1);
    }

    [Fact]
    public void Default_max_health_is_30()
    {
        var logic = new TowerLogic();
        logic.MaxHealth.Should().Be(30);
        logic.Health.Should().Be(30);
    }

    [Fact]
    public void Custom_max_health_is_configurable()
    {
        var logic = new TowerLogic(maxHealth: 50);
        logic.MaxHealth.Should().Be(50);
        logic.Health.Should().Be(50);
    }

    [Fact]
    public void CanFire_when_initialized()
    {
        var logic = new TowerLogic();
        logic.CanFire.Should().BeTrue();
    }

    [Fact]
    public void CannotFire_when_cooldown_not_ready()
    {
        var logic = new TowerLogic();
        logic.ResetCooldown();
        logic.CanFire.Should().BeFalse();
    }

    [Fact]
    public void CannotFire_when_destroyed()
    {
        var logic = new TowerLogic(maxHealth: 10);
        logic.TakeDamage(10);
        logic.CanFire.Should().BeFalse();
    }

    [Fact]
    public void TickCooldown_reduces_cooldown()
    {
        var logic = new TowerLogic();
        logic.ResetCooldown();
        logic.CooldownRemaining.Should().Be(logic.AttackCooldown);

        logic.TickCooldown(0.6f);
        logic.CooldownRemaining.Should().BeApproximately(0.4f, 0.001f);
    }

    [Fact]
    public void TickCooldown_does_not_go_below_zero()
    {
        var logic = new TowerLogic();
        logic.ResetCooldown();
        logic.TickCooldown(2f);
        logic.CooldownRemaining.Should().Be(0f);
    }

    [Fact]
    public void TickCooldown_enables_fire_after_full_duration()
    {
        var logic = new TowerLogic();
        logic.ResetCooldown();
        logic.CanFire.Should().BeFalse();

        logic.TickCooldown(1f);
        logic.CanFire.Should().BeTrue();
    }

    [Fact]
    public void ResetCooldown_fires_event()
    {
        var logic = new TowerLogic();
        var firedCount = 0;
        logic.Fired += () => firedCount++;

        logic.ResetCooldown();
        firedCount.Should().Be(1);
    }

    [Fact]
    public void TickCooldown_does_not_fire_event()
    {
        var logic = new TowerLogic();
        var firedCount = 0;
        logic.Fired += () => firedCount++;

        logic.ResetCooldown();
        firedCount.Should().Be(1);

        logic.TickCooldown(0.5f);
        firedCount.Should().Be(1);
    }

    [Fact]
    public void Default_attack_damage_is_10()
    {
        var logic = new TowerLogic();
        logic.AttackDamage.Should().Be(10);
    }

    [Fact]
    public void Default_attack_cooldown_is_1s()
    {
        var logic = new TowerLogic();
        logic.AttackCooldown.Should().Be(1f);
    }

    [Fact]
    public void Custom_attack_params_are_configurable()
    {
        var logic = new TowerLogic(attackDamage: 15, attackCooldown: 0.5f);
        logic.AttackDamage.Should().Be(15);
        logic.AttackCooldown.Should().Be(0.5f);
    }
}
