namespace MiniGuys;

public record WaveConfig
{
    public int EnemyCount { get; init; }
    public float SpawnIntervalSeconds { get; init; }
    public float PreparationTimeSeconds { get; init; }
    public float EnemySpeed { get; init; }
    public int EnemyHealth { get; init; }
}
