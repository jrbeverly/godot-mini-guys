namespace MiniGuys;

using System;

public class RockLogic
{
    public int StoneRemaining { get; private set; }
    public bool IsMarked { get; private set; }

    public event Action? MarkedChanged;
    public event Action? Depleted;

    public RockLogic(int stoneAmount = 20)
    {
        StoneRemaining = stoneAmount;
    }

    public void Mark()
    {
        if (IsMarked) return;
        IsMarked = true;
        MarkedChanged?.Invoke();
    }

    public void Unmark()
    {
        if (!IsMarked) return;
        IsMarked = false;
        MarkedChanged?.Invoke();
    }

    public int Mine(int amount)
    {
        if (StoneRemaining <= 0) return 0;

        var mined = Math.Min(amount, StoneRemaining);
        StoneRemaining -= mined;

        if (StoneRemaining == 0)
        {
            Unmark();
            Depleted?.Invoke();
        }

        return mined;
    }
}
