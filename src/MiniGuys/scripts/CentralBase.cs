namespace MiniGuys;

using Godot;

public partial class CentralBase : Node2D
{
    private CentralBaseLogic _logic = null!;
    private ColorRect? _visual;

    [Signal]
    public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);

    public int Health => _logic.Health;
    public int MaxHealth => _logic.MaxHealth;

    public override void _Ready()
    {
        _logic = new CentralBaseLogic(maxHealth: 100);
        _logic.Damaged += OnDamaged;
        _logic.Destroyed += OnDestroyed;
        _visual = GetNode<ColorRect>("Visual");
    }

    public void TakeDamage(int amount) => _logic.TakeDamage(amount);

    private void OnDamaged(int currentHealth, int maxHealth)
    {
        EmitSignal(SignalName.HealthChanged, currentHealth, maxHealth);

        if (_visual != null && !_logic.IsDestroyed)
            _visual.Color = Colors.Red;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Keycode: Key.K })
            TakeDamage(10);
    }

    private void OnDestroyed()
    {
        if (_visual != null)
            _visual.Color = Colors.DarkRed;

        var gameState = GetNode<GameState>("/root/GameState");
        if (gameState.CurrentPhase is GamePhase.Won or GamePhase.Lost)
            return;

        gameState.TransitionTo(GamePhase.Lost);
    }
}
