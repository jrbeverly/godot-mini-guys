namespace MiniGuys;

using System;

public class TowerLogic
{
    public int MaxHealth { get; }
    public int Health { get; private set; }
    public bool IsDestroyed { get; private set; }

    public int AttackDamage { get; }
    public float AttackCooldown { get; }
    public float CooldownRemaining { get; private set; }
    public bool CanFire => !IsDestroyed && CooldownRemaining <= 0f;

    public event Action<int, int>? Damaged;
    public event Action? Destroyed;
    public event Action? Fired;

    public TowerLogic(int maxHealth = 30, int attackDamage = 10, float attackCooldown = 1f)
    {
        MaxHealth = maxHealth;
        Health = maxHealth;
        AttackDamage = attackDamage;
        AttackCooldown = attackCooldown;
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

    public void TickCooldown(float delta)
    {
        if (IsDestroyed)
            return;

        if (CooldownRemaining > 0f)
        {
            CooldownRemaining -= delta;
            if (CooldownRemaining < 0f)
                CooldownRemaining = 0f;
        }
    }

    public void ResetCooldown()
    {
        CooldownRemaining = AttackCooldown;
        Fired?.Invoke();
    }
}
