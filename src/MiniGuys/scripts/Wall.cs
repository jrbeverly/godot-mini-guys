namespace MiniGuys;

using Godot;

public partial class Wall : Node2D
{
    private WallLogic _logic = null!;
    private ColorRect? _visual;

    [Signal]
    public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);

    public int Health => _logic.Health;
    public int MaxHealth => _logic.MaxHealth;
    public bool IsDestroyed => _logic.IsDestroyed;

    public override void _Ready()
    {
        _logic = new WallLogic(maxHealth: 50);
        _logic.Damaged += OnDamaged;
        _logic.Destroyed += OnDestroyed;
        _visual = GetNode<ColorRect>("Visual");
    }

    public void TakeDamage(int amount) => _logic.TakeDamage(amount);

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Keycode: Key.W })
            TakeDamage(10);
    }

    private void OnDamaged(int currentHealth, int maxHealth)
    {
        EmitSignal(SignalName.HealthChanged, currentHealth, maxHealth);

        if (_visual != null && !_logic.IsDestroyed)
            _visual.Color = new Color(0.7f, 0.25f, 0.15f, 1f);
    }

    private void OnDestroyed()
    {
        if (_visual != null)
            _visual.Color = new Color(0.25f, 0.25f, 0.25f, 1f);

        QueueFree();
    }
}
