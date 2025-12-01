namespace MiniGuys.Tests;

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

public class WaveControllerLogicTests
{
    [Fact]
    public void Initial_phase_is_idle()
    {
        var logic = new WaveControllerLogic();
        logic.Phase.Should().Be(WaveControllerPhase.Idle);
        logic.WaveIndex.Should().Be(-1);
    }

    [Fact]
    public void StartFirstPreparation_starts_first_wave_preparation()
    {
        var logic = new WaveControllerLogic();
        logic.StartFirstPreparation();

        logic.Phase.Should().Be(WaveControllerPhase.Preparing);
        logic.WaveIndex.Should().Be(0);
        logic.PreparationTimeRemaining.Should().Be(WaveControllerLogic.Configs[0].PreparationTimeSeconds);
    }

    [Fact]
    public void StartFirstPreparation_is_idempotent()
    {
        var logic = new WaveControllerLogic();
        logic.StartFirstPreparation();
        var remaining = logic.PreparationTimeRemaining;

        logic.StartFirstPreparation();
        logic.PreparationTimeRemaining.Should().Be(remaining);
    }

    [Fact]
    public void Preparation_tick_decreases_timer_and_fires_event()
    {
        var logic = new WaveControllerLogic();
        logic.StartFirstPreparation();

        var calls = new List<(float remaining, float total)>();
        logic.PreparationTicked += (r, t) => calls.Add((r, t));

        logic.Tick(1.0);

        calls.Should().HaveCount(1);
        calls[0].remaining.Should().BeApproximately(
            WaveControllerLogic.Configs[0].PreparationTimeSeconds - 1f, 0.01f);
        calls[0].total.Should().Be(WaveControllerLogic.Configs[0].PreparationTimeSeconds);
    }

    [Fact]
    public void Preparation_timer_expiry_starts_wave()
    {
        var logic = new WaveControllerLogic();
        logic.StartFirstPreparation();

        var waveStartedCalls = new List<(int wave, int total)>();
        logic.WaveStarted += (w, t) => waveStartedCalls.Add((w, t));

        logic.Tick(WaveControllerLogic.Configs[0].PreparationTimeSeconds);

        logic.Phase.Should().Be(WaveControllerPhase.Wave);
        waveStartedCalls.Should().HaveCount(1);
        waveStartedCalls[0].wave.Should().Be(1);
        waveStartedCalls[0].total.Should().Be(5);
    }

    [Fact]
    public void Wave_spawns_initial_enemy_immediately()
    {
        var logic = new WaveControllerLogic();
        logic.StartFirstPreparation();
        logic.Tick(WaveControllerLogic.Configs[0].PreparationTimeSeconds);

        var spawnCalls = 0;
        logic.SpawnRequested += () => spawnCalls++;

        logic.Tick(0.0);

        spawnCalls.Should().Be(1);
        logic.EnemiesToSpawn.Should().Be(WaveControllerLogic.Configs[0].EnemyCount - 1);
    }

    [Fact]
    public void Spawn_interval_respects_config()
    {
        var logic = new WaveControllerLogic();
        logic.StartFirstPreparation();
        logic.Tick(WaveControllerLogic.Configs[0].PreparationTimeSeconds);

        var spawnCalls = 0;
        logic.SpawnRequested += () => spawnCalls++;

        logic.Tick(0.0);
        spawnCalls.Should().Be(1);

        logic.Tick(WaveControllerLogic.Configs[0].SpawnIntervalSeconds);
        spawnCalls.Should().Be(2);
    }

    [Fact]
    public void RegisterEnemyRemoved_transitions_to_next_preparation()
    {
        var logic = new WaveControllerLogic();
        logic.StartFirstPreparation();
        logic.Tick(WaveControllerLogic.Configs[0].PreparationTimeSeconds);

        var waveEndedCalls = new List<int>();
        logic.WaveEnded += w => waveEndedCalls.Add(w);

        // Simulate all enemies spawned and removed
        var enemyCount = WaveControllerLogic.Configs[0].EnemyCount;
        for (var i = 0; i < enemyCount; i++)
            logic.RegisterEnemyRemoved();

        logic.Phase.Should().Be(WaveControllerPhase.Preparing);
        logic.WaveIndex.Should().Be(1);
        waveEndedCalls.Should().HaveCount(1);
        waveEndedCalls[0].Should().Be(1);
    }

