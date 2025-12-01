namespace MiniGuys;

using System;

public class EnemyLogic
{
    public int MaxHealth { get; }
    public int Health { get; private set; }
    public int Damage { get; }
    public float Speed { get; }
    public bool IsDead { get; private set; }

    public bool IsAttackingWall { get; private set; }
    public float AttackCooldown { get; }
    public float CooldownRemaining { get; private set; }
    public bool CanAttackWall => IsAttackingWall && CooldownRemaining <= 0f;

    public event Action? Killed;

    public EnemyLogic(int maxHealth, int damage, float speed, float attackCooldown = 1f)
    {
        MaxHealth = maxHealth;
        Health = maxHealth;
        Damage = damage;
        Speed = speed;
        AttackCooldown = attackCooldown;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead)
            return;

        Health -= amount;
        if (Health <= 0)
        {
            Health = 0;
            IsDead = true;
            Killed?.Invoke();
        }
    }

    public void BeginAttackingWall()
    {
        IsAttackingWall = true;
        CooldownRemaining = AttackCooldown;
    }

    public void StopAttackingWall()
    {
        IsAttackingWall = false;
        CooldownRemaining = 0f;
    }

    public void TickAttackCooldown(float delta)
    {
        if (!IsAttackingWall)
            return;

        if (CooldownRemaining > 0f)
        {
            CooldownRemaining -= delta;
            if (CooldownRemaining < 0f)
                CooldownRemaining = 0f;
        }
    }

    public void ResetAttackCooldown()
    {
        CooldownRemaining = AttackCooldown;
    }
}
