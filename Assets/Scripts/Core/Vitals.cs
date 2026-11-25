using System;

namespace DungeonDave.Core
{
    public sealed class Health
    {
        public int Max { get; private set; }
        public int Current { get; private set; }
        public bool IsDead { get { return Current <= 0; } }
        public float Fraction { get { return Max <= 0 ? 0f : (float)Current / Max; } }

        public Health(int max)
        {
            Max = Math.Max(1, max);
            Current = Max;
        }

        /// <summary>Returns the damage actually applied.</summary>
        public int TakeDamage(int amount)
        {
            if (amount <= 0 || IsDead) return 0;
            int applied = Math.Min(amount, Current);
            Current -= applied;
            return applied;
        }

        public int Heal(int amount)
        {
            if (amount <= 0 || IsDead) return 0;
            int applied = Math.Min(amount, Max - Current);
            Current += applied;
            return applied;
        }

        /// <summary>Changes max health (level up, new gear) and keeps the same health fraction.</summary>
        public void SetMax(int newMax)
        {
            newMax = Math.Max(1, newMax);
            float fraction = Fraction;
            Max = newMax;
            Current = IsDeadFraction(fraction) ? 0 : Math.Max(1, (int)Math.Round(newMax * fraction, MidpointRounding.AwayFromZero));
        }

        private static bool IsDeadFraction(float fraction)
        {
            return fraction <= 0f;
        }
    }

    public sealed class Stamina
    {
        private readonly StaminaDefinition def;
        private float sinceSpend;

        public float Current { get; private set; }
        public float Max { get { return def.max; } }
        public float Fraction { get { return def.max <= 0f ? 0f : Current / def.max; } }

        public Stamina(StaminaDefinition def)
        {
            this.def = def;
            Current = def.max;
            sinceSpend = def.regenDelay;
        }

        public bool CanSpend(float cost)
        {
            return Current >= cost;
        }

        public bool TrySpend(float cost)
        {
            if (Current < cost) return false;
            Current -= cost;
            sinceSpend = 0f;
            return true;
        }

        public void Tick(float dt)
        {
            sinceSpend += dt;
            if (sinceSpend < def.regenDelay) return;
            // Only the part of this tick after the delay regenerates, so frame rate does not matter.
            float regenTime = Math.Min(dt, sinceSpend - def.regenDelay);
            Current = Math.Min(def.max, Current + def.regenPerSecond * regenTime);
        }
    }

    public sealed class Cooldown
    {
        public float Duration { get; private set; }
        public float Remaining { get; private set; }
        public bool IsReady { get { return Remaining <= 0f; } }
        public float Fraction { get { return Duration <= 0f ? 0f : Remaining / Duration; } }

        /// <summary>
        /// Starts the cooldown if it is ready. Reduction is a 0..1 fraction (stat plus talents),
        /// capped at maxReduction (combat.json).
        /// </summary>
        public bool TryStart(float baseDuration, float reduction, float maxReduction)
        {
            if (!IsReady) return false;
            if (float.IsNaN(reduction)) reduction = 0f;
            float clamped = Math.Max(0f, Math.Min(maxReduction, reduction));
            Duration = baseDuration * (1f - clamped);
            Remaining = Duration;
            return true;
        }

        public void Tick(float dt)
        {
            if (Remaining > 0f) Remaining = Math.Max(0f, Remaining - dt);
        }
    }

    /// <summary>
    /// Poise is how much punishment an enemy soaks before it staggers. Hits chip it down;
    /// it refills if the enemy is left alone for a moment, and resets after a stagger.
    /// </summary>
    public sealed class PoiseMeter
    {
        private readonly float max;
        private readonly float recoverDelay;
        private float sinceHit;

        public float Current { get; private set; }

        public PoiseMeter(float max, float recoverDelay)
        {
            this.max = max;
            this.recoverDelay = recoverDelay;
            Current = max;
        }

        /// <summary>Returns true when this hit breaks poise and the target should stagger.</summary>
        public bool ApplyHit(float poiseDamage)
        {
            if (poiseDamage <= 0f) return false;
            if (max <= 0f) return true;
            sinceHit = 0f;
            Current -= poiseDamage;
            if (Current > 0f) return false;
            Current = max;
            return true;
        }

        public void Tick(float dt)
        {
            if (Current >= max) return;
            sinceHit += dt;
            if (sinceHit >= recoverDelay) Current = max;
        }
    }
}
