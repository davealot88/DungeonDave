using System;

namespace DungeonDave.Core
{
    /// <summary>
    /// The melee combo. Tap to swing; tap again during a swing (buffered) or in the short window
    /// after it to chain the next hit. Miss the window and the combo starts over.
    /// </summary>
    public sealed class ComboTracker
    {
        private readonly ComboDefinition def;
        private int step;          // 0 = idle, otherwise the current or last finished step (1-based)
        private float swingLeft;
        private float windowLeft;
        private bool buffered;

        public ComboTracker(ComboDefinition def)
        {
            this.def = def;
        }

        public bool IsSwinging { get { return swingLeft > 0f; } }
        /// <summary>Step being swung right now (1-based), or 0 when not swinging.</summary>
        public int CurrentStep { get { return IsSwinging ? step : 0; } }
        public int StepCount { get { return def.steps.Length; } }

        public ComboStepDefinition GetStep(int oneBasedStep)
        {
            return def.steps[oneBasedStep - 1];
        }

        /// <summary>Melee button tapped. Returns the step that started (1-based), or 0 if none started now.</summary>
        public int Press()
        {
            if (IsSwinging)
            {
                if (step < def.steps.Length) buffered = true;
                return 0;
            }
            if (windowLeft > 0f && step < def.steps.Length) return Start(step + 1);
            return Start(1);
        }

        /// <summary>Advances time. Returns a step that started from a buffered tap (1-based), or 0.</summary>
        public int Tick(float dt)
        {
            if (swingLeft > 0f)
            {
                swingLeft -= dt;
                if (swingLeft > 0f) return 0;

                float overflow = -swingLeft;
                swingLeft = 0f;
                if (step >= def.steps.Length)
                {
                    Reset();
                    return 0;
                }
                if (buffered) return Start(step + 1);
                windowLeft = def.chainWindow - overflow;
                if (windowLeft <= 0f) Reset();
                return 0;
            }

            if (windowLeft > 0f)
            {
                windowLeft -= dt;
                if (windowLeft <= 0f) Reset();
            }
            return 0;
        }

        /// <summary>Drops the combo, for example when the hero dodges or is knocked down.</summary>
        public void Reset()
        {
            step = 0;
            swingLeft = 0f;
            windowLeft = 0f;
            buffered = false;
        }

        private int Start(int newStep)
        {
            step = newStep;
            swingLeft = def.steps[newStep - 1].duration;
            windowLeft = 0f;
            buffered = false;
            return newStep;
        }
    }

    /// <summary>The dodge: a short dash that costs stamina and ignores hits for its first moments.</summary>
    public sealed class DodgeState
    {
        private readonly DodgeDefinition def;
        private float timeLeft;
        private float invulnerableLeft;

        public DodgeState(DodgeDefinition def)
        {
            this.def = def;
        }

        public bool IsDodging { get { return timeLeft > 0f; } }
        public bool IsInvulnerable { get { return invulnerableLeft > 0f; } }
        /// <summary>Dash speed that covers the configured distance over the dodge's duration.</summary>
        public float Speed { get { return def.duration <= 0f ? 0f : def.distance / def.duration; } }

        public bool TryStart(Stamina stamina)
        {
            if (IsDodging) return false;
            if (!stamina.TrySpend(def.staminaCost)) return false;
            timeLeft = def.duration;
            invulnerableLeft = def.invulnerableTime;
            return true;
        }

        public void Tick(float dt)
        {
            if (timeLeft > 0f) timeLeft = Math.Max(0f, timeLeft - dt);
            if (invulnerableLeft > 0f) invulnerableLeft = Math.Max(0f, invulnerableLeft - dt);
        }
    }

    public static class ChargeShot
    {
        /// <summary>0 for a tap, 1 for a full charge.</summary>
        public static float ChargeFraction(RangedDefinition def, float heldSeconds)
        {
            if (def.maxChargeTime <= 0f) return 1f;
            return Math.Max(0f, Math.Min(1f, heldSeconds / def.maxChargeTime));
        }

        /// <summary>Damage multiplier for a shot held this long: tap = min, full charge = max.</summary>
        public static float Multiplier(RangedDefinition def, float heldSeconds)
        {
            float t = ChargeFraction(def, heldSeconds);
            return def.minMultiplier + (def.maxMultiplier - def.minMultiplier) * t;
        }
    }

    /// <summary>Anything that can be hit: the hero or an enemy.</summary>
    public sealed class Combatant
    {
        public StatBlock Stats;
        public Health Health;
        public PoiseMeter Poise;   // may be null (the hero does not stagger in the slice)
        public DodgeState Dodge;   // may be null (enemies do not dodge)

        /// <summary>statScale (difficulty) raises health and power; armor stays as authored.</summary>
        public static Combatant FromEnemy(EnemyDefinition def, float statScale = 1f)
        {
            return new Combatant
            {
                Stats = new StatBlock
                {
                    power = def.power * statScale,
                    maxHealth = def.maxHealth * statScale,
                    armor = def.armor,
                    critDamage = 1f,
                },
                Health = new Health((int)Math.Round(def.maxHealth * statScale, MidpointRounding.AwayFromZero)),
                Poise = new PoiseMeter(def.poise, def.poiseRecoverDelay),
            };
        }
    }

    public struct HitOutcome
    {
        public bool landed;
        public bool dodged;
        public int damage;
        public bool isCrit;
        public bool staggered;
        public bool killed;
    }

    public static class HitResolver
    {
        public static HitOutcome Resolve(StatBlock attacker, float damageMultiplier, float poiseDamage,
            Combatant target, CombatDefinition combat, IRandom rng, float bonusCritChance = 0f)
        {
            var outcome = new HitOutcome();
            if (target.Health.IsDead) return outcome;
            if (target.Dodge != null && target.Dodge.IsInvulnerable)
            {
                outcome.dodged = true;
                return outcome;
            }

            var result = DamageCalc.Compute(attacker, target.Stats.armor, damageMultiplier, combat, rng, bonusCritChance);
            outcome.landed = true;
            outcome.damage = target.Health.TakeDamage(result.amount);
            outcome.isCrit = result.isCrit;
            outcome.killed = target.Health.IsDead;
            if (!outcome.killed && target.Poise != null) outcome.staggered = target.Poise.ApplyHit(poiseDamage);
            return outcome;
        }
    }
}
