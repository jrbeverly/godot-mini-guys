namespace MiniGuys;

using System.Linq;
using Godot;

public partial class Map : Node2D
{
    private const int WallStoneCost = 5;
    private const int TowerStoneCost = 10;
    private const int WallLabourRequired = 10;
    private const int TowerLabourRequired = 15;
    private const float PlacementInvalidRadiusBase = 60f;
    private const float PlacementInvalidRadiusStructure = 30f;

    private CentralBase? _base;
    private Hud? _hud;
    private WaveController? _waveController;
    private GameState? _gameState;

    private BuildingType? _placingType;
    private ColorRect? _ghostPreview;

    public override void _Ready()
    {
        var baseScene = GD.Load<PackedScene>("res://scenes/CentralBase.tscn");
        _base = baseScene.Instantiate<CentralBase>();
        var spawnMarker = GetNode<Marker2D>("BaseSpawn");
        _base.Position = spawnMarker.Position;
        AddChild(_base);

        var hudScene = GD.Load<PackedScene>("res://scenes/Hud.tscn");
        _hud = hudScene.Instantiate<Hud>();
        AddChild(_hud);

        _base.HealthChanged += _hud.OnBaseHealthChanged;

        _gameState = GetNode<GameState>("/root/GameState");
        _gameState.StoneChanged += _hud.OnStoneChanged;
        _gameState.Won += OnWon;
        _gameState.Lost += OnLost;
        _hud.OnStoneChanged(_gameState.Stone);

        _hud.WallButtonPressed += () => EnterBuildMode(BuildingType.Wall);
        _hud.TowerButtonPressed += () => EnterBuildMode(BuildingType.Tower);

        _waveController = new WaveController();
        AddChild(_waveController);

        var markers = new[]
        {
            GetNode<Marker2D>("EnemySpawn0"),
            GetNode<Marker2D>("EnemySpawn1"),
            GetNode<Marker2D>("EnemySpawn2"),
            GetNode<Marker2D>("EnemySpawn3"),
        };

        _waveController.Initialize(_base, _hud, markers);

        SpawnWorkers();
        SpawnRocks();
    }

    public override void _Process(double delta)
    {
        if (_placingType is not null && _ghostPreview is not null)
            UpdateGhostPreview();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_placingType is not null)
        {
            if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            {
                TryPlaceBuilding();
                return;
            }

            if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right }
                || @event is InputEventKey { Pressed: true, Keycode: Key.Escape })
            {
                ExitBuildMode();
                return;
            }
        }

        if (@event is InputEventKey { Pressed: true, Keycode: Key.E }
            && _base != null
            && _gameState?.CurrentPhase == GamePhase.Wave)
        {
            var spawnMarker = GetNode<Marker2D>("EnemySpawn0");
            var enemy = Enemy.Spawn(spawnMarker.Position);
            enemy.SetTarget(_base);
            AddChild(enemy);
        }
    }

    private void EnterBuildMode(BuildingType type)
    {
        _placingType = type;

        _ghostPreview = new ColorRect
        {
            Size = new Vector2(40f, 40f),
            Position = new Vector2(-20f, -20f)
        };
        AddChild(_ghostPreview);
        UpdateGhostPreview();
    }

    private void ExitBuildMode()
    {
        _placingType = null;
        _ghostPreview?.QueueFree();
        _ghostPreview = null;
    }

    private void UpdateGhostPreview()
    {
        if (_ghostPreview is null || _placingType is null)
            return;

        var mousePos = GetLocalMousePosition();
        _ghostPreview.Position = mousePos - new Vector2(20f, 20f);

        var valid = IsPlacementValid(mousePos) && (_gameState?.CurrentPhase == GamePhase.Preparation);
        _ghostPreview.Color = _placingType.Value switch
        {
            BuildingType.Wall => new Color(valid ? 0.4f : 0.6f, valid ? 0.35f : 0.15f, valid ? 0.25f : 0.15f, 0.5f),
            BuildingType.Tower => new Color(valid ? 0.85f : 0.9f, valid ? 0.55f : 0.4f, valid ? 0.1f : 0.08f, 0.5f),
            _ => new Color(0.5f, 0.5f, 0.5f, 0.5f)
        };
    }

    private bool IsPlacementValid(Vector2 position)
    {
        if (_base is not null && position.DistanceTo(_base.GlobalPosition) < PlacementInvalidRadiusBase)
            return false;

        foreach (var child in GetChildren())
        {
            if (child is BuildSite or Rock)
            {
                var childPos = ((Node2D)child).GlobalPosition;
                if (position.DistanceTo(childPos) < PlacementInvalidRadiusStructure)
                    return false;
            }
        }

        return true;
    }

    private void TryPlaceBuilding()
    {
        if (_placingType is null || _gameState is null)
            return;

        if (_gameState.CurrentPhase != GamePhase.Preparation)
            return;

        var position = GetLocalMousePosition();
        if (!IsPlacementValid(position))
            return;

        var stoneCost = _placingType.Value == BuildingType.Wall ? WallStoneCost : TowerStoneCost;
        if (!_gameState.TrySpendStone(stoneCost))
            return;

        var labour = _placingType.Value == BuildingType.Wall ? WallLabourRequired : TowerLabourRequired;

        var buildSiteScene = GD.Load<PackedScene>("res://scenes/BuildSite.tscn");
        var buildSite = buildSiteScene.Instantiate<BuildSite>();
        buildSite.Position = position;
        buildSite.Configure(_placingType.Value, labour);
        AddChild(buildSite);

        ExitBuildMode();
    }

    private void SpawnRocks()
    {
        var rockScene = GD.Load<PackedScene>("res://scenes/Rock.tscn");

        for (var i = 0; i < 6; i++)
        {
            var marker = GetNode<Marker2D>($"RockSpawn{i}");
            var rock = rockScene.Instantiate<Rock>();
            rock.Position = marker.Position;
            AddChild(rock);
        }
    }

    private void SpawnWorkers()
    {
        if (_base is null)
            return;

        var workerScene = GD.Load<PackedScene>("res://scenes/Worker.tscn");
        var basePos = _base.Position;

        var offsets = new[]
        {
            new Vector2(20f, 0f),
            new Vector2(-20f, 0f),
            new Vector2(0f, 20f),
            new Vector2(0f, -20f),
        };

        foreach (var offset in offsets)
        {
            var worker = workerScene.Instantiate<Worker>();
            worker.Position = basePos + offset;
            AddChild(worker);
        }
    }

    private void OnWon()
    {
        var winScreenScene = GD.Load<PackedScene>("res://scenes/WinScreen.tscn");
        var winScreen = winScreenScene.Instantiate();
        AddChild(winScreen);
    }

    private void OnLost()
    {
        var loseScreenScene = GD.Load<PackedScene>("res://scenes/LoseScreen.tscn");
        var loseScreen = loseScreenScene.Instantiate();
        AddChild(loseScreen);
    }
}
