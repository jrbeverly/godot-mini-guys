namespace MiniGuys;

using System.Collections.Generic;
using Godot;

public partial class WaveController : Node
{
    private readonly WaveControllerLogic _logic = new();
    private readonly List<Enemy> _activeEnemies = new();
    private readonly RandomNumberGenerator _rng = new();

    private CentralBase? _base;
    private Hud? _hud;
    private Marker2D[] _spawnMarkers = [];
    private GameState? _gameState;

    public override void _Ready()
    {
        _logic.PreparationTicked += OnPreparationTicked;
        _logic.WaveStarted += OnWaveStarted;
        _logic.WaveEnded += OnWaveEnded;
        _logic.AllWavesCleared += OnAllWavesCleared;
        _logic.SpawnRequested += OnSpawnRequested;
        _rng.Randomize();
    }

    public void Initialize(CentralBase targetBase, Hud hud, Marker2D[] spawnMarkers)
    {
        _base = targetBase;
        _hud = hud;
        _spawnMarkers = spawnMarkers;
        _gameState = GetNode<GameState>("/root/GameState");

        _gameState.TransitionTo(GamePhase.Preparation);
        _logic.StartFirstPreparation();
        _hud.OnPreparationTicked(_logic.CurrentConfig!.PreparationTimeSeconds, _logic.CurrentConfig.PreparationTimeSeconds);
    }

    public override void _Process(double delta)
    {
        if (_gameState is null)
            return;

        var phase = _gameState.CurrentPhase;
        if (phase == GamePhase.Won || phase == GamePhase.Lost)
            return;

        _logic.Tick(delta);
        CleanupDeadEnemies();
    }

    private void OnPreparationTicked(float remaining, float total)
    {
        _hud?.OnPreparationTicked(remaining, total);
    }

    private void OnWaveStarted(int waveNumber, int totalWaves)
    {
        _gameState?.TransitionTo(GamePhase.Wave);
        _hud?.OnWaveStarted(waveNumber, totalWaves);
    }

    private void OnWaveEnded(int waveNumber)
    {
        if (_gameState?.CurrentPhase is GamePhase.Won or GamePhase.Lost)
            return;

        _gameState?.TransitionTo(GamePhase.Preparation);
        _hud?.OnWaveEnded(_logic.WaveIndex + 2, _logic.TotalWaves);
    }

    private void OnAllWavesCleared()
    {
        if (_gameState?.CurrentPhase is GamePhase.Won or GamePhase.Lost)
            return;

        _gameState?.TransitionTo(GamePhase.Won);
    }

    private void OnSpawnRequested()
    {
        if (_base is null || _spawnMarkers.Length == 0)
            return;

        var marker = _spawnMarkers[_rng.RandiRange(0, _spawnMarkers.Length - 1)];
        var enemy = Enemy.Spawn(marker.Position);
        enemy.Configure(
            _logic.CurrentConfig!.EnemyHealth,
            10,
            _logic.CurrentConfig.EnemySpeed);
        enemy.SetTarget(_base);
        enemy.TreeExited += () => OnEnemyRemoved(enemy);
        _activeEnemies.Add(enemy);
        GetParent().AddChild(enemy);
    }

    private void OnEnemyRemoved(Enemy enemy)
    {
        _activeEnemies.Remove(enemy);
        if (_gameState?.CurrentPhase is GamePhase.Won or GamePhase.Lost)
            return;

        if (_logic.EnemiesToSpawn == 0)
            _logic.RegisterEnemyRemoved();
    }

    private void CleanupDeadEnemies()
    {
        _activeEnemies.RemoveAll(e => !GodotObject.IsInstanceValid(e));
    }
}
