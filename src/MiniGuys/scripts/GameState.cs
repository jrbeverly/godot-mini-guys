namespace MiniGuys;

using Godot;

public partial class GameState : Node
{
    private readonly GameStateLogic _logic = new();
    private readonly StoneResourceService _stoneService = new();

    [Signal]
    public delegate void PhaseChangedEventHandler(GamePhase from, GamePhase to);

    [Signal]
    public delegate void WonEventHandler();

    [Signal]
    public delegate void LostEventHandler();

    [Signal]
    public delegate void StoneChangedEventHandler(int amount);

    public GamePhase CurrentPhase => _logic.CurrentPhase;

    public int Stone => _stoneService.Amount;

    public override void _Ready()
    {
        _stoneService.Changed += amount => EmitSignal(SignalName.StoneChanged, amount);
    }

    public void AddStone(int amount) => _stoneService.Add(amount);

    public bool TrySpendStone(int amount) => _stoneService.TrySpend(amount);

    public void Reset()
    {
        _logic.Reset();
        _stoneService.Reset();
    }

    public bool TransitionTo(GamePhase to)
    {
        var from = _logic.CurrentPhase;

        if (!_logic.TryTransitionTo(to))
            return false;

        EmitSignal(SignalName.PhaseChanged, (int)from, (int)to);

        if (to == GamePhase.Won)
            EmitSignal(SignalName.Won);
        else if (to == GamePhase.Lost)
            EmitSignal(SignalName.Lost);

        return true;
    }
}
