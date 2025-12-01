namespace MiniGuys;

using System;

public enum WaveControllerPhase { Idle, Preparing, Wave, Complete }

public class WaveControllerLogic
{
    public static readonly WaveConfig[] Configs =
    {
        new() { EnemyCount = 3, SpawnIntervalSeconds = 1.5f, PreparationTimeSeconds = 30f, EnemySpeed = 80f, EnemyHealth = 30 },
        new() { EnemyCount = 5, SpawnIntervalSeconds = 1.2f, PreparationTimeSeconds = 25f, EnemySpeed = 90f, EnemyHealth = 35 },
        new() { EnemyCount = 7, SpawnIntervalSeconds = 1.0f, PreparationTimeSeconds = 20f, EnemySpeed = 100f, EnemyHealth = 40 },
        new() { EnemyCount = 9, SpawnIntervalSeconds = 0.8f, PreparationTimeSeconds = 18f, EnemySpeed = 110f, EnemyHealth = 45 },
        new() { EnemyCount = 12, SpawnIntervalSeconds = 0.6f, PreparationTimeSeconds = 15f, EnemySpeed = 120f, EnemyHealth = 50 },
    };

    public WaveControllerPhase Phase { get; private set; } = WaveControllerPhase.Idle;
    public int WaveIndex { get; private set; } = -1;
    public int TotalWaves => Configs.Length;
    public float PreparationTimeRemaining { get; private set; }
    public float SpawnTimer { get; private set; }
    public int EnemiesToSpawn { get; private set; }
    public int EnemiesAlive { get; private set; }
    public WaveConfig? CurrentConfig { get; private set; }

    public event Action<float, float>? PreparationTicked;
    public event Action<int, int>? WaveStarted;
    public event Action<int>? WaveEnded;
    public event Action? AllWavesCleared;
    public event Action? SpawnRequested;

    public void StartFirstPreparation()
    {
        if (Phase != WaveControllerPhase.Idle)
            return;
        NextPreparation();
    }

    public void Tick(double delta)
    {
        switch (Phase)
        {
            case WaveControllerPhase.Preparing:
                PreparationTimeRemaining -= (float)delta;
                if (PreparationTimeRemaining < 0f)
                    PreparationTimeRemaining = 0f;

                PreparationTicked?.Invoke(PreparationTimeRemaining, CurrentConfig!.PreparationTimeSeconds);

                if (PreparationTimeRemaining <= 0f)
                    BeginWave();
                break;

            case WaveControllerPhase.Wave:
                if (EnemiesToSpawn > 0)
                {
                    SpawnTimer -= (float)delta;
                    if (SpawnTimer <= 0f)
                    {
                        SpawnTimer = CurrentConfig!.SpawnIntervalSeconds;
                        SpawnRequested?.Invoke();
                        EnemiesToSpawn--;
                    }
                }
                break;
        }
    }

    public void RegisterEnemyRemoved()
    {
        EnemiesAlive--;
        if (EnemiesAlive <= 0 && Phase == WaveControllerPhase.Wave)
        {
            var completedWave = WaveIndex;

            if (WaveIndex + 1 >= TotalWaves)
            {
                Phase = WaveControllerPhase.Complete;
                AllWavesCleared?.Invoke();
            }
            else
            {
                WaveEnded?.Invoke(completedWave + 1);
                NextPreparation();
            }
        }
    }

    private void NextPreparation()
    {
        WaveIndex++;
        if (WaveIndex >= Configs.Length)
        {
            Phase = WaveControllerPhase.Complete;
            AllWavesCleared?.Invoke();
            return;
        }

        CurrentConfig = Configs[WaveIndex];
        PreparationTimeRemaining = CurrentConfig.PreparationTimeSeconds;
        Phase = WaveControllerPhase.Preparing;
    }

    private void BeginWave()
    {
        Phase = WaveControllerPhase.Wave;
        EnemiesToSpawn = CurrentConfig!.EnemyCount;
        EnemiesAlive = CurrentConfig.EnemyCount;
        SpawnTimer = 0f;
        WaveStarted?.Invoke(WaveIndex + 1, TotalWaves);
    }
}
