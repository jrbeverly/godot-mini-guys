namespace MiniGuys.Tests;

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

public class CentralBaseLogicTests
{
    [Fact]
    public void Initial_state_has_full_health()
    {
        var logic = new CentralBaseLogic(maxHealth: 100);
        logic.Health.Should().Be(100);
        logic.MaxHealth.Should().Be(100);
        logic.IsDestroyed.Should().BeFalse();
    }

    [Fact]
    public void TakeDamage_reduces_health()
    {
        var logic = new CentralBaseLogic(maxHealth: 100);
        logic.TakeDamage(30);
        logic.Health.Should().Be(70);
        logic.IsDestroyed.Should().BeFalse();
    }

    [Fact]
    public void TakeDamage_below_zero_floors_health_at_zero()
    {
        var logic = new CentralBaseLogic(maxHealth: 50);
        logic.TakeDamage(60);
        logic.Health.Should().Be(0);
        logic.IsDestroyed.Should().BeTrue();
    }

    [Fact]
    public void Destroyed_event_fires_exactly_once()
    {
        var logic = new CentralBaseLogic(maxHealth: 30);
        var fireCount = 0;
        logic.Destroyed += () => fireCount++;

        logic.TakeDamage(30);
        fireCount.Should().Be(1);

        logic.TakeDamage(10);
        fireCount.Should().Be(1);
    }

    [Fact]
    public void TakeDamage_after_destruction_is_noop()
    {
        var logic = new CentralBaseLogic(maxHealth: 20);
        logic.TakeDamage(20);
        logic.Health.Should().Be(0);

        logic.TakeDamage(5);
        logic.Health.Should().Be(0);
        logic.IsDestroyed.Should().BeTrue();
    }

    [Fact]
    public void Multiple_damage_events_before_destruction_accumulate()
    {
        var logic = new CentralBaseLogic(maxHealth: 100);
        logic.TakeDamage(20);
        logic.TakeDamage(30);
        logic.Health.Should().Be(50);
        logic.IsDestroyed.Should().BeFalse();
    }

    [Fact]
    public void Damaged_event_fires_after_each_damage()
    {
        var logic = new CentralBaseLogic(maxHealth: 100);
        var calls = new List<(int current, int max)>();

        logic.Damaged += (h, m) => calls.Add((h, m));

        logic.TakeDamage(30);
        logic.TakeDamage(20);

        calls.Should().HaveCount(2);
        calls[0].Should().Be((70, 100));
        calls[1].Should().Be((50, 100));
    }

    [Fact]
    public void Damaged_event_fires_on_destroying_blow()
    {
        var logic = new CentralBaseLogic(maxHealth: 40);
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
        var logic = new CentralBaseLogic(maxHealth: 20);
        var damagedCount = 0;

        logic.Damaged += (_, _) => damagedCount++;
        logic.TakeDamage(20);
        damagedCount.Should().Be(1);

        logic.TakeDamage(10);
        damagedCount.Should().Be(1);
    }

    [Fact]
    public void Destroyed_event_transitions_game_state_to_lost_exactly_once()
    {
        var baseLogic = new CentralBaseLogic(maxHealth: 50);
        var gameState = new GameStateLogic();
        var lostCount = 0;

        baseLogic.Destroyed += () =>
        {
            lostCount++;
            gameState.TryTransitionTo(GamePhase.Lost);
        };

        gameState.TryTransitionTo(GamePhase.Preparation).Should().BeTrue();
        baseLogic.TakeDamage(50);

        lostCount.Should().Be(1);
        gameState.CurrentPhase.Should().Be(GamePhase.Lost);

        // Second Lost transition attempt must fail (terminal state)
        gameState.TryTransitionTo(GamePhase.Lost).Should().BeFalse();
    }

    [Fact]
    public void Destroyed_transition_is_idempotent_when_already_lost()
    {
        var gameState = new GameStateLogic();

        gameState.TryTransitionTo(GamePhase.Preparation).Should().BeTrue();
        gameState.TryTransitionTo(GamePhase.Lost).Should().BeTrue();
        gameState.CurrentPhase.Should().Be(GamePhase.Lost);

        // All subsequent transitions must be rejected
        gameState.TryTransitionTo(GamePhase.Lost).Should().BeFalse();
        gameState.TryTransitionTo(GamePhase.Preparation).Should().BeFalse();
        gameState.TryTransitionTo(GamePhase.Wave).Should().BeFalse();
        gameState.TryTransitionTo(GamePhase.Won).Should().BeFalse();
        gameState.TryTransitionTo(GamePhase.Setup).Should().BeFalse();
    }
}
