namespace MiniGuys;

using System.Linq;
using Godot;

public partial class Worker : Node2D
{
    private WorkerLogic _movement = null!;
    private WorkerGatherJobLogic _job = null!;
    private ColorRect? _visual;

    private CentralBase? _base;
    private GameState? _gameState;
    private Rock? _currentRock;
    private BuildSite? _currentBuildSite;

    public bool HasArrived => _movement.HasArrived;

    public float Speed
    {
        get => _movement.Speed;
        set => _movement.Speed = value;
    }

    public void MoveTo(Vector2 target) => _movement.MoveTo(target.X, target.Y);

    public override void _Ready()
    {
        _movement = new WorkerLogic(Position.X, Position.Y);
        _job = new WorkerGatherJobLogic();
        _visual = GetNode<ColorRect>("Visual");

        _job.GatherCompleted += OnGatherCompleted;
        _job.DeliveryCompleted += OnDeliveryCompleted;
        _job.BuildLabourApplied += OnBuildLabourApplied;
    }

    public override void _Process(double delta)
    {
        FindBaseAndService();

        UpdateJobInputs();
        _job.Tick((float)delta);

        if (_job.WantsToMove)
            _movement.MoveTo(_job.MoveTarget.X, _job.MoveTarget.Y);
    }

    public override void _PhysicsProcess(double delta)
    {
        _movement.Step((float)delta);
        Position = new Vector2(_movement.PosX, _movement.PosY);
    }

    private void FindBaseAndService()
    {
        if (_base is null)
        {
            foreach (var child in GetParent().GetChildren())
            {
                if (child is CentralBase cb)
                {
                    _base = cb;
                    break;
                }
            }
        }

        _gameState ??= GetNode<GameState>("/root/GameState");
    }

    private void UpdateJobInputs()
    {
        if (_base is not null)
            _job.BasePos = _base.Position;

        _job.HasArrivedAtTarget = _movement.HasArrived;

        var markedRocks = GetParent().GetChildren().OfType<Rock>()
            .Where(r => r.IsMarked && r.StoneRemaining > 0)
            .ToArray();

        var activeBuildSites = GetParent().GetChildren().OfType<BuildSite>()
            .Where(b => b.LabourRemaining > 0)
            .ToArray();

        if (_currentRock is not null)
            _job.CurrentTargetValid = _currentRock.IsMarked && _currentRock.StoneRemaining > 0;
        else if (_currentBuildSite is not null)
        {
            if (GodotObject.IsInstanceValid(_currentBuildSite))
                _job.CurrentTargetValid = _currentBuildSite.LabourRemaining > 0;
            else
            {
                _job.CurrentTargetValid = false;
                _currentBuildSite = null;
            }
        }

        // Find nearest rock
        Rock? nearestRock = null;
        float nearestRockDist = float.MaxValue;
        if (markedRocks.Length > 0)
        {
            nearestRock = markedRocks.MinBy(r => Position.DistanceSquaredTo(r.Position));
            if (nearestRock is not null)
                nearestRockDist = Position.DistanceSquaredTo(nearestRock.Position);
        }

        // Find nearest build site
        BuildSite? nearestBuildSite = null;
        float nearestBuildSiteDist = float.MaxValue;
        if (activeBuildSites.Length > 0)
        {
            nearestBuildSite = activeBuildSites.MinBy(b => Position.DistanceSquaredTo(b.Position));
            if (nearestBuildSite is not null)
                nearestBuildSiteDist = Position.DistanceSquaredTo(nearestBuildSite.Position);
        }

        // Reset job found flags and current targets
        _job.NearestRockFound = false;
        _job.NearestBuildSiteFound = false;
        _currentRock = null;
        _currentBuildSite = null;

        if (nearestRock is not null && nearestBuildSite is not null)
        {
            if (nearestRockDist <= nearestBuildSiteDist)
            {
                _job.NearestRockFound = true;
                _job.NearestRockPos = nearestRock.Position;
                _currentRock = nearestRock;
            }
            else
            {
                _job.NearestBuildSiteFound = true;
                _job.NearestBuildSitePos = nearestBuildSite.Position;
                _currentBuildSite = nearestBuildSite;
            }
        }
        else if (nearestRock is not null)
        {
            _job.NearestRockFound = true;
            _job.NearestRockPos = nearestRock.Position;
            _currentRock = nearestRock;
        }
        else if (nearestBuildSite is not null)
        {
            _job.NearestBuildSiteFound = true;
            _job.NearestBuildSitePos = nearestBuildSite.Position;
            _currentBuildSite = nearestBuildSite;
        }
    }

    private void OnGatherCompleted()
    {
        _currentRock?.Mine(1);
    }

    private void OnDeliveryCompleted()
    {
        _gameState?.AddStone(1);
    }

    private void OnBuildLabourApplied()
    {
        if (_currentBuildSite is not null && GodotObject.IsInstanceValid(_currentBuildSite))
            _currentBuildSite.ContributeLabour(1);
    }
}
