namespace MiniGuys;

using Godot;

public partial class Rock : Node2D
{
    private RockLogic _logic = null!;
    private ColorRect? _visual;
    private ColorRect? _markerOverlay;
    private ColorRect? _hoverOverlay;

    [Signal]
    public delegate void DepletedEventHandler();

    [Signal]
    public delegate void StoneMinedEventHandler(int remaining);

    public int StoneRemaining => _logic.StoneRemaining;
    public bool IsMarked => _logic.IsMarked;

    public override void _Ready()
    {
        _visual = GetNode<ColorRect>("Visual");
        _markerOverlay = GetNode<ColorRect>("MarkedOverlay");
        _hoverOverlay = GetNode<ColorRect>("HoverOverlay");
        _logic = new RockLogic(stoneAmount: 20);

        _logic.MarkedChanged += UpdateMarker;
        _logic.Depleted += OnDepleted;

        var clickArea = GetNode<Area2D>("ClickArea");
        clickArea.MouseEntered += OnMouseEntered;
        clickArea.MouseExited += OnMouseExited;

        UpdateMarker();
    }

    public void Mark() => _logic.Mark();
    public void Unmark() => _logic.Unmark();

    public int Mine(int amount)
    {
        var mined = _logic.Mine(amount);
        EmitSignal(SignalName.StoneMined, _logic.StoneRemaining);
        return mined;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
        {
            var clickPos = GetGlobalMousePosition();
            if (IsPointInsideArea(clickPos))
            {
                if (_logic.IsMarked)
                    Unmark();
                else
                    Mark();
                GetViewport().SetInputAsHandled();
            }
        }
    }

    private bool IsPointInsideArea(Vector2 point)
    {
        var half = 24f;
        return point.X >= GlobalPosition.X - half
            && point.X <= GlobalPosition.X + half
            && point.Y >= GlobalPosition.Y - half
            && point.Y <= GlobalPosition.Y + half;
    }

    private void OnMouseEntered()
    {
        if (_hoverOverlay != null)
            _hoverOverlay.Visible = true;
    }

    private void OnMouseExited()
    {
        if (_hoverOverlay != null)
            _hoverOverlay.Visible = false;
    }

    private void UpdateMarker()
    {
        if (_markerOverlay != null)
            _markerOverlay.Visible = _logic.IsMarked;
    }

    private void OnDepleted()
    {
        EmitSignal(SignalName.Depleted);
        if (_visual != null)
            _visual.Color = new Color(0.2f, 0.2f, 0.2f, 1f);
    }
}
