using System;
using System.Collections.Generic;

namespace DungeonDave.Core
{
    public enum TalentEffect
    {
        None,
        MeleeDamage,      // +fraction to melee damage
        RangedDamage,     // +fraction to ranged damage
        SpecialDamage,    // +fraction to special damage
        SpecialCooldown,  // -fraction off the special's cooldown
        MaxHealth,        // +fraction to max health
    }

    public static class TalentEffects
    {
        /// <summary>Reads an effect name from data. Unknown names and bare numbers give None.</summary>
        public static TalentEffect Parse(string name)
        {
            if (string.IsNullOrEmpty(name) || char.IsDigit(name[0]) || name[0] == '-' || name[0] == '+')
                return TalentEffect.None;
            TalentEffect parsed;
            if (!Enum.TryParse(name, true, out parsed) || !Enum.IsDefined(typeof(TalentEffect), parsed))
                return TalentEffect.None;
            return parsed;
        }
    }

    public static class LevelCurve
    {
        /// <summary>XP needed to go from this level to the next. 0 at the level cap.</summary>
        public static int XpToNext(ProgressionDefinition def, int level)
        {
            if (level >= def.levelCap) return 0;
            return Math.Max(1, (int)Math.Round(def.xpBase * Math.Pow(level, def.xpExponent), MidpointRounding.AwayFromZero));
        }
    }

    /// <summary>One hero's level and XP. Each hero levels separately.</summary>
    public sealed class HeroProgress
    {
        public int Level { get; private set; }
        /// <summary>XP earned toward the next level.</summary>
        public int Xp { get; private set; }

        public HeroProgress(int level = 1, int xp = 0)
        {
            Level = Math.Max(1, level);
            Xp = Math.Max(0, xp);
        }

        /// <summary>
        /// Makes loaded save data legal: level within the cap, leftover XP rolled into levels.
        /// Call once after loading.
        /// </summary>
        public void Normalize(ProgressionDefinition def)
        {
            if (Level > def.levelCap) Level = def.levelCap;
            if (Level >= def.levelCap) { Xp = 0; return; }
            int carried = Xp;
            Xp = 0;
            AddXp(carried, def);
        }

        /// <summary>Adds XP and returns how many levels were gained.</summary>
        public int AddXp(int amount, ProgressionDefinition def)
        {
            if (amount <= 0 || Level >= def.levelCap) return 0;
            int gained = 0;
            Xp += amount;
            while (Level < def.levelCap)
            {
                int needed = LevelCurve.XpToNext(def, Level);
                if (Xp < needed) break;
                Xp -= needed;
                Level++;
                gained++;
            }
            if (Level >= def.levelCap) Xp = 0;
            return gained;
        }
    }

    /// <summary>Which talents a hero has bought. One point per level from level 2.</summary>
    public sealed class TalentState
    {
        private readonly ProgressionDefinition def;
        private readonly Dictionary<string, int> ranks = new Dictionary<string, int>();

        public TalentState(ProgressionDefinition def)
        {
            this.def = def;
        }

        public int SpentPoints { get; private set; }

        public static int EarnedPoints(int heroLevel)
        {
            return Math.Max(0, heroLevel - 1);
        }

        public int AvailablePoints(int heroLevel)
        {
            return EarnedPoints(heroLevel) - SpentPoints;
        }

        public int GetRank(string talentId)
        {
            int rank;
            return ranks.TryGetValue(talentId, out rank) ? rank : 0;
        }

        public bool TrySpend(string talentId, int heroLevel)
        {
            var talent = def.FindTalent(talentId);
            if (talent == null) return false;
            if (AvailablePoints(heroLevel) <= 0) return false;
            int rank = GetRank(talentId);
            if (rank >= talent.maxRank) return false;
            ranks[talentId] = rank + 1;
            SpentPoints++;
            return true;
        }

        /// <summary>Refunds every point for gold. Returns false if the hero cannot afford it.</summary>
        public bool TryReset(Wallet wallet)
        {
            if (SpentPoints == 0) return false;
            if (!wallet.TrySpend(def.talentResetGold, 0)) return false;
            ranks.Clear();
            SpentPoints = 0;
            return true;
        }

        /// <summary>
        /// Restores a saved rank (loading a save). It does not check points; after loading,
        /// call <see cref="IsLegalFor"/> and reset the talents if a save claims more than the level allows.
        /// </summary>
        public void SetRank(string talentId, int rank)
        {
            var talent = def.FindTalent(talentId);
            if (talent == null) return;
            rank = Math.Max(0, Math.Min(talent.maxRank, rank));
            SpentPoints += rank - GetRank(talentId);
            if (rank == 0) ranks.Remove(talentId); else ranks[talentId] = rank;
        }

        /// <summary>True when the spent points fit what a hero of this level has earned.</summary>
        public bool IsLegalFor(int heroLevel)
        {
            return SpentPoints <= EarnedPoints(heroLevel);
        }

        /// <summary>Clears every rank without charging gold (used when a loaded save is not legal).</summary>
        public void Clear()
        {
            ranks.Clear();
            SpentPoints = 0;
        }

        /// <summary>Sum of every bought talent's value for one effect.</summary>
        public float Total(TalentEffect effect)
        {
            float total = 0f;
            foreach (var talent in def.talents)
            {
                int rank = GetRank(talent.id);
                if (rank == 0) continue;
                if (TalentEffects.Parse(talent.effect) == effect)
                    total += talent.valuePerRank * rank;
            }
            return total;
        }
    }
}
