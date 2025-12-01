namespace MiniGuys.Tests;

using FluentAssertions;
using Xunit;

public class EnemyLogicTests
{
    [Fact]
    public void Initial_state_has_full_health()
    {
        var logic = new EnemyLogic(maxHealth: 30, damage: 10, speed: 80f);

        logic.Health.Should().Be(30);
        logic.MaxHealth.Should().Be(30);
        logic.Damage.Should().Be(10);
        logic.Speed.Should().Be(80f);
        logic.IsDead.Should().BeFalse();
    }

    [Fact]
    public void TakeDamage_reduces_health()
    {
        var logic = new EnemyLogic(maxHealth: 30, damage: 10, speed: 80f);

        logic.TakeDamage(10);

        logic.Health.Should().Be(20);
        logic.IsDead.Should().BeFalse();
    }

    [Fact]
    public void TakeDamage_below_zero_floors_health_at_zero()
    {
        var logic = new EnemyLogic(maxHealth: 20, damage: 10, speed: 80f);

        logic.TakeDamage(30);

        logic.Health.Should().Be(0);
        logic.IsDead.Should().BeTrue();
    }

    [Fact]
    public void Killed_event_fires_exactly_once()
    {
        var logic = new EnemyLogic(maxHealth: 20, damage: 10, speed: 80f);
        var fireCount = 0;
        logic.Killed += () => fireCount++;

        logic.TakeDamage(20);
        fireCount.Should().Be(1);

        logic.TakeDamage(10);
        fireCount.Should().Be(1);
    }

    [Fact]
    public void TakeDamage_after_death_is_noop()
    {
        var logic = new EnemyLogic(maxHealth: 20, damage: 10, speed: 80f);

        logic.TakeDamage(20);
        logic.Health.Should().Be(0);

        logic.TakeDamage(5);
        logic.Health.Should().Be(0);
        logic.IsDead.Should().BeTrue();
    }

    [Fact]
    public void Initial_state_is_not_attacking_wall()
    {
        var logic = new EnemyLogic(maxHealth: 30, damage: 10, speed: 80f);

        logic.IsAttackingWall.Should().BeFalse();
        logic.CanAttackWall.Should().BeFalse();
    }

    [Fact]
    public void BeginAttackingWall_sets_state_and_resets_cooldown()
    {
        var logic = new EnemyLogic(maxHealth: 30, damage: 10, speed: 80f, attackCooldown: 1.5f);

        logic.BeginAttackingWall();

        logic.IsAttackingWall.Should().BeTrue();
        logic.CooldownRemaining.Should().Be(1.5f);
        logic.CanAttackWall.Should().BeFalse();
    }

    [Fact]
    public void TickAttackCooldown_reduces_remaining()
    {
        var logic = new EnemyLogic(maxHealth: 30, damage: 10, speed: 80f, attackCooldown: 1f);
        logic.BeginAttackingWall();

        logic.TickAttackCooldown(0.4f);
        logic.CooldownRemaining.Should().BeApproximately(0.6f, 0.001f);
    }

    [Fact]
    public void CanAttackWall_when_cooldown_expires()
    {
        var logic = new EnemyLogic(maxHealth: 30, damage: 10, speed: 80f, attackCooldown: 1f);
        logic.BeginAttackingWall();

        logic.TickAttackCooldown(1f);

        logic.CooldownRemaining.Should().Be(0f);
        logic.CanAttackWall.Should().BeTrue();
    }

    [Fact]
    public void ResetAttackCooldown_restores_full_cooldown()
    {
        var logic = new EnemyLogic(maxHealth: 30, damage: 10, speed: 80f, attackCooldown: 1f);
        logic.BeginAttackingWall();
        logic.TickAttackCooldown(1f);

        logic.ResetAttackCooldown();

        logic.CooldownRemaining.Should().Be(1f);
        logic.CanAttackWall.Should().BeFalse();
    }

    [Fact]
    public void StopAttackingWall_clears_state()
    {
        var logic = new EnemyLogic(maxHealth: 30, damage: 10, speed: 80f, attackCooldown: 1f);
        logic.BeginAttackingWall();

        logic.StopAttackingWall();

        logic.IsAttackingWall.Should().BeFalse();
        logic.CooldownRemaining.Should().Be(0f);
        logic.CanAttackWall.Should().BeFalse();
    }

    [Fact]
    public void TickAttackCooldown_is_noop_when_not_attacking()
    {
        var logic = new EnemyLogic(maxHealth: 30, damage: 10, speed: 80f, attackCooldown: 1f);

        logic.TickAttackCooldown(0.5f);

        logic.CooldownRemaining.Should().Be(0f);
    }

    [Fact]
    public void Default_attack_cooldown_is_one_second()
    {
        var logic = new EnemyLogic(maxHealth: 30, damage: 10, speed: 80f);

        logic.AttackCooldown.Should().Be(1f);
    }
}
