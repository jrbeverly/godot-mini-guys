namespace MiniGuys;

using System;

public class StoneResourceService
{
    public int Amount { get; private set; }

    public event Action<int>? Changed;

    public void Add(int amount)
    {
        Amount += amount;
        Changed?.Invoke(Amount);
    }

    public bool TrySpend(int amount)
    {
        if (amount > Amount)
            return false;

        Amount -= amount;
        Changed?.Invoke(Amount);
        return true;
    }

    public void Reset()
    {
        Amount = 0;
    }
}
