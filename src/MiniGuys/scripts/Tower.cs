namespace MiniGuys;

using System.Collections.Generic;
using Godot;

public partial class Tower : Node2D
{
    private TowerLogic _logic = null!;
    private ColorRect? _visual;
    private Color _originalColor;
    private GameState? _gameState;
    private float _fireEffectTimer;
    private Vector2 _fireTargetOffset;

    [Signal]
    public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);

    public int Health => _logic.Health;
    public int MaxHealth => _logic.MaxHealth;
    public bool IsDestroyed => _logic.IsDestroyed;

    public override void _Ready()
    {
        _logic = new TowerLogic(maxHealth: 30);
        _logic.Damaged += OnDamaged;
        _logic.Destroyed += OnDestroyed;
        _visual = GetNode<ColorRect>("Visual");
        _originalColor = _visual.Color;

        QueueRedraw();
    }

    public void TakeDamage(int amount) => _logic.TakeDamage(amount);

    public override void _Process(double delta)
    {
        var dt = (float)delta;

        _gameState ??= GetNode<GameState>("/root/GameState");

        if (_logic.IsDestroyed)
            return;

        var phase = _gameState?.CurrentPhase;
        if (phase != GamePhase.Wave)
            return;

        _logic.TickCooldown(dt);

        if (_fireEffectTimer > 0f)
        {
            _fireEffectTimer -= dt;
            if (_visual != null)
            {
                var t = _fireEffectTimer / 0.15f;
                _visual.Color = new Color(1f, 0.9f, 0.2f, 1f).Lerp(_originalColor, 1f - t);
            }
            QueueRedraw();
        }

        if (!_logic.CanFire)
            return;

        var enemies = FindEnemiesInRange();
        if (enemies.Count == 0)
            return;

        var positions = enemies.ConvertAll(e => e.Position);
        var nearestIdx = TowerTargeting.FindNearestIndex(Position, positions);
        var target = enemies[nearestIdx];

        target.TakeDamage(_logic.AttackDamage);

        _fireEffectTimer = 0.15f;
        _fireTargetOffset = target.Position - Position;
        if (_visual != null)
            _visual.Color = new Color(1f, 0.9f, 0.2f, 1f);

        _logic.ResetCooldown();
        QueueRedraw();
    }

    public override void _Draw()
    {
        const float rangeRadius = 150f;
        DrawArc(Vector2.Zero, rangeRadius, 0, Mathf.Tau, 64,
            new Color(1f, 1f, 1f, 0.08f), 1.5f);

        if (_fireEffectTimer > 0f)
        {
            var alpha = _fireEffectTimer / 0.15f;
            DrawLine(Vector2.Zero, _fireTargetOffset, new Color(1f, 0.9f, 0.2f, alpha), 2f);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Keycode: Key.T })
            TakeDamage(5);
    }

    private List<Enemy> FindEnemiesInRange()
    {
        var result = new List<Enemy>();
        const float rangeSq = 150f * 150f;

        foreach (var child in GetParent().GetChildren())
        {
            if (child is Enemy enemy && GodotObject.IsInstanceValid(enemy) && !enemy.IsDead)
            {
                if (Position.DistanceSquaredTo(enemy.Position) <= rangeSq)
                    result.Add(enemy);
            }
        }

        return result;
    }

    private void OnDamaged(int currentHealth, int maxHealth)
    {
        EmitSignal(SignalName.HealthChanged, currentHealth, maxHealth);

        if (_visual != null && !_logic.IsDestroyed)
            _visual.Color = new Color(0.8f, 0.3f, 0.3f, 1f);
    }

    private void OnDestroyed()
    {
        if (_visual != null)
            _visual.Color = new Color(0.25f, 0.25f, 0.25f, 1f);

        QueueFree();
    }
}
