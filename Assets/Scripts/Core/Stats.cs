using System;

namespace DungeonDave.Core
{
    /// <summary>The six core stats from the GDD. Field names match the JSON data files.</summary>
    [Serializable]
    public class StatBlock
    {
        public float power;
        public float maxHealth;
        public float armor;
        public float critChance;
        public float critDamage;
        public float cooldownReduction;

        public StatBlock Clone()
        {
            return (StatBlock)MemberwiseClone();
        }

        public StatBlock Plus(StatBlock other)
        {
            if (other == null) return Clone();
            return new StatBlock
            {
                power = power + other.power,
                maxHealth = maxHealth + other.maxHealth,
                armor = armor + other.armor,
                critChance = critChance + other.critChance,
                critDamage = critDamage + other.critDamage,
                cooldownReduction = cooldownReduction + other.cooldownReduction,
            };
        }

        public StatBlock Scaled(float factor)
        {
            return new StatBlock
            {
                power = power * factor,
                maxHealth = maxHealth * factor,
                armor = armor * factor,
                critChance = critChance * factor,
                critDamage = critDamage * factor,
                cooldownReduction = cooldownReduction * factor,
            };
        }
    }

    public struct DamageResult
    {
        public int amount;
        public bool isCrit;
    }

    public static class DamageCalc
    {
        /// <summary>Fraction of damage blocked. Armor equal to armorConstant blocks half.</summary>
        public static float Mitigation(float armor, float armorConstant)
        {
            return armor <= 0f ? 0f : armor / (armor + armorConstant);
        }

        /// <summary>
        /// Damage = power x multiplier, times crit damage on a crit, reduced by the target's armor.
        /// A hit always does at least 1.
        /// </summary>
        public static DamageResult Compute(StatBlock attacker, float targetArmor, float multiplier,
            CombatDefinition combat, IRandom rng, float bonusCritChance = 0f)
        {
            float raw = attacker.power * multiplier;
            bool crit = rng.NextFloat() < attacker.critChance + bonusCritChance;
            if (crit) raw *= Math.Max(1f, attacker.critDamage);
            raw *= 1f - Mitigation(targetArmor, combat.armorConstant);
            int amount = Math.Max(1, (int)Math.Round(raw, MidpointRounding.AwayFromZero));
            return new DamageResult { amount = amount, isCrit = crit };
        }
    }
}
