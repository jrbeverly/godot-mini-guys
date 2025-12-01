namespace MiniGuys;

public class GameStateLogic
{
    public GamePhase CurrentPhase { get; private set; } = GamePhase.Setup;

    public string? LastError { get; private set; }

    public bool TryTransitionTo(GamePhase to)
    {
        var from = CurrentPhase;

        if (!IsValidTransition(from, to))
        {
            LastError = $"Invalid transition: {from} → {to}";
            return false;
        }

        CurrentPhase = to;
        LastError = null;
        return true;
    }

    public void Reset()
    {
        CurrentPhase = GamePhase.Setup;
        LastError = null;
    }

    public static bool IsValidTransition(GamePhase from, GamePhase to)
    {
        return (from, to) switch
        {
            (GamePhase.Setup, GamePhase.Preparation) => true,
            (GamePhase.Preparation, GamePhase.Wave) => true,
            (GamePhase.Wave, GamePhase.Preparation) => true,
            (GamePhase.Wave, GamePhase.Won) => true,
            (not GamePhase.Won and not GamePhase.Lost, GamePhase.Lost) => true,
            _ => false
        };
    }
}
