namespace MiniGuys;

using Godot;

public partial class Enemy : Node2D
{
    private int _maxHealth = 30;
    private int _damage = 10;
    private float _speed = 80f;

    private EnemyLogic _logic = null!;
    private ColorRect? _visual;
    private CentralBase? _targetBase;
    private Wall? _attackingWall;

    public void Configure(int maxHealth, int damage, float speed)
    {
        _maxHealth = maxHealth;
        _damage = damage;
        _speed = speed;
    }

    public void SetTarget(CentralBase target) => _targetBase = target;

    public static Enemy Spawn(Vector2 at)
    {
        var scene = GD.Load<PackedScene>("res://scenes/Enemy.tscn");
        var enemy = scene.Instantiate<Enemy>();
        enemy.Position = at;
        return enemy;
    }

    public override void _Ready()
    {
        _logic = new EnemyLogic(maxHealth: _maxHealth, damage: _damage, speed: _speed);
        _logic.Killed += OnKilled;
        _visual = GetNode<ColorRect>("Visual");
    }

    public override void _Process(double delta)
    {
        if (_logic.IsDead || _targetBase == null)
            return;

        var dt = (float)delta;

        if (_logic.IsAttackingWall)
        {
            ProcessAttackingWall(dt);
            return;
        }

        var dir = (_targetBase.Position - Position).Normalized();
        Position += dir * _logic.Speed * dt;

        if (Position.DistanceTo(_targetBase.Position) < 20f)
        {
            _targetBase.TakeDamage(_logic.Damage);
            QueueFree();
        }

        var wall = FindWallInPath();
        if (wall != null)
            BeginAttackingWall(wall);
    }

    public bool IsDead => _logic.IsDead;

    public void TakeDamage(int amount) => _logic.TakeDamage(amount);

    private void ProcessAttackingWall(float dt)
    {
        _logic.TickAttackCooldown(dt);

        if (!GodotObject.IsInstanceValid(_attackingWall) || _attackingWall!.IsDestroyed)
        {
            StopAttackingWall();
            return;
        }

        if (_logic.CanAttackWall)
        {
            _attackingWall.TakeDamage(_logic.Damage);
            _logic.ResetAttackCooldown();
        }
    }

    private Wall? FindWallInPath()
    {
        const float collisionThresholdSq = 30f * 30f;

        foreach (var child in GetParent().GetChildren())
        {
            if (child is Wall wall && GodotObject.IsInstanceValid(wall) && !wall.IsDestroyed)
            {
                if (Position.DistanceSquaredTo(wall.Position) <= collisionThresholdSq)
                    return wall;
            }
        }

        return null;
    }

    private void BeginAttackingWall(Wall wall)
    {
        _attackingWall = wall;
        _logic.BeginAttackingWall();
    }

    private void StopAttackingWall()
    {
        _attackingWall = null;
        _logic.StopAttackingWall();
    }

    private void OnKilled() => QueueFree();
}