    [Fact]
    public void Last_wave_completion_fires_all_waves_cleared()
    {
        var logic = new WaveControllerLogic();
        var allClearedCount = 0;
        logic.AllWavesCleared += () => allClearedCount++;

        // Play through all 5 waves
        for (var w = 0; w < WaveControllerLogic.Configs.Length; w++)
        {
            if (w == 0)
                logic.StartFirstPreparation();
            else
            {
                logic.Phase.Should().Be(WaveControllerPhase.Preparing);
            }

            logic.Tick(WaveControllerLogic.Configs[w].PreparationTimeSeconds);

            var count = WaveControllerLogic.Configs[w].EnemyCount;
            for (var i = 0; i < count; i++)
                logic.RegisterEnemyRemoved();
        }

        logic.Phase.Should().Be(WaveControllerPhase.Complete);
        allClearedCount.Should().Be(1);
    }

    [Fact]
    public void Last_wave_completion_does_not_fire_wave_ended()
    {
        var logic = new WaveControllerLogic();
        var endedCalls = new List<int>();
        logic.WaveEnded += w => endedCalls.Add(w);

        // Play through all 5 waves
        for (var w = 0; w < WaveControllerLogic.Configs.Length; w++)
        {
            if (w == 0)
                logic.StartFirstPreparation();
            else
                logic.Phase.Should().Be(WaveControllerPhase.Preparing);

            logic.Tick(WaveControllerLogic.Configs[w].PreparationTimeSeconds);

            var count = WaveControllerLogic.Configs[w].EnemyCount;
            for (var i = 0; i < count; i++)
                logic.RegisterEnemyRemoved();
        }

        // WaveEnded fires for waves 1-4 only, not for wave 5
        endedCalls.Should().Equal(1, 2, 3, 4);
    }

    [Fact]
    public void Preparation_time_does_not_go_negative()
    {
        var logic = new WaveControllerLogic();
        logic.StartFirstPreparation();

        logic.Tick(WaveControllerLogic.Configs[0].PreparationTimeSeconds + 5f);

        logic.PreparationTimeRemaining.Should().Be(0f);
    }

    [Fact]
    public void Configs_have_five_waves_with_increasing_difficulty()
    {
        WaveControllerLogic.Configs.Should().HaveCount(5);

        for (var i = 1; i < WaveControllerLogic.Configs.Length; i++)
        {
            var prev = WaveControllerLogic.Configs[i - 1];
            var curr = WaveControllerLogic.Configs[i];

            curr.EnemyCount.Should().BeGreaterThan(prev.EnemyCount);
            curr.SpawnIntervalSeconds.Should().BeLessThan(prev.SpawnIntervalSeconds);
            curr.EnemySpeed.Should().BeGreaterThan(prev.EnemySpeed);
            curr.EnemyHealth.Should().BeGreaterThan(prev.EnemyHealth);
        }
    }

    [Fact]
    public void Second_call_to_StartFirstPreparation_is_noop()
    {
        var logic = new WaveControllerLogic();
        logic.StartFirstPreparation();
        logic.Tick(WaveControllerLogic.Configs[0].PreparationTimeSeconds);

        logic.StartFirstPreparation();
        logic.Phase.Should().Be(WaveControllerPhase.Wave);
        logic.WaveIndex.Should().Be(0);
    }

    [Fact]
    public void Wave_ended_fires_when_enemies_alive_reaches_zero()
    {
        var logic = new WaveControllerLogic();
        logic.StartFirstPreparation();
        logic.Tick(WaveControllerLogic.Configs[0].PreparationTimeSeconds);
        logic.Phase.Should().Be(WaveControllerPhase.Wave);

        var endedCalls = new List<int>();
        logic.WaveEnded += w => endedCalls.Add(w);

        // Not enough enemies — wave shouldn't end yet
        for (var i = 0; i < WaveControllerLogic.Configs[0].EnemyCount - 1; i++)
            logic.RegisterEnemyRemoved();

        endedCalls.Should().HaveCount(0);
        logic.Phase.Should().Be(WaveControllerPhase.Wave);

        // Last enemy
        logic.RegisterEnemyRemoved();

        endedCalls.Should().HaveCount(1);
        endedCalls[0].Should().Be(1);
        logic.Phase.Should().Be(WaveControllerPhase.Preparing);
    }
}
