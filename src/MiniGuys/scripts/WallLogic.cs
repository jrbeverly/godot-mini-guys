namespace MiniGuys;

using System;

public class WallLogic
{
    public int MaxHealth { get; }
    public int Health { get; private set; }
    public bool IsDestroyed { get; private set; }

    public event Action<int, int>? Damaged;
    public event Action? Destroyed;

    public WallLogic(int maxHealth = 50)
    {
        MaxHealth = maxHealth;
        Health = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (IsDestroyed)
            return;

        Health -= amount;
        if (Health <= 0)
        {
            Health = 0;
            IsDestroyed = true;
        }

        Damaged?.Invoke(Health, MaxHealth);

        if (IsDestroyed)
            Destroyed?.Invoke();
    }
}
