namespace MiniGuys;

using System;
using Godot;

public partial class BuildSite : Node2D
{
    private BuildSiteLogic _logic = null!;
    private ColorRect? _visual;
    private ColorRect? _progressBar;

    public BuildingType Type => _logic.Type;
    public int LabourRequired => _logic.LabourRequired;
    public int LabourApplied => _logic.LabourApplied;
    public int LabourRemaining => _logic.LabourRemaining;

    public event Action<BuildingType, Vector2>? Completed;

    public void Configure(BuildingType type, int labourRequired)
    {
        _logic = new BuildSiteLogic(type, labourRequired);
        _logic.Completed += OnLogicCompleted;
        UpdateVisual();
    }

    public override void _Ready()
    {
        _visual = GetNode<ColorRect>("Visual");
        _progressBar = GetNode<ColorRect>("ProgressBar");

        if (_logic is not null)
            UpdateVisual();
    }

    public void ContributeLabour(int amount)
    {
        var applied = _logic.ContributeLabour(amount);
        UpdateProgressBar();
    }

    private void UpdateVisual()
    {
        if (_visual is null)
            return;

        _visual.Color = _logic.Type switch
        {
            BuildingType.Wall => new Color(0.4f, 0.35f, 0.25f, 0.5f),
            BuildingType.Tower => new Color(0.85f, 0.55f, 0.1f, 0.5f),
            _ => new Color(0.5f, 0.5f, 0.5f, 0.5f)
        };
    }

    private void UpdateProgressBar()
    {
        if (_progressBar is null)
            return;

        var ratio = (float)_logic.LabourApplied / _logic.LabourRequired;
        _progressBar.Scale = new Vector2(ratio, 1f);
    }

    private void OnLogicCompleted()
    {
        var position = Position;
        var type = _logic.Type;

        var scenePath = type switch
        {
            BuildingType.Wall => "res://scenes/Wall.tscn",
            BuildingType.Tower => "res://scenes/Tower.tscn",
            _ => ""
        };

        if (scenePath.Length > 0)
        {
            var scene = GD.Load<PackedScene>(scenePath);
            var building = scene.Instantiate<Node2D>();
            building.Position = position;
            GetParent().AddChild(building);
        }

        Completed?.Invoke(type, position);
        QueueFree();
    }
}
