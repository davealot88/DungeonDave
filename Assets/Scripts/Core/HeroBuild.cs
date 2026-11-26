using System;

namespace DungeonDave.Core
{
    /// <summary>Turns a hero's level, weapon and talents into the numbers combat uses.</summary>
    public static class HeroBuild
    {
        public static StatBlock ComputeStats(HeroDefinition hero, int level, StatBlock gearStats, TalentState talents)
        {
            var stats = hero.baseStats.Plus(hero.perLevel.Scaled(Math.Max(0, level - 1))).Plus(gearStats);
            if (talents != null) stats.maxHealth *= 1f + talents.Total(TalentEffect.MaxHealth);
            return stats;
        }

        /// <summary>Extra damage multiplier from talents for one kind of attack (1 = no bonus).</summary>
        public static float DamageBonus(AttackKind kind, TalentState talents)
        {
            if (talents == null) return 1f;
            switch (kind)
            {
                case AttackKind.Melee: return 1f + talents.Total(TalentEffect.MeleeDamage);
                case AttackKind.Ranged: return 1f + talents.Total(TalentEffect.RangedDamage);
                default: return 1f + talents.Total(TalentEffect.SpecialDamage);
            }
        }

        /// <summary>Cooldown reduction for the special: the stat plus the talent, as a 0..1 fraction.</summary>
        public static float SpecialCooldownReduction(StatBlock stats, TalentState talents)
        {
            float talent = talents == null ? 0f : talents.Total(TalentEffect.SpecialCooldown);
            return stats.cooldownReduction + talent;
        }
    }
}
